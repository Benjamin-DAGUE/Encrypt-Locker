using EncryptLocker.Database;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EncryptLocker.DataService.Services;

public class LockerService(ILogger<LockerService> logger, IDbContextFactory<EncryptLockerContext> dbContextFactory) : LockerSvc.LockerSvcBase
{
    public override async Task<GetLockersReply> GetAvailableLockers(Empty request, ServerCallContext context)
    {
        logger.LogTrace("Request available lockers");

        using EncryptLockerContext dbContext = await dbContextFactory.CreateDbContextAsync();

        GetLockersReply reply = new();

        //TODO : Filter lockers by user ID
        reply.Lockers.AddRange(
            (
                await dbContext.Lockers.ToListAsync()
            )
            .Select(l => new LockerMessage()
            {
                Id = l.Id,
                Name = l.Name,
            })
            .ToList());

        return reply;
    }

    public override async Task<Empty> AddLocker(AddLockerRequest request, ServerCallContext context)
    {
        logger.LogTrace("Request add locker");

        using EncryptLockerContext dbContext = await dbContextFactory.CreateDbContextAsync();

        //TODO : Add requester as Admin
        await dbContext.Lockers.AddAsync(new Locker()
        {
            Name = request.Name,
            KeyHash = request.KeyHash.ToByteArray()
        });

        await dbContext.SaveChangesAsync();

        return new();
    }

    public override async Task<DeleteLockerReply> DeleteLocker(DeleteLockerRequest request, ServerCallContext context)
    {
        logger.LogTrace("Request delete locker");

        using EncryptLockerContext dbContext = await dbContextFactory.CreateDbContextAsync();
        IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync();

        bool isDeleted = false;
        string message = string.Empty;

        try
        {
            //TODO : Check requester is a locker admin
            Locker? locker = await dbContext.Lockers.FirstOrDefaultAsync(l => l.Id == request.Id);

            if (locker != null)
            {
                if (locker.KeyHash.SequenceEqual(request.KeyHash.ToByteArray()))
                {
                    dbContext.Lockers.Remove(locker);
                    dbContext.SaveChanges();
                    transaction.Commit();
                    isDeleted = true;
                    message = "Locker deleted";
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
            try { transaction.Rollback(); } catch { }
            isDeleted = false;
            message = "Internal error";
        }

        return new()
        {
            IsDeleted = isDeleted,
            Message = message
        };
    }
}
