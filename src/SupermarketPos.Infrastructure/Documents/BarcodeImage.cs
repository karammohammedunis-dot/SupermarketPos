using System.Runtime.InteropServices;
using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace SupermarketPos.Infrastructure.Documents;

/// <summary>Renders an EAN-13 barcode (with human-readable digits) to a PNG byte array.</summary>
public static class BarcodeImage
{
    public static byte[]? TryRenderEan13(string rawBarcode, int width = 300, int height = 110)
    {
        var digits = new string(rawBarcode.Where(char.IsDigit).ToArray());
        if (digits.Length < 12) return null;
        var content = digits.Substring(0, 12); // ZXing appends the 13th (check) digit

        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.EAN_13,
            Options = new EncodingOptions { Width = width, Height = height, Margin = 4, PureBarcode = false }
        };

        try
        {
            var pixels = writer.Write(content);
            using var bitmap = new SKBitmap(pixels.Width, pixels.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            Marshal.Copy(pixels.Pixels, 0, bitmap.GetPixels(), pixels.Pixels.Length);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
        catch
        {
            return null; // unencodable barcode — label falls back to text only
        }
    }
}
