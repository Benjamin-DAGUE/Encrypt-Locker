using EncryptLocker.Website.Models;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;

namespace EncryptLocker.Website.Services;

public class LockerDataService(LockerSvc.LockerSvcClient client)
{
    private class LockerStream
    {
        public required AsyncDuplexStreamingCall<LockerStreamRequest, LockerStreamReply> Stream { get; set; }
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
    }

    private readonly ConcurrentDictionary<int, LockerStream> _streams = [];

    public async Task<Locker?> GetLockerAsync(int id)
    {
        Locker? result = null;

        if (id > 0)
        {
            result = (await GetLockersAsync(id)).FirstOrDefault();

        }

        return result;
    }

    public async Task<List<Locker>> GetLockersAsync() => await GetLockersAsync(null);

    private async Task<List<Locker>> GetLockersAsync(int? id)
    {
        List<Locker> results = [];

        try
        {
            GetLockersReply reply = await client.GetLockersAsync(new()
            {
                Id = id ?? 0,
            });

            foreach (LockerMessage lockerMessage in reply.Lockers)
            {
                results.Add(new Locker()
                {
                    Id = lockerMessage.Id,
                    Name = lockerMessage.Name,
                });
            }
        }
        catch (Exception)
        {

        }

        return results;
    }

    public async Task<Locker> AddLockerAsync(Locker locker, byte[] passwordHash)
    {
        try
        {
            LockerMessage lockerMessage = await client.AddLockerAsync(new()
            {
                Name = locker.Name,
                PasswordHash = ByteString.CopyFrom(passwordHash)
            });

            locker.Id = lockerMessage.Id;
        }
        catch
        {

        }

        return locker;
    }

    public async Task<ResultMessage> DeleteLockerAsync(Locker locker, byte[] passwordHash)
    {
        ResultMessage result = new()
        {
            IsSuccess = false,
            Message = "No response from server"
        };

        try
        {
            if (_streams.TryGetValue(locker.Id, out LockerStream? lockerStream))
            {
                await lockerStream.Semaphore.WaitAsync();

                try
                {
                    LockerStreamRequest lockerStreamRequest = new()
                    {
                        LockerId = locker.Id,
                        DeleteLocker = new()
                        {
                            PasswordHash = ByteString.CopyFrom(passwordHash)
                        }
                    };

                    await lockerStream.Stream.RequestStream.WriteAsync(lockerStreamRequest);

                    if (await lockerStream.Stream.ResponseStream.MoveNext() == false)
                    {

                    }
                    if (lockerStream.Stream.ResponseStream.Current.TypeCase != LockerStreamReply.TypeOneofCase.DeleteLocker)
                    {

                        result.Message = "Bad response from server";
                    }
                    if (lockerStream.Stream.ResponseStream.Current.Result.IsSuccess == false)
                    {
                        result.IsSuccess = false;
                        result.Message = lockerStream.Stream.ResponseStream.Current.Result.Message;
                    }

                    result.IsSuccess = true;
                }
                finally
                {
                    lockerStream.Semaphore.Release();
                }
            }
        }
        catch
        {

        }
        return result;
    }

    public async Task<List<SafeEntry>> LockerStreamAsync(Locker locker, byte[] passwordHash)
    {
        List<SafeEntry> results = [];

        if (_streams.ContainsKey(locker.Id) == false)
        {
            LockerStream? lockerStream = new()
            {
                Stream = client.LockerStream()
            };

            if (_streams.TryAdd(locker.Id, lockerStream) == false)
            {
                lockerStream.Stream.Dispose();
                _streams.TryGetValue(locker.Id, out lockerStream);
                if (lockerStream == null)
                {
                    //TODO : Failure
                    throw new Exception();
                }
            }

            await lockerStream.Semaphore.WaitAsync();
            try
            {
                LockerStreamRequest lockerStreamRequest = new()
                {
                    LockerId = locker.Id,
                    OpenLocker = new()
                    {
                        PasswordHash = ByteString.CopyFrom(passwordHash)
                    }
                };

                await lockerStream.Stream.RequestStream.WriteAsync(lockerStreamRequest);

                if (await lockerStream.Stream.ResponseStream.MoveNext() == false)
                {
                    //TODO : Failure
                    throw new Exception();
                }
                if (lockerStream.Stream.ResponseStream.Current.TypeCase != LockerStreamReply.TypeOneofCase.OpenLocker)
                {
                    //TODO : Failure
                    throw new Exception();
                }
                if (lockerStream.Stream.ResponseStream.Current.Result.IsSuccess == false)
                {
                    //TODO : Failure
                    throw new Exception();
                }

                results.AddRange(lockerStream.Stream.ResponseStream.Current.OpenLocker.SafeEntries.Select(s => new SafeEntry()
                {
                    Id = s.Id,
                }));
            }
            catch (Exception)
            {
                if(_streams.TryRemove(locker.Id, out lockerStream))
                {
                    lockerStream.Semaphore.Dispose();
                    lockerStream.Stream.Dispose();
                    lockerStream = null;
                }
            }
            finally
            {
                lockerStream?.Semaphore?.Release();
            }
        }

        //TODO : RefreshEntries from server
        return results;
    }

    public bool IsLockerOpened(Locker locker) => _streams.ContainsKey(locker.Id);
}
