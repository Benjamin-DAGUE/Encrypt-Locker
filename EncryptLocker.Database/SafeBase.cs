namespace EncryptLocker.Database;

public abstract class SafeBase
{
    #region Constants

    public const int SAFE_GROUP_DISCRIMINATOR = 1;
    public const int SAFE_ENTRY_DISCRIMINATOR = 2;

    #endregion

    #region Properties

    public int Id { get; set; }
    public int Discriminator { get; set; }
    public int? LockerId { get; set; }
    public int? ParentId { get; set; }
    public int TitleId { get; set; }

    #region Navigations

    public required Locker? Locker { get; set; }
    public required SafeGroup? Parent { get; set; }
    public required CypherValue Title { get; set; }

    #endregion

    #endregion
}
