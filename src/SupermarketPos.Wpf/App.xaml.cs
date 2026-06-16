using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using SupermarketPos.Infrastructure;
using SupermarketPos.Wpf.Localization;
using SupermarketPos.Wpf.ViewModels;

namespace SupermarketPos.Wpf;

public partial class App : System.Windows.Application
{
    private IServiceProvider _provider = null!;
    private IServiceScope _scope = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnUnhandledException;

        var services = new ServiceCollection();
        services.AddPos();                       // persistence + application services
        RegisterViewModels(services);

        var root = services.BuildServiceProvider();
        // One long-lived scope drives the whole single-user register app.
        _scope = root.CreateScope();
        _provider = _scope.ServiceProvider;

        try
        {
            await DependencyInjection.InitializeDatabaseAsync(_provider);

            // Apply persisted language before any window shows.
            var settings = _provider.GetRequiredService<SupermarketPos.Application.Abstractions.ISettingsRepository>();
            var current = await settings.GetAsync();
            LocalizationManager.Instance.SetLanguage(current.Language);

            var window = _provider.GetRequiredService<MainWindow>();
            window.DataContext = _provider.GetRequiredService<MainViewModel>();
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Başlatma hatası / Startup error:\n{ex}", "Supermarket POS",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<PosViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, LocalizationManager.Instance["Common_Error"],
            MessageBoxButton.OK, MessageBoxImage.Warning);
        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _scope?.Dispose();
        base.OnExit(e);
    }
}
