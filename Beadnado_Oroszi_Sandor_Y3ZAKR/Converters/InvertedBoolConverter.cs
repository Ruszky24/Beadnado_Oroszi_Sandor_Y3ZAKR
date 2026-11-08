using System.Globalization;

namespace Beadnado_Oroszi_Sandor_Y3ZAKR.Converters;

/// <summary>
/// Megfordítja a bool értéket. Pl. a Start gomb akkor aktív, ha a mérés NEM fut.
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not true;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not true;
}
