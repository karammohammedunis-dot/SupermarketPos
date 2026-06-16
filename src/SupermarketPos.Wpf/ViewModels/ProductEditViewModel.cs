using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SupermarketPos.Domain.Entities;

namespace SupermarketPos.Wpf.ViewModels;

/// <summary>Editable form state for creating or updating a product.</summary>
public sealed partial class ProductEditViewModel : ObservableObject
{
    public int? Id { get; private set; }

    [ObservableProperty] private string _barcode = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _price = string.Empty;       // KDV-inclusive
    [ObservableProperty] private decimal _vatRate = 0.20m;           // stored as fraction
    [ObservableProperty] private bool _isKilogram;
    [ObservableProperty] private string _stock = string.Empty;
    [ObservableProperty] private int _categoryId;
    [ObservableProperty] private string? _imagePath;

    public ObservableCollection<decimal> VatRates { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    public bool IsNew => Id is null;

    public void LoadNew(int defaultCategoryId, decimal defaultVat)
    {
        Id = null;
        Barcode = Name = Price = Stock = string.Empty;
        VatRate = defaultVat;
        IsKilogram = false;
        CategoryId = defaultCategoryId;
        ImagePath = null;
    }

    public void Load(Product p)
    {
        Id = p.Id;
        Barcode = p.Barcode;
        Name = p.Name;
        Price = p.UnitPrice.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        VatRate = p.VatRate;
        IsKilogram = p.Unit == Domain.Common.Unit.Kilogram;
        Stock = p.StockQuantity.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        CategoryId = p.CategoryId;
        ImagePath = p.ImagePath;
    }

    [RelayCommand]
    private void BrowseImage()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Ürün görseli seç",
            Filter = "Görseller|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Tüm dosyalar|*.*"
        };
        if (dialog.ShowDialog() == true)
            ImagePath = dialog.FileName;
    }

    [RelayCommand]
    private void ClearImage() => ImagePath = null;
}
