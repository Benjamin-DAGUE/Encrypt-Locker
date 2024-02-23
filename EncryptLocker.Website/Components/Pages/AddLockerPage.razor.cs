using EncryptLocker.Website.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace EncryptLocker.Website.Components.Pages;

public partial class AddLockerPage : IDisposable
{
    #region Fields

    private bool _isPasswordVisible;
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    private DotNetObjectReference<AddLockerPage>? _objRef;

    #endregion

    #region Methods

    public void Dispose()
    {
        _objRef?.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task BeginAddLocker()
    {
        _objRef?.Dispose();
        _objRef = DotNetObjectReference.Create(this);
        await JSRuntime.InvokeVoidAsync("postAddLockerData", _objRef);
    }

    [JSInvokable("AddLocker")]
    public async Task AddLocker(string name, List<byte> passwordHash)
    {
        Locker locker = await DataService.AddLockerAsync(new Locker()
        {
            Name = name
        }, [.. passwordHash]);

        NavigationManager.NavigateTo($"/locker/{locker.Id}");
    }

    private void ShowHidePassword()
    {
        if (_isPasswordVisible)
        {
            _isPasswordVisible = false;
            _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
            _passwordInput = InputType.Password;
        }
        else
        {
            _isPasswordVisible = true;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
            _passwordInput = InputType.Text;
        }
    }

    #endregion

}
