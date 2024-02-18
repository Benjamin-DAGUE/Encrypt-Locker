namespace EncryptLocker.Database;

public class SafeGroup : SafeBase
{
    #region Properties

    #region Navigations

    public IEnumerable<SafeBase> Children { get; set; } = new HashSet<SafeBase>();

    #endregion

    #endregion
}
