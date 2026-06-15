using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SupermarketPos.Application.Abstractions;
using SupermarketPos.Application.Dtos;
using SupermarketPos.Wpf.Common;
using SupermarketPos.Wpf.Localization;

namespace SupermarketPos.Wpf.ViewModels;

public sealed partial class ReportsViewModel : ObservableObject
{
    private readonly IReportService _reports;
    private readonly IReceiptService _receipts;
    private readonly IReportPdfService _reportPdf;

    private SalesReport? _current;

    public ReportsViewModel(IReportService reports, IReceiptService receipts, IReportPdfService reportPdf)
    {
        _reports = reports;
        _receipts = receipts;
        _reportPdf = reportPdf;
        SelectedDay = DateTime.Today;
        HistoryFrom = DateTime.Today.AddDays(-7);
        HistoryTo = DateTime.Today;
    }

    public ObservableCollection<VatLineViewModel> VatBreakdown { get; } = new();
    public ObservableCollection<SaleRowViewModel> History { get; } = new();

    [ObservableProperty] private DateTime _selectedDay;
    [ObservableProperty] private DateTime _historyFrom;
    [ObservableProperty] private DateTime _historyTo;
    [ObservableProperty] private SaleRowViewModel? _selectedSale;

    [ObservableProperty] private string? _reportTitle;
    [ObservableProperty] private string _saleCountText = "0";
    [ObservableProperty] private string _grossText = "0,00 ₺";
    [ObservableProperty] private string _netText = "0,00 ₺";
    [ObservableProperty] private string _vatText = "0,00 ₺";
    [ObservableProperty] private string _cashText = "0,00 ₺";
    [ObservableProperty] private string _cardText = "0,00 ₺";
    [ObservableProperty] private bool _hasReport;

    [RelayCommand]
    private Task GenerateXAsync() => GenerateAsync(true);

    [RelayCommand]
    private Task GenerateZAsync() => GenerateAsync(false);

    private async Task GenerateAsync(bool isX)
    {
        _current = isX
            ? await _reports.GetXReportAsync(SelectedDay)
            : await _reports.GetZReportAsync(SelectedDay);
        Show(_current);
    }

    private void Show(SalesReport r)
    {
        ReportTitle = r.Title;
        SaleCountText = r.SaleCount.ToString();
        GrossText = MoneyText.Format(r.GrossTotal);
        NetText = MoneyText.Format(r.NetTotal);
        VatText = MoneyText.Format(r.VatTotal);
        CashText = $"{MoneyText.Format(r.PaymentSplit.CashTotal)} ({r.PaymentSplit.CashCount})";
        CardText = $"{MoneyText.Format(r.PaymentSplit.CardTotal)} ({r.PaymentSplit.CardCount})";

        VatBreakdown.Clear();
        foreach (var g in r.VatBreakdown) VatBreakdown.Add(new VatLineViewModel(g));
        HasReport = true;
    }

    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        var sales = await _reports.GetSalesAsync(HistoryFrom.Date, HistoryTo.Date.AddDays(1));
        History.Clear();
        foreach (var s in sales) History.Add(new SaleRowViewModel(s));
    }

    [RelayCommand]
    private async Task OpenReceiptAsync()
    {
        if (SelectedSale is null) return;
        var bytes = await _receipts.RenderPdfAsync(SelectedSale.Sale);
        OpenPdf(bytes, $"Fis_{SelectedSale.ReceiptNo}.pdf");
    }

    [RelayCommand]
    private async Task ExportReportAsync()
    {
        if (_current is null) return;
        var dialog = new SaveFileDialog
        {
            FileName = $"{_current.Title}_{_current.FromInclusive:yyyyMMdd}.pdf",
            Filter = "PDF (*.pdf)|*.pdf"
        };
        if (dialog.ShowDialog() == true)
        {
            await _reportPdf.SavePdfAsync(_current, dialog.FileName);
            TryOpen(dialog.FileName);
        }
    }

    private static void OpenPdf(byte[] bytes, string fileName)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);
        System.IO.File.WriteAllBytes(path, bytes);
        TryOpen(path);
    }

    private static void TryOpen(string path)
    {
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true }); }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, LocalizationManager.Instance["Common_Error"], MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
