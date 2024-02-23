using EncryptLocker.Website.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text;

namespace EncryptLocker.Website.Components.Pages;

public partial class LockerPage : IDisposable
{
    #region Fields

    private Locker? _locker;
    private bool _isOpened;
    private bool _isDeleting;
    private List<SafeEntry>? _safeEntries;
    bool _isPasswordVisible;
    InputType _passwordInput = InputType.Password;
    string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    private DotNetObjectReference<LockerPage>? _objRef;

    #endregion

    #region Properties

    [Parameter]
    public int Id { get; set; }

    #endregion

    #region Methods

    protected override async Task OnParametersSetAsync()
    {
        _locker = await DataService.GetLockerAsync(Id);

        if (_locker != null)
        {
            _isOpened = DataService.IsLockerOpened(_locker);
            if (_isOpened)
            {
                //TODO : list entries
            }
        }
    }

    public void Dispose()
    {
        _objRef?.Dispose();
        GC.SuppressFinalize(this);
    }

    #region OpenLocker

    private async Task BeginOpenLocker()
    {
        if (_isPasswordVisible) 
        {
            ShowHidePassword();
        }
        _objRef?.Dispose();
        _objRef = DotNetObjectReference.Create(this);
        await JSRuntime.InvokeVoidAsync("postOpenLockerData", _objRef);
    }

    [JSInvokable("OpenLocker")]
    public async Task OpenLocker(List<byte> passwordHash)
    {
        if (_locker != null && _isOpened == false)
        {
            _safeEntries = await DataService.LockerStreamAsync(_locker, [.. passwordHash]);
            _isOpened = DataService.IsLockerOpened(_locker);
            if (_isOpened == false)
            {
                SnackbarService.Add("Mot de passe incorect", Severity.Error);
            }
            else
            {
                SnackbarService.Add("Coffre ouvert", Severity.Success);
            }
            await InvokeAsync(StateHasChanged);
        }
    }

    #endregion

    #region DeleteLocker

    private void OnDeleteLockerClick()
    {
        _isDeleting = true;
    }

    private async Task BeginDeleteLocker()
    {
        if (_isPasswordVisible)
        {
            ShowHidePassword();
        }
        _objRef?.Dispose();
        _objRef = DotNetObjectReference.Create(this);
        await JSRuntime.InvokeVoidAsync("postDeleteLockerData", _objRef);
    }

    [JSInvokable("DeleteLocker")]
    public async Task DeleteLocker(List<byte> passwordHash)
    {
        if (_locker != null && _isOpened == true)
        {
            ResultMessage message = await DataService.DeleteLockerAsync(_locker, [.. passwordHash]);
            if (message.IsSuccess == false)
            {
                SnackbarService.Add("Mot de passe incorect", Severity.Error);
            }
            else
            {
                SnackbarService.Add("Coffre supprimé définitivement", Severity.Success);
            }
            await InvokeAsync(StateHasChanged);
            //TODO : Retourner à la page d'accueil
        }
    }

    #endregion

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
