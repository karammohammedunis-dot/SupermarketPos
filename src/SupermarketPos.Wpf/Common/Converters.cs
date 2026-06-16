using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SupermarketPos.Wpf.Common;

/// <summary>Deterministically maps an integer (category id/index) to one of the vibrant hues.</summary>
public sealed class CategoryColorConverter : IValueConverter
{
    public static readonly Color[] Palette =
    {
        Color.FromRgb(0xEF, 0x44, 0x44), // red
        Color.FromRgb(0x8B, 0x5C, 0xF6), // violet
        Color.FromRgb(0xEC, 0x48, 0x99), // pink
        Color.FromRgb(0xF5, 0x9E, 0x0B), // amber
        Color.FromRgb(0x10, 0xB9, 0x81), // emerald
        Color.FromRgb(0x3B, 0x82, 0xF6), // blue
        Color.FromRgb(0x14, 0xB8, 0xA6), // teal
        Color.FromRgb(0xF9, 0x73, 0x16), // orange
    };

    public static Brush Brush(int key)
    {
        var b = new SolidColorBrush(Palette[((key % Palette.Length) + Palette.Length) % Palette.Length]);
        b.Freeze();
        return b;
    }

    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture) =>
        Brush(value is int i ? i : 0);

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Loads an ImageSource from a file path; returns null (no image) on missing/invalid path.</summary>
public sealed class PathToImageConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        var path = value as string;
        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path)) return null;
        try
        {
            var bmp = new System.Windows.Media.Imaging.BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(path, UriKind.Absolute);
            bmp.DecodePixelWidth = 220;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>True when there is a usable image at the given path (for placeholder toggling).</summary>
public sealed class HasImageToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string s && !string.IsNullOrWhiteSpace(s) && System.IO.File.Exists(s)
            ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Up to two uppercase initials from a name, for avatar placeholders.</summary>
public sealed class InitialsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        var name = (value as string ?? string.Empty).Trim();
        if (name.Length == 0) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var s = parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}" : name.Substring(0, Math.Min(2, name.Length));
        return s.ToUpper(CultureInfo.CurrentCulture);
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>True -> Collapsed, False -> Visible (used to show the login screen while signed out).</summary>
public sealed class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Visibility.Collapsed;
}

/// <summary>Non-empty string -> Visible, empty/null -> Collapsed.</summary>
public sealed class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
