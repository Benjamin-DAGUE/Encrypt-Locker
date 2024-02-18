namespace EncryptLocker.Database;

public class CypherValue
{
    #region Properties

    public int Id { get; set; }
    public byte[] Cypher { get; set; } = [];
    public byte[] Tag { get; set; } = [];
    public byte[] IV { get; set; } = [];

    #endregion
}
