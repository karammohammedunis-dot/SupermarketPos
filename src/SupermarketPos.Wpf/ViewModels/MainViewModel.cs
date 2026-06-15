using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SupermarketPos.Application.Abstractions;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Wpf.ViewModels;

/// <summary>
/// Shell view model: shows the login screen until authenticated, then hosts the navigable screens.
/// Admin-only destinations are gated by <see cref="IsAdmin"/>.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _provider;
    private readonly IAuthService _auth;

    private PosViewModel? _pos;
    private ProductsViewModel? _products;
    private ReportsViewModel? _reports;
    private SettingsViewModel? _settings;

    public MainViewModel(IServiceProvider provider, IAuthService auth, LoginViewModel login)
    {
        _provider = provider;
        _auth = auth;
        Login = login;
        Login.LoggedIn += OnLoggedIn;
        CurrentViewModel = login;
    }

    public LoginViewModel Login { get; }

    [ObservableProperty] private object _currentViewModel;
    [ObservableProperty] private bool _isLoggedIn;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private string _userDisplay = string.Empty;
    [ObservableProperty] private string _activeScreen = "pos";

    private void OnLoggedIn(User user)
    {
        IsLoggedIn = true;
        IsAdmin = user.IsAdmin;
        UserDisplay = $"{user.DisplayName} ({user.Role})";
        ShowPos();
    }

    [RelayCommand]
    private void ShowPos()
    {
        _pos ??= _provider.GetRequiredService<PosViewModel>();
        CurrentViewModel = _pos;
        ActiveScreen = "pos";
    }

    [RelayCommand]
    private async Task ShowProductsAsync()
    {
        if (!_auth.CurrentUser?.IsAdmin ?? true) return;
        _products ??= _provider.GetRequiredService<ProductsViewModel>();
        await _products.RefreshAsync();
        CurrentViewModel = _products;
        ActiveScreen = "products";
    }

    [RelayCommand]
    private void ShowReports()
    {
        if (!_auth.CurrentUser?.IsAdmin ?? true) return;
        _reports ??= _provider.GetRequiredService<ReportsViewModel>();
        CurrentViewModel = _reports;
        ActiveScreen = "reports";
    }

    [RelayCommand]
    private async Task ShowSettingsAsync()
    {
        if (!_auth.CurrentUser?.IsAdmin ?? true) return;
        _settings ??= _provider.GetRequiredService<SettingsViewModel>();
        await _settings.LoadAsync();
        CurrentViewModel = _settings;
        ActiveScreen = "settings";
    }

    [RelayCommand]
    private void Logout()
    {
        _auth.Logout();
        IsLoggedIn = false;
        IsAdmin = false;
        UserDisplay = string.Empty;
        _pos = null;
        _products = null;
        _reports = null;
        _settings = null;
        CurrentViewModel = Login;
    }
}
