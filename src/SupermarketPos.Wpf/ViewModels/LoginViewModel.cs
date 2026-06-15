using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupermarketPos.Application.Abstractions;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Wpf.ViewModels;

public sealed partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;

    public LoginViewModel(IAuthService auth) => _auth = auth;

    /// <summary>Raised when authentication succeeds; the shell listens to switch screens.</summary>
    public event Action<User>? LoggedIn;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string? _error;

    [RelayCommand]
    private async Task LoginAsync()
    {
        Error = null;
        try
        {
            var user = await _auth.LoginAsync(Username, Password);
            Password = string.Empty;
            LoggedIn?.Invoke(user);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}
