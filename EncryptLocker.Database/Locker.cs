namespace EncryptLocker.Database;

public class Locker
{
    #region Properties

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte[] KeyHash { get; set; } = [];

    #region Navigations

    public IEnumerable<LockerAccessRight> LockerAccessRights { get; set; } = new HashSet<LockerAccessRight>();

    public IEnumerable<SafeBase> SafeEntries { get; set; } = new HashSet<SafeBase>();

    #endregion

    #endregion
}
