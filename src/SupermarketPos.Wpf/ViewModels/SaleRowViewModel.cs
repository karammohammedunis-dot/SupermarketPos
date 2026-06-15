using SupermarketPos.Domain.Entities;
using SupermarketPos.Wpf.Common;

namespace SupermarketPos.Wpf.ViewModels;

public sealed class SaleRowViewModel
{
    public Sale Sale { get; }
    public SaleRowViewModel(Sale sale) => Sale = sale;

    public string ReceiptNo => Sale.ReceiptNo;
    public string DateText => Sale.CreatedAt.ToString("dd.MM.yyyy HH:mm");
    public string Cashier => Sale.CashierName;
    public string PaymentText => Sale.PaymentMethod == "CASH" ? "Nakit / Cash" : "Kart / Card";
    public string TotalText => MoneyText.Format(Sale.GrossTotal);
}
