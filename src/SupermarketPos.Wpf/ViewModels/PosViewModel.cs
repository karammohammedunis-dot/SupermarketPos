using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SupermarketPos.Application.Abstractions;
using SupermarketPos.Domain.Entities;
using SupermarketPos.Domain.Pricing;
using SupermarketPos.Wpf.Common;
using SupermarketPos.Wpf.Localization;

namespace SupermarketPos.Wpf.ViewModels;

public sealed partial class PosViewModel : ObservableObject
{
    private readonly IProductRepository _products;
    private readonly IBarcodeService _barcode;
    private readonly ISaleService _sales;
    private readonly IReceiptService _receipts;
    private readonly IAuthService _auth;

    private readonly Cart _cart = new();

    public PosViewModel(IProductRepository products, IBarcodeService barcode,
                        ISaleService sales, IReceiptService receipts, IAuthService auth)
    {
        _products = products;
        _barcode = barcode;
        _sales = sales;
        _receipts = receipts;
        _auth = auth;
        Recalculate();
    }

    public ObservableCollection<CartLineViewModel> Lines { get; } = new();
    public ObservableCollection<Product> SearchResults { get; } = new();
    public ObservableCollection<VatLineViewModel> VatBreakdown { get; } = new();

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _showResults;
    [ObservableProperty] private string? _status;

    [ObservableProperty] private string _grossTotalText = "0,00 ₺";
    [ObservableProperty] private string _netTotalText = "0,00 ₺";
    [ObservableProperty] private string _vatTotalText = "0,00 ₺";
    [ObservableProperty] private string _itemCountText = "0";

    // Cart discount inputs
    [ObservableProperty] private string _cartDiscountPercent = string.Empty;
    [ObservableProperty] private string _cartDiscountAmount = string.Empty;
    [ObservableProperty] private string? _cartDiscountText;

    // Payment overlay
    [ObservableProperty] private bool _isPaymentOpen;
    [ObservableProperty] private bool _isCash = true;
    [ObservableProperty] private string _tendered = string.Empty;
    [ObservableProperty] private string _changeText = "0,00 ₺";

    private decimal CurrentGross => PricingService.GrossTotal(_cart);

    // ---------- adding products ----------

    [RelayCommand]
    private async Task SubmitSearchAsync()
    {
        var input = _barcode.Normalize(SearchText);
        if (string.IsNullOrWhiteSpace(input)) return;

        var byBarcode = await _products.GetByBarcodeAsync(input);
        if (byBarcode is not null)
        {
            AddProduct(byBarcode);
            SearchText = string.Empty;
            ShowResults = false;
            return;
        }

        var matches = await _products.SearchByNameAsync(input);
        SearchResults.Clear();
        foreach (var p in matches) SearchResults.Add(p);

        if (SearchResults.Count == 1)
        {
            AddProduct(SearchResults[0]);
            SearchText = string.Empty;
            ShowResults = false;
        }
        else
        {
            ShowResults = SearchResults.Count > 0;
            Status = SearchResults.Count == 0 ? LocalizationManager.Instance["Pos_NotFound"] : null;
        }
    }

    [RelayCommand]
    private void AddFromResults(Product? product)
    {
        if (product is null) return;
        AddProduct(product);
        SearchText = string.Empty;
        ShowResults = false;
        SearchResults.Clear();
    }

    private void AddProduct(Product product)
    {
        var qty = product.Unit == Domain.Common.Unit.Kilogram ? 1.000m : 1m;
        var existing = Lines.FirstOrDefault(l => l.Line.Product.Id == product.Id);
        _cart.AddOrIncrement(product, qty);

        if (existing is null)
            Lines.Add(new CartLineViewModel(_cart.Lines.Last(), Recalculate));
        else
            existing.Refresh();

        Status = null;
        Recalculate();
    }

    [RelayCommand]
    private void RemoveLine(CartLineViewModel? line)
    {
        if (line is null) return;
        _cart.Remove(line.Line);
        Lines.Remove(line);
        Recalculate();
    }

    // ---------- discounts ----------

    [RelayCommand]
    private void ApplyCartDiscount()
    {
        Discount? discount = null;
        if (TryParse(CartDiscountPercent, out var pct) && pct > 0)
            discount = new PercentageDiscount(pct / 100m);
        else if (TryParse(CartDiscountAmount, out var amt) && amt > 0)
            discount = new FixedAmountDiscount(amt);

        _cart.ApplyCartDiscount(discount);
        CartDiscountText = discount?.Describe();
        Recalculate();
    }

    [RelayCommand]
    private void ClearCartDiscount()
    {
        _cart.ApplyCartDiscount(null);
        CartDiscountPercent = string.Empty;
        CartDiscountAmount = string.Empty;
        CartDiscountText = null;
        Recalculate();
    }

    // ---------- payment ----------

    [RelayCommand]
    private void OpenCash()
    {
        if (_cart.IsEmpty) return;
        IsCash = true;
        Tendered = CurrentGross.ToString("0.00", CultureInfo.InvariantCulture);
        UpdateChange();
        IsPaymentOpen = true;
    }

    [RelayCommand]
    private void OpenCard()
    {
        if (_cart.IsEmpty) return;
        IsCash = false;
        IsPaymentOpen = true;
    }

    [RelayCommand]
    private void CancelPayment() => IsPaymentOpen = false;

    partial void OnTenderedChanged(string value) => UpdateChange();

    private void UpdateChange()
    {
        var change = TryParse(Tendered, out var t) ? t - CurrentGross : 0m;
        ChangeText = MoneyText.Format(change < 0 ? 0m : change);
    }

    [RelayCommand]
    private async Task CompletePaymentAsync()
    {
        if (_cart.IsEmpty || _auth.CurrentUser is null) return;
        var total = CurrentGross;

        try
        {
            Payment payment = IsCash
                ? new CashPayment(total, ParseOrThrow(Tendered))
                : new CardPayment(total);

            var sale = await _sales.CheckoutAsync(_cart, payment, _auth.CurrentUser);

            IsPaymentOpen = false;
            Status = LocalizationManager.Instance.Format("Msg_SaleComplete", sale.ReceiptNo);

            await OfferReceiptAsync(sale);

            _cart.Clear();
            Lines.Clear();
            ClearCartDiscount();
            Recalculate();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, LocalizationManager.Instance["Common_Error"],
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async Task OfferReceiptAsync(Sale sale)
    {
        var dialog = new SaveFileDialog
        {
            Title = LocalizationManager.Instance["Msg_PrintReceipt"],
            FileName = $"Fis_{sale.ReceiptNo}.pdf",
            Filter = "PDF (*.pdf)|*.pdf"
        };
        if (dialog.ShowDialog() == true)
        {
            await _receipts.SavePdfAsync(sale, dialog.FileName);
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true }); }
            catch { /* viewer not available */ }
        }
    }

    // ---------- recompute ----------

    private void Recalculate()
    {
        var summary = PricingService.Summarize(_cart);
        GrossTotalText = MoneyText.Format(summary.GrossTotal);
        NetTotalText = MoneyText.Format(summary.NetTotal);
        VatTotalText = MoneyText.Format(summary.VatTotal);
        ItemCountText = _cart.ItemCount.ToString("0.###", CultureInfo.InvariantCulture);

        VatBreakdown.Clear();
        foreach (var g in summary.VatBreakdown)
            VatBreakdown.Add(new VatLineViewModel(g));

        OpenCashCommand.NotifyCanExecuteChanged();
        OpenCardCommand.NotifyCanExecuteChanged();
        UpdateChange();
    }

    private static bool TryParse(string? s, out decimal value) =>
        decimal.TryParse((s ?? string.Empty).Replace(',', '.'), NumberStyles.Any,
            CultureInfo.InvariantCulture, out value);

    private static decimal ParseOrThrow(string s) =>
        TryParse(s, out var v) ? v : throw new SupermarketPos.Domain.Common.DomainException("Invalid amount.");
}

public sealed class VatLineViewModel
{
    public VatLineViewModel(VatGroup g)
    {
        RateText = $"%{g.Rate * 100:0.#}";
        NetText = MoneyText.Format(g.Net);
        VatText = MoneyText.Format(g.Vat);
    }

    public string RateText { get; }
    public string NetText { get; }
    public string VatText { get; }
}
