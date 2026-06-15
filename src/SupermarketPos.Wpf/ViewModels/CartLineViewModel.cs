using CommunityToolkit.Mvvm.ComponentModel;
using SupermarketPos.Domain.Pricing;
using SupermarketPos.Wpf.Common;

namespace SupermarketPos.Wpf.ViewModels;

/// <summary>View wrapper over a domain <see cref="CartLine"/>. Editing the quantity flows back into
/// the domain line and triggers a recalculation of the cart totals.</summary>
public sealed partial class CartLineViewModel : ObservableObject
{
    private readonly Action _onChanged;

    public CartLine Line { get; }

    public CartLineViewModel(CartLine line, Action onChanged)
    {
        Line = line;
        _onChanged = onChanged;
    }

    public string Name => Line.Product.Name;
    public string UnitText => Line.Product.Unit == Domain.Common.Unit.Kilogram ? "kg" : "ad";
    public string UnitPriceText => MoneyText.Format(Line.GrossUnitPrice);
    public string LineTotalText => MoneyText.Format(Line.GrossTotal);
    public string? DiscountText => Line.LineDiscount?.Describe();

    public decimal Quantity
    {
        get => Line.Quantity;
        set
        {
            try
            {
                if (value <= 0) return;
                Line.SetQuantity(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(LineTotalText));
                _onChanged();
            }
            catch
            {
                OnPropertyChanged(); // revert display on invalid entry
            }
        }
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(Quantity));
        OnPropertyChanged(nameof(LineTotalText));
        OnPropertyChanged(nameof(DiscountText));
    }
}
