using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SupermarketPos.Application.Abstractions;
using SupermarketPos.Domain.Common;
using SupermarketPos.Domain.Entities;
using SupermarketPos.Wpf.Localization;

namespace SupermarketPos.Wpf.ViewModels;

public sealed partial class ProductsViewModel : ObservableObject
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;
    private readonly ISettingsRepository _settings;
    private readonly IShelfLabelService _labels;
    private readonly IUnitOfWork _uow;

    private decimal _lowStockThreshold = 5m;
    private List<Category> _categoryList = new();

    public ProductsViewModel(IProductRepository products, ICategoryRepository categories,
                             ISettingsRepository settings, IShelfLabelService labels, IUnitOfWork uow)
    {
        _products = products;
        _categories = categories;
        _settings = settings;
        _labels = labels;
        _uow = uow;
    }

    public ObservableCollection<ProductRowViewModel> Products { get; } = new();
    public ProductEditViewModel Editor { get; } = new();

    [ObservableProperty] private bool _lowStockOnly;
    [ObservableProperty] private bool _isEditorOpen;
    [ObservableProperty] private string _restockAmount = string.Empty;

    partial void OnLowStockOnlyChanged(bool value) => _ = RefreshAsync();

    public async Task RefreshAsync()
    {
        var settings = await _settings.GetAsync();
        _lowStockThreshold = settings.LowStockThreshold;

        _categoryList = (await _categories.GetAllAsync()).ToList();
        Editor.Categories.Clear();
        foreach (var c in _categoryList) Editor.Categories.Add(c);
        Editor.VatRates.Clear();
        foreach (var r in settings.VatRates) Editor.VatRates.Add(r);

        var items = LowStockOnly
            ? await _products.GetLowStockAsync(_lowStockThreshold)
            : await _products.GetAllAsync();

        Products.Clear();
        foreach (var p in items)
            Products.Add(new ProductRowViewModel(p, _lowStockThreshold));
    }

    [RelayCommand]
    private void New()
    {
        Editor.LoadNew(_categoryList.FirstOrDefault()?.Id ?? 0,
                       Editor.VatRates.Contains(0.20m) ? 0.20m : Editor.VatRates.FirstOrDefault());
        IsEditorOpen = true;
    }

    [RelayCommand]
    private void Edit(ProductRowViewModel? row)
    {
        if (row is null) return;
        Editor.Load(row.Product);
        RestockAmount = string.Empty;
        IsEditorOpen = true;
    }

    [RelayCommand]
    private void CancelEdit() => IsEditorOpen = false;

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var price = ParseDecimal(Editor.Price);
            var stock = ParseDecimal(Editor.Stock);
            var unit = Editor.IsKilogram ? Unit.Kilogram : Unit.Piece;

            if (Editor.IsNew)
            {
                var product = new Product(Editor.Barcode, Editor.Name, price, Editor.VatRate, unit, stock, Editor.CategoryId);
                await _products.AddAsync(product);
            }
            else
            {
                var product = await _products.GetByIdAsync(Editor.Id!.Value)
                              ?? throw new DomainException("Product not found.");
                product.SetBarcode(Editor.Barcode);
                product.SetName(Editor.Name);
                product.SetPrice(price);
                product.SetVatRate(Editor.VatRate);
                product.SetUnit(unit);
                product.SetStock(stock);
                product.SetCategory(Editor.CategoryId);
            }

            await _uow.SaveChangesAsync();
            IsEditorOpen = false;
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    [RelayCommand]
    private async Task RestockAsync()
    {
        if (Editor.IsNew || !decimal.TryParse(RestockAmount.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
            return;
        try
        {
            var product = await _products.GetByIdAsync(Editor.Id!.Value)
                          ?? throw new DomainException("Product not found.");
            product.Restock(amount);
            await _uow.SaveChangesAsync();
            Editor.Load(product);
            RestockAmount = string.Empty;
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(ProductRowViewModel? row)
    {
        if (row is null) return;
        var confirm = MessageBox.Show($"{row.Name}?", LocalizationManager.Instance["Common_Delete"],
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;
        try
        {
            var product = await _products.GetByIdAsync(row.Id);
            if (product is not null)
            {
                _products.Remove(product);
                await _uow.SaveChangesAsync();
            }
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    [RelayCommand]
    private async Task PrintLabelsAsync()
    {
        if (Products.Count == 0) return;
        try
        {
            var items = Products.Select(r => r.Product).ToList();
            var dialog = new SaveFileDialog
            {
                FileName = $"RafEtiketleri_{DateTime.Now:yyyyMMdd}.pdf",
                Filter = "PDF (*.pdf)|*.pdf"
            };
            if (dialog.ShowDialog() != true) return;

            await _labels.SavePdfAsync(items, dialog.FileName);
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true }); }
            catch { /* no viewer */ }
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    private static decimal ParseDecimal(string s) =>
        decimal.TryParse((s ?? string.Empty).Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
            ? v : throw new DomainException("Invalid number.");

    private static void Error(string msg) =>
        MessageBox.Show(msg, LocalizationManager.Instance["Common_Error"], MessageBoxButton.OK, MessageBoxImage.Warning);
}
