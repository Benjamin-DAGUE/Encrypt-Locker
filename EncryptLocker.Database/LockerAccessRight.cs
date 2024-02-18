namespace EncryptLocker.Database;

public class LockerAccessRight
{
    #region Properties

    public int Id { get; set; }
    public int LockerId { get; set; }
    public int RegisteredUserId { get; set; }
    public AccessType AccessType { get; set; }

    #region Navigations

    public required Locker Locker { get; set; }
    public required RegisteredUser RegisteredUser { get; set; }

    #endregion

    #endregion
}
