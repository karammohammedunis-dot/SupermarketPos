using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace SupermarketPos.Wpf.Localization;

/// <summary>
/// Live UI string source backed by the embedded .resx files (TR default, EN satellite).
/// Bindings via <see cref="TranslateExtension"/> target the indexer, so switching language at
/// runtime refreshes every translated control. No user-facing strings are hardcoded in XAML.
/// </summary>
public sealed class LocalizationManager : INotifyPropertyChanged
{
    public static LocalizationManager Instance { get; } = new();

    private readonly ResourceManager _resources =
        new("SupermarketPos.Wpf.Localization.Strings", typeof(LocalizationManager).Assembly);

    private CultureInfo _culture = CultureInfo.GetCultureInfo("tr");

    private LocalizationManager() { }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentLanguage => _culture.TwoLetterISOLanguageName;

    /// <summary>Indexer used by bindings: <c>{loc:T Pos_Total}</c> resolves to <c>this["Pos_Total"]</c>.</summary>
    public string this[string key] => _resources.GetString(key, _culture) ?? key;

    public string Format(string key, params object[] args) =>
        string.Format(this[key], args);

    public void SetLanguage(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) code = "tr";
        _culture = CultureInfo.GetCultureInfo(code);
        Thread.CurrentThread.CurrentUICulture = _culture;
        // Notify the indexer changed so all translated bindings re-evaluate.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
    }
}
