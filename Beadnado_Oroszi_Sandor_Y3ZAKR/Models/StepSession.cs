namespace Beadnado_Oroszi_Sandor_Y3ZAKR.Models;

/// <summary>
/// Egy befejezett mérés (edzés) adatai – ez a Model réteg.
/// </summary>
public class StepSession
{
    public DateTime StartTime { get; init; }
    public TimeSpan Duration { get; init; }
    public int Steps { get; init; }
    public bool GoalReached { get; init; }

    public string DurationText => Duration.ToString(@"mm\:ss");
}
