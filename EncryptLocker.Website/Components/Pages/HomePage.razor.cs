using EncryptLocker.Website.Models;

namespace EncryptLocker.Website.Components.Pages;

public partial class HomePage
{
    #region Fields

    private List<Locker>? _lockers = null;

    #endregion

    #region Methods

    protected override async Task OnInitializedAsync()
    {
        _lockers = await DataService.GetLockersAsync();
    }

    #endregion
}
