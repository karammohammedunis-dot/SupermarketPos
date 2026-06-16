using CommunityToolkit.Mvvm.ComponentModel;
using SupermarketPos.Application.Abstractions;
using SupermarketPos.Wpf.Common;

namespace SupermarketPos.Wpf.ViewModels;

/// <summary>
/// Admin landing screen: a handful of live, colorful metric cards pulled from the repositories
/// and today's X-report. Inspired by modern POS dashboards, but its own layout/metrics.
/// </summary>
public sealed partial class DashboardViewModel : ObservableObject
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;
    private readonly ISettingsRepository _settings;
    private readonly IReportService _reports;
    private readonly IAuthService _auth;

    public DashboardViewModel(IProductRepository products, ICategoryRepository categories,
                              ISettingsRepository settings, IReportService reports, IAuthService auth)
    {
        _products = products;
        _categories = categories;
        _settings = settings;
        _reports = reports;
        _auth = auth;
    }

    [ObservableProperty] private string _greeting = string.Empty;
    [ObservableProperty] private string _dateText = string.Empty;

    [ObservableProperty] private string _revenueText = "0,00 ₺";
    [ObservableProperty] private string _saleCountText = "0";
    [ObservableProperty] private string _vatText = "0,00 ₺";
    [ObservableProperty] private string _productsText = "0";
    [ObservableProperty] private string _categoriesText = "0";
    [ObservableProperty] private string _lowStockText = "0";

    public async Task RefreshAsync()
    {
        var now = DateTime.Now;
        var hour = now.Hour;
        var partOfDay = hour < 12 ? "Günaydın" : hour < 18 ? "İyi günler" : "İyi akşamlar";
        var name = _auth.CurrentUser?.DisplayName ?? "";
        Greeting = $"{partOfDay}, {name}";
        DateText = now.ToString("dd MMMM yyyy");

        var settings = await _settings.GetAsync();
        var allProducts = await _products.GetAllAsync();
        var lowStock = await _products.GetLowStockAsync(settings.LowStockThreshold);
        var cats = await _categories.GetAllAsync();
        var today = await _reports.GetXReportAsync(now);

        ProductsText = allProducts.Count.ToString();
        CategoriesText = cats.Count.ToString();
        LowStockText = lowStock.Count.ToString();
        RevenueText = MoneyText.Format(today.GrossTotal);
        SaleCountText = today.SaleCount.ToString();
        VatText = MoneyText.Format(today.VatTotal);
    }
}
