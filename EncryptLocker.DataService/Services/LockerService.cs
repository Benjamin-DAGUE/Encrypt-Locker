using Azure.Core;
using EncryptLocker.Database;
using EncryptLocker.DataService.Exceptions;
using EncryptLocker.Messages;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;

namespace EncryptLocker.DataService.Services;

public class LockerService(ILogger<LockerService> logger, IDbContextFactory<EncryptLockerContext> dbContextFactory) : LockerSvc.LockerSvcBase
{
    public override async Task LockerStream(IAsyncStreamReader<LockerStreamRequest> requestStream, IServerStreamWriter<LockerStreamReply> responseStream, ServerCallContext context)
    {
        try
        {
            ConcurrentDictionary<int, bool> openedLockers = [];

            await foreach (LockerStreamRequest request in requestStream.ReadAllAsync())
            {
                if (request.TypeCase != LockerStreamRequest.TypeOneofCase.OpenLocker && openedLockers.ContainsKey(request.LockerId) == false)
                {
                    await responseStream.WriteAsync(new()
                    {
                        Result = new()
                        {
                            IsSuccess = false,
                            Message = "Locker must be opened"
                        }
                    });
                }
                else
                {
                    switch (request.TypeCase)
                    {
                        case LockerStreamRequest.TypeOneofCase.GetLockerState:
                            await GetLockerState(request.LockerId, openedLockers, responseStream, context);
                            break;
                        case LockerStreamRequest.TypeOneofCase.OpenLocker:
                            await OpenLocker(request.LockerId, request.OpenLocker, openedLockers, responseStream, context);
                            break;
                        case LockerStreamRequest.TypeOneofCase.DeleteLocker:
                            await DeleteLocker(request.LockerId, request.DeleteLocker, openedLockers, responseStream, context);
                            break;
                        case LockerStreamRequest.TypeOneofCase.GetCypherPassword:

                            break;
                        default:
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error:{nl}{ex}", Environment.NewLine, ex);
            throw new InternalServerException(ex);
        }
    }

    #region Locker

    public override async Task<GetLockersReply> GetLockers(GetLockersRequest request, ServerCallContext context)
    {
        try
        {
            logger.LogTrace("Request available lockers");
            using EncryptLockerContext dbContext = await dbContextFactory.CreateDbContextAsync();

            GetLockersReply reply = new();

            reply.Lockers.AddRange(
                (
                    //TODO : Filter lockers by user ID
                    await dbContext.Lockers
                        .Where(l => l.Id == request.Id || request.Id == 0)
                        .ToListAsync()
                )
                .Select(l => new LockerMessage()
                {
                    Id = l.Id,
                    Name = l.Name,
                })
                .ToList());

            return reply;
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error:{nl}{ex}", Environment.NewLine, ex);
            throw new InternalServerException(ex);
        }
    }

    public override async Task<LockerMessage> AddLocker(AddLockerRequest request, ServerCallContext context)
    {
        try
        {
            logger.LogTrace("Request add locker");

            using EncryptLockerContext dbContext = await dbContextFactory.CreateDbContextAsync();

            //TODO : Add requester as Admin
            Locker locker = new()
            {
                Name = request.Name,
                KeyHash = request.PasswordHash.ToByteArray()
            };

            await dbContext.Lockers.AddAsync(locker);

            await dbContext.SaveChangesAsync();

            return new()
            {
                Id = locker.Id,
                Name = locker.Name
            };
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error:{nl}{ex}", Environment.NewLine, ex);
            throw new InternalServerException(ex);
        }
    }

    private static async Task GetLockerState(int lockerId, ConcurrentDictionary<int, bool> openedLockers, IServerStreamWriter<LockerStreamReply> responseStream, ServerCallContext context)
    {
        bool isOpened = openedLockers.ContainsKey(lockerId) == false;

        await responseStream.WriteAsync(new()
        {
            Result = new()
            {
                IsSuccess = true
            },
            GetLockerState = new()
            {
                IsOpen = isOpened
            }
        });
    }

    private async Task OpenLocker(int lockerId, OpenLockerRequest request, ConcurrentDictionary<int, bool> openedLockers, IServerStreamWriter<LockerStreamReply> responseStream, ServerCallContext context)
    {
        bool isOpened = false;
        string message = string.Empty;

        if (openedLockers.ContainsKey(lockerId) == false)
        {
            using EncryptLockerContext dbContext = await dbContextFactory.CreateDbContextAsync();

            Locker? locker = await dbContext.Lockers.FirstOrDefaultAsync(l => l.Id == lockerId);

            if (locker != null)
            {
                if (locker.KeyHash.SequenceEqual(request.PasswordHash.ToByteArray()))
                {
                    openedLockers.TryAdd(lockerId, true);
                    isOpened = true;
                    message = "Locker successfuly opened.";
                }
                else
                {
                    message = "Wrong password";
                }
            }
            else
            {
                message = "Locker not found";
            }
        }
        else
        {
            isOpened = true;
            message = "Locker was previously opened";
        }

        await responseStream.WriteAsync(new LockerStreamReply()
        {
            Result = new()
            {
                IsSuccess = isOpened,
                Message = message
            },
            OpenLocker = new()
        });
    }

    private async Task DeleteLocker(int lockerId, DeleteLockerRequest request, ConcurrentDictionary<int, bool> openedLockers, IServerStreamWriter<LockerStreamReply> responseStream, ServerCallContext context)
    {
        logger.LogTrace("Request delete locker");

        using EncryptLockerContext dbContext = await dbContextFactory.CreateDbContextAsync();
        IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync();

        bool isDeleted = false;
        string message = string.Empty;

        try
        {
            //TODO : Check requester is a locker admin
            Locker? locker = await dbContext.Lockers.FirstOrDefaultAsync(l => l.Id == lockerId);

            if (locker != null)
            {
                if (locker.KeyHash.SequenceEqual(request.PasswordHash.ToByteArray()))
                {
                    dbContext.Lockers.Remove(locker);
                    dbContext.SaveChanges();
                    transaction.Commit();
                    isDeleted = true;
                    message = "Locker successfuly deleted";
                    openedLockers.TryRemove(lockerId, out _);
                }
                else
                {
                    transaction.Rollback();
                    message = "Wrong password";
                    logger.LogTrace("Wrong locker key provided");
                }
            }
            else
            {
                transaction.Rollback();
                message = "Locker not found";
                logger.LogTrace("Unable to find locker with id for user.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error:{nl}{ex}", Environment.NewLine, ex);
            try { transaction.Rollback(); } catch { }
            isDeleted = false;
            message = "Internal error";
        }

        await responseStream.WriteAsync(new LockerStreamReply()
        {
            Result = new()
            {
                IsSuccess = isDeleted,
                Message = message
            },
            DeleteLocker = new()
        });
    }

    #endregion

    #region AccessRight

    #endregion

    #region SafeEntry

    #endregion


}
