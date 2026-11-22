using System.Globalization;

namespace Beadnado_Oroszi_Sandor_Y3ZAKR.Converters;

/// <summary>
/// A mozgásállapot szövegéből színt állít elő:
///   „Nyugalomban"   → zöld
///   „Enyhe mozgás"  → narancssárga
///   „Erős mozgás"   → piros
/// Bármi más (pl. „–") → semleges szürke.
/// </summary>
public class MotionStateColorConverter : IValueConverter
{
    public static readonly Color RestColor   = Color.FromArgb("#22C55E"); // zöld
    public static readonly Color LightColor  = Color.FromArgb("#FB923C"); // narancssárga
    public static readonly Color StrongColor = Color.FromArgb("#EF4444"); // piros
    public static readonly Color NeutralColor = Color.FromArgb("#94A3B8");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value as string) switch
        {
            "Nyugalomban"  => RestColor,
            "Enyhe mozgás" => LightColor,
            "Erős mozgás"  => StrongColor,
            _              => NeutralColor
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
