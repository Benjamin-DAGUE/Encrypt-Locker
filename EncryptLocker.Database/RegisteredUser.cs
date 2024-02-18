namespace EncryptLocker.Database;

public class RegisteredUser
{
    #region Properties

    public int Id { get; set; }
    public Guid AzureID { get; set; }

    #region Navigations

    public IEnumerable<LockerAccessRight> LockerAccessRights { get; set; } = new HashSet<LockerAccessRight>();
    public IEnumerable<PasswordReadLog> PasswordReadLogs { get; set; } = new HashSet<PasswordReadLog>();

    #endregion

    #endregion
}
