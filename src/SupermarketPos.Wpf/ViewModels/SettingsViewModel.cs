using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupermarketPos.Application.Abstractions;
using SupermarketPos.Wpf.Localization;

namespace SupermarketPos.Wpf.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsRepository _settings;

    public SettingsViewModel(ISettingsRepository settings) => _settings = settings;

    [ObservableProperty] private string _storeName = string.Empty;
    [ObservableProperty] private string _storeAddress = string.Empty;
    [ObservableProperty] private string _language = "tr";
    [ObservableProperty] private string _lowStockThreshold = "5";
    [ObservableProperty] private string _vatRatesCsv = "0.01,0.10,0.20";

    public string[] Languages { get; } = { "tr", "en" };

    public async Task LoadAsync()
    {
        var s = await _settings.GetAsync();
        StoreName = s.StoreName;
        StoreAddress = s.StoreAddress;
        Language = s.Language;
        LowStockThreshold = s.LowStockThreshold.ToString("0.###", CultureInfo.InvariantCulture);
        VatRatesCsv = s.VatRatesCsv;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var threshold = decimal.Parse(LowStockThreshold.Replace(',', '.'), CultureInfo.InvariantCulture);
            var rates = VatRatesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(r => decimal.Parse(r, CultureInfo.InvariantCulture))
                .ToList();

            var s = await _settings.GetAsync();
            s.Update(StoreName, StoreAddress, Language, threshold, rates);
            await _settings.SaveAsync(s);

            LocalizationManager.Instance.SetLanguage(Language);

            MessageBox.Show(LocalizationManager.Instance["Settings_Saved"],
                LocalizationManager.Instance["Common_Success"], MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, LocalizationManager.Instance["Common_Error"], MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
