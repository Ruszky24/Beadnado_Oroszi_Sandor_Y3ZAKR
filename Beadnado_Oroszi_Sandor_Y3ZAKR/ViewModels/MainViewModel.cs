using System.Windows.Input;
using Beadnado_Oroszi_Sandor_Y3ZAKR.Models;

namespace Beadnado_Oroszi_Sandor_Y3ZAKR.ViewModels;

/// <summary>
/// A főoldal ViewModelje: a megjelenített adatok, állapotok, számítások és műveletek
/// (helymeghatározás, gyorsulásmérő, iránytű). A nézet csak adatkötéssel kapcsolódik hozzá.
/// </summary>
public class MainViewModel : ViewModelBase
{
    public const string Dash = "–";

    public MainViewModel()
    {
        GetLocationCommand = new Command(async () => await GetLocationAsync(), () => !IsBusy);
    }

    // =====================================================================
    // 1. HELYMEGHATÁROZÁS
    // =====================================================================

    private double? _latitude;
    public double? Latitude { get => _latitude; private set => SetProperty(ref _latitude, value); }

    private double? _longitude;
    public double? Longitude { get => _longitude; private set => SetProperty(ref _longitude, value); }

    private double? _accuracy;
    /// <summary>A helyadat pontossága méterben (extra információ).</summary>
    public double? Accuracy { get => _accuracy; private set => SetProperty(ref _accuracy, value); }

    private string _locationStatus = "Még nincs lekérdezés";
    public string LocationStatus { get => _locationStatus; private set => SetProperty(ref _locationStatus, value); }

    private bool _isLocationError;
    public bool IsLocationError { get => _isLocationError; private set => SetProperty(ref _isLocationError, value); }

    private bool _isLocationSuccess;
    public bool IsLocationSuccess { get => _isLocationSuccess; private set => SetProperty(ref _isLocationSuccess, value); }

    private string _lastLocationTime = Dash;
    public string LastLocationTime { get => _lastLocationTime; private set => SetProperty(ref _lastLocationTime, value); }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
                ((Command)GetLocationCommand).ChangeCanExecute();
        }
    }

    public ICommand GetLocationCommand { get; }

    /// <summary>
    /// Az aktuális pozíció lekérdezése a .NET MAUI Geolocation API-jával,
    /// az Android helymeghatározási engedély kérésével és teljes hibakezeléssel.
    /// </summary>
    public async Task GetLocationAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        SetLocationState("Helyzet lekérdezése folyamatban…", isError: false, isSuccess: false);

        try
        {
            // 1) Engedély ellenőrzése és szükség esetén kérése
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                SetLocationState("Hiba: a helymeghatározási engedély nincs megadva.", true, false);
                return;
            }

            // 2) Aktuális pozíció lekérdezése (ha nem jön, az utolsó ismert pozíció)
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request)
                           ?? await Geolocation.Default.GetLastKnownLocationAsync();

            if (location is null)
            {
                SetLocationState("Hiba: nem érhető el helyadat. Próbálja újra szabad ég alatt.", true, false);
                return;
            }

            Latitude = location.Latitude;
            Longitude = location.Longitude;
            Accuracy = location.Accuracy;
            LastLocationTime = DateTime.Now.ToString("HH:mm:ss");
            SetLocationState("Helymeghatározás sikeres", false, true);
        }
        catch (FeatureNotSupportedException)
        {
            SetLocationState("Hiba: a helymeghatározás nem támogatott ezen az eszközön.", true, false);
        }
        catch (FeatureNotEnabledException)
        {
            SetLocationState("Hiba: a helymeghatározás ki van kapcsolva az eszközön.", true, false);
        }
        catch (PermissionException)
        {
            SetLocationState("Hiba: a helymeghatározási engedély nincs megadva.", true, false);
        }
        catch (Exception ex)
        {
            SetLocationState($"Hiba: {ex.Message}", true, false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetLocationState(string message, bool isError, bool isSuccess)
    {
        LocationStatus = message;
        IsLocationError = isError;
        IsLocationSuccess = isSuccess;
    }

    // =====================================================================
    // 2. GYORSULÁSMÉRŐ
    // =====================================================================

    private const double GaugeRangeG = 2.0; // a tengely-sávok ±2 g-ig skáláznak

    private MotionData? _motion;

    public double? X => _motion?.X;
    public double? Y => _motion?.Y;
    public double? Z => _motion?.Z;
    public double? Magnitude => _motion?.Magnitude;
    public double? Deviation => _motion?.Deviation;

    // Középről induló sávok: a negatív és a pozitív irány külön sávot kap
    public double XNegative => Bar(-(X ?? 0));
    public double XPositive => Bar(X ?? 0);
    public double YNegative => Bar(-(Y ?? 0));
    public double YPositive => Bar(Y ?? 0);
    public double ZNegative => Bar(-(Z ?? 0));
    public double ZPositive => Bar(Z ?? 0);

    /// <summary>A d eltérés sávja (0 … 1 g).</summary>
    public double DeviationBar => Math.Clamp(Deviation ?? 0, 0, 1);

    private static double Bar(double value) => Math.Clamp(value / GaugeRangeG, 0, 1);

    private string _motionState = Dash;
    public string MotionState { get => _motionState; private set => SetProperty(ref _motionState, value); }

    private string _accelerometerStatus = "Kikapcsolva";
    public string AccelerometerStatus { get => _accelerometerStatus; private set => SetProperty(ref _accelerometerStatus, value); }

    private bool _isAccelerometerOn;
    /// <summary>A Switch IsToggled tulajdonságához TwoWay kötéssel kapcsolódik.</summary>
    public bool IsAccelerometerOn
    {
        get => _isAccelerometerOn;
        set
        {
            if (!SetProperty(ref _isAccelerometerOn, value))
                return;

            if (value) StartAccelerometer();
            else StopAccelerometer();

            OnPropertyChanged(nameof(ActiveSensorsText));
        }
    }

    /// <summary>Mozgásbesorolás a d = |a − 1| érték alapján.</summary>
    public static string ClassifyMotion(double d)
    {
        if (d < 0.15) return "Nyugalomban";
        if (d < 0.50) return "Enyhe mozgás";
        return "Erős mozgás";
    }

    private void StartAccelerometer()
    {
        try
        {
            if (!Accelerometer.Default.IsSupported)
            {
                AccelerometerStatus = "A gyorsulásmérő nem támogatott ezen az eszközön.";
                ResetSwitch(() => _isAccelerometerOn = false, nameof(IsAccelerometerOn));
                return;
            }

            if (!Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
            AccelerometerStatus = "Mérés folyamatban…";
        }
        catch (FeatureNotSupportedException)
        {
            AccelerometerStatus = "A gyorsulásmérő nem támogatott ezen az eszközön.";
            ResetSwitch(() => _isAccelerometerOn = false, nameof(IsAccelerometerOn));
        }
        catch (Exception ex)
        {
            AccelerometerStatus = $"Hiba: {ex.Message}";
            ResetSwitch(() => _isAccelerometerOn = false, nameof(IsAccelerometerOn));
        }
    }

    private void StopAccelerometer()
    {
        try
        {
            if (Accelerometer.Default.IsMonitoring)
                Accelerometer.Default.Stop();
        }
        catch (Exception) { /* leállításkor nincs teendő */ }

        Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;

        // Az utolsó értékek láthatók maradnak, csak az állapot változik
        if (AccelerometerStatus == "Mérés folyamatban…")
            AccelerometerStatus = "Leállítva – az utolsó mért értékek láthatók";
    }

    private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var a = e.Reading.Acceleration;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // a leállítás után még sorban álló mérések ne írják felül az értékeket
            if (!IsAccelerometerOn)
                return;

            UpdateMotion(new MotionData(a.X, a.Y, a.Z));
        });
    }

    private void UpdateMotion(MotionData data)
    {
        _motion = data;
        MotionState = ClassifyMotion(data.Deviation);

        OnPropertyChanged(nameof(X));
        OnPropertyChanged(nameof(Y));
        OnPropertyChanged(nameof(Z));
        OnPropertyChanged(nameof(Magnitude));
        OnPropertyChanged(nameof(Deviation));
        OnPropertyChanged(nameof(DeviationBar));
        OnPropertyChanged(nameof(XNegative));
        OnPropertyChanged(nameof(XPositive));
        OnPropertyChanged(nameof(YNegative));
        OnPropertyChanged(nameof(YPositive));
        OnPropertyChanged(nameof(ZNegative));
        OnPropertyChanged(nameof(ZPositive));
    }

    // =====================================================================
    // 3. IRÁNYTŰ
    // =====================================================================

    private static readonly string[] CardinalNames =
    {
        "Észak", "Északkelet", "Kelet", "Délkelet",
        "Dél", "Délnyugat", "Nyugat", "Északnyugat"
    };

    private int? _heading;
    /// <summary>Mágneses irány egész fokra kerekítve (0–359).</summary>
    public int? Heading
    {
        get => _heading;
        private set
        {
            if (SetProperty(ref _heading, value))
                OnPropertyChanged(nameof(DialRotation));
        }
    }

    /// <summary>Az iránytű-lapot az irány ellentettjével forgatjuk, így az „É" mindig északra néz.</summary>
    public double DialRotation => -(Heading ?? 0);

    private string _cardinalDirection = Dash;
    public string CardinalDirection { get => _cardinalDirection; private set => SetProperty(ref _cardinalDirection, value); }

    private string _compassStatus = "Kikapcsolva";
    public string CompassStatus { get => _compassStatus; private set => SetProperty(ref _compassStatus, value); }

    private bool _isCompassOn;
    /// <summary>A Switch IsToggled tulajdonságához TwoWay kötéssel kapcsolódik.</summary>
    public bool IsCompassOn
    {
        get => _isCompassOn;
        set
        {
            if (!SetProperty(ref _isCompassOn, value))
                return;

            if (value) StartCompass();
            else StopCompass();

            OnPropertyChanged(nameof(ActiveSensorsText));
        }
    }

    /// <summary>
    /// Fokérték → égtáj. A tartomány alsó határa a tartományhoz tartozik,
    /// a felső a következőhöz; a 360°-ot 0°-ként kezeljük.
    /// </summary>
    public static string ToCardinal(double degrees)
    {
        double normalized = ((degrees % 360) + 360) % 360;      // 0 ≤ x < 360
        int index = (int)Math.Floor((normalized + 22.5) / 45.0) % 8;
        return CardinalNames[index];
    }

    private void StartCompass()
    {
        try
        {
            if (!Compass.Default.IsSupported)
            {
                CompassStatus = "Az iránytű nem támogatott ezen az eszközön.";
                ResetSwitch(() => _isCompassOn = false, nameof(IsCompassOn));
                return;
            }

            if (!Compass.Default.IsMonitoring)
            {
                Compass.Default.ReadingChanged += OnCompassReadingChanged;
                Compass.Default.Start(SensorSpeed.UI, applyLowPassFilter: true);
            }
            CompassStatus = "Mérés folyamatban…";
        }
        catch (FeatureNotSupportedException)
        {
            CompassStatus = "Az iránytű nem támogatott ezen az eszközön.";
            ResetSwitch(() => _isCompassOn = false, nameof(IsCompassOn));
        }
        catch (Exception ex)
        {
            CompassStatus = $"Hiba: {ex.Message}";
            ResetSwitch(() => _isCompassOn = false, nameof(IsCompassOn));
        }
    }

    private void StopCompass()
    {
        try
        {
            if (Compass.Default.IsMonitoring)
                Compass.Default.Stop();
        }
        catch (Exception) { /* leállításkor nincs teendő */ }

        Compass.Default.ReadingChanged -= OnCompassReadingChanged;

        if (CompassStatus == "Mérés folyamatban…")
            CompassStatus = "Leállítva – az utolsó irány látható";
    }

    private void OnCompassReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        double raw = e.Reading.HeadingMagneticNorth;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!IsCompassOn)
                return;

            int rounded = (int)Math.Round(raw, MidpointRounding.AwayFromZero) % 360; // 360° → 0°
            Heading = rounded;
            CardinalDirection = ToCardinal(raw);
        });
    }

    // =====================================================================
    // Közös
    // =====================================================================

    public string ActiveSensorsText =>
        $"{(IsAccelerometerOn ? 1 : 0) + (IsCompassOn ? 1 : 0)}/2 szenzor aktív";

    /// <summary>Mindkét szenzor leállítása (pl. amikor az oldal eltűnik).</summary>
    public void StopAllSensors()
    {
        IsAccelerometerOn = false;
        IsCompassOn = false;
    }

    /// <summary>
    /// Ha a szenzor nem indítható, a kapcsolót visszaállítjuk kikapcsoltra.
    /// A visszaállítás a következő UI-ciklusban történik, hogy a Switch biztosan frissüljön.
    /// </summary>
    private void ResetSwitch(Action resetField, string propertyName)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            resetField();
            OnPropertyChanged(propertyName);
            OnPropertyChanged(nameof(ActiveSensorsText));
        });
    }
}
