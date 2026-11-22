namespace Beadnado_Oroszi_Sandor_Y3ZAKR.Models;

/// <summary>
/// Model: a gyorsulásmérő egy mérésének X, Y, Z értéke (g egységben).
/// A származtatott értékeket (vektor nagysága, eltérés 1 g-től) is itt számoljuk.
/// </summary>
public class MotionData
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public MotionData(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>A gyorsulásvektor nagysága: a = √(x² + y² + z²).</summary>
    public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>Eltérés a nyugalmi (1 g) értéktől: d = |a − 1|.</summary>
    public double Deviation => Math.Abs(Magnitude - 1.0);
}
