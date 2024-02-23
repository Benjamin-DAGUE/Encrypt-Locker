namespace EncryptLocker.Database;

public class SafeEntry : SafeBase
{
    #region Properties

    public int LoginId { get; set; }
    public int PasswordId { get; set; }
    public int? UrlId { get; set; }
    public int? NoteId { get; set; }

    #region Navigations

    public required CypherValue Login { get; set; }
    public required CypherValue Password { get; set; }
    public CypherValue? Url { get; set; }
    public CypherValue? Note { get; set; }
    public IEnumerable<PasswordReadLog> PasswordReadLogs { get; set; } = new HashSet<PasswordReadLog>();

    #endregion

    #endregion
}
