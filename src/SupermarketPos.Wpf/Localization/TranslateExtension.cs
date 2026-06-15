using System.Windows.Data;
using System.Windows.Markup;

namespace SupermarketPos.Wpf.Localization;

/// <summary>
/// XAML markup extension: <c>{loc:T Pos_Total}</c> binds a control to the live localized string,
/// so a language switch updates the UI without reloading views.
/// </summary>
public sealed class TExtension : MarkupExtension
{
    public string Key { get; set; }

    public TExtension() => Key = string.Empty;
    public TExtension(string key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizationManager.Instance,
            Mode = BindingMode.OneWay
        };
        return binding.ProvideValue(serviceProvider);
    }
}
