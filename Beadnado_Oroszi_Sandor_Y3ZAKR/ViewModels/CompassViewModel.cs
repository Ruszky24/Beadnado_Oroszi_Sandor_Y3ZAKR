namespace Beadnado_Oroszi_Sandor_Y3ZAKR.ViewModels;

/// <summary>
/// Az iránytű oldal ViewModelje (Compass szenzor = magnetométer + gyorsulásmérő).
/// </summary>
public class CompassViewModel : ViewModelBase
{
    private static readonly string[] Directions = { "É", "ÉK", "K", "DK", "D", "DNy", "Ny", "ÉNy" };

    private double _heading;
    public double Heading
    {
        get => _heading;
        set
        {
            if (SetProperty(ref _heading, value))
            {
                OnPropertyChanged(nameof(NeedleRotation));
                OnPropertyChanged(nameof(DirectionText));
                OnPropertyChanged(nameof(IsFacingNorth));
            }
        }
    }

    /// <summary>A tűt az irány ellentettjével forgatjuk, így mindig északra mutat.</summary>
    public double NeedleRotation => -Heading;

    public string DirectionText => Directions[(int)Math.Round(Heading / 45.0) % 8];

    public bool IsFacingNorth => Heading < 10 || Heading > 350;

    private string _statusMessage = "";
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    public void Start()
    {
        if (!Compass.Default.IsSupported)
        {
            StatusMessage = "Ezen az eszközön nincs iránytű szenzor.";
            return;
        }

        if (Compass.Default.IsMonitoring)
            return;

        Compass.Default.ReadingChanged += OnReadingChanged;
        Compass.Default.Start(SensorSpeed.UI, applyLowPassFilter: true);
        StatusMessage = "Forgasd a telefont!";
    }

    public void Stop()
    {
        if (!Compass.Default.IsMonitoring)
            return;

        Compass.Default.Stop();
        Compass.Default.ReadingChanged -= OnReadingChanged;
    }

    private void OnReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => Heading = e.Reading.HeadingMagneticNorth);
    }
}
