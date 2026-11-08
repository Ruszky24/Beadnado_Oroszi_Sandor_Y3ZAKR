using System.Globalization;

namespace Beadnado_Oroszi_Sandor_Y3ZAKR.Converters;

/// <summary>
/// bool -> Color átalakító. Pl. ha elértük a célt, zöld lesz a szöveg.
/// A színeket XAML-ből lehet megadni (TrueColor / FalseColor).
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Colors.Green;
    public Color FalseColor { get; set; } = Colors.Gray;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? TrueColor : FalseColor;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
