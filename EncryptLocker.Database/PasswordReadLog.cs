namespace EncryptLocker.Database;

public class PasswordReadLog
{
    #region Properties

    public int Id { get; set; }
    public int SafeEntryId { get; set; }
    public int RegisteredUserId { get; set; }
    public DateTimeOffset DateTime {  get; set; }

    #region Navigations

    public required SafeEntry SafeEntry { get; set; }
    public required RegisteredUser RegisteredUser { get; set; }

    #endregion

    #endregion
}
