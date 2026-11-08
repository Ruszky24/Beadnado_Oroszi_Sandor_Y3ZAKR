using System.Collections.ObjectModel;
using Beadnado_Oroszi_Sandor_Y3ZAKR.Models;

namespace Beadnado_Oroszi_Sandor_Y3ZAKR.ViewModels;

/// <summary>
/// A lépésszámláló oldal ViewModelje.
/// A gyorsulásmérő (Accelerometer) adataiból egyszerű csúcsdetektálással számol lépéseket.
/// </summary>
public class StepCounterViewModel : ViewModelBase
{
    // Lépésdetektálás paraméterei (g egységben; nyugalomban kb. 1 g)
    private const double UpperThreshold = 1.20;
    private const double LowerThreshold = 1.05;
    private const int MinStepIntervalMs = 300;

    private double _filteredMagnitude = 1.0;
    private bool _isAboveThreshold;
    private DateTime _lastStepTime = DateTime.MinValue;
    private DateTime _sessionStart;

    // ---------- Kötött tulajdonságok ----------

    private double _x;
    public double X { get => _x; set => SetProperty(ref _x, value); }

    private double _y;
    public double Y { get => _y; set => SetProperty(ref _y, value); }

    private double _z;
    public double Z { get => _z; set => SetProperty(ref _z, value); }

    private double _magnitude;
    public double Magnitude { get => _magnitude; set => SetProperty(ref _magnitude, value); }

    private int _steps;
    public int Steps
    {
        get => _steps;
        set
        {
            if (SetProperty(ref _steps, value))
            {
                // Származtatott tulajdonságok: ezek is változnak, ha a Steps változik
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(IsGoalReached));
                OnPropertyChanged(nameof(ProgressText));
                OnPropertyChanged(nameof(DistanceKm));
            }
        }
    }

    private int _goal = 50;
    /// <summary>Kétirányú (TwoWay) kötés: Entry és Slider is ezt módosítja.</summary>
    public int Goal
    {
        get => _goal;
        set
        {
            if (value < 1) value = 1;
            if (SetProperty(ref _goal, value))
            {
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(IsGoalReached));
                OnPropertyChanged(nameof(ProgressText));
            }
        }
    }

    private double _stepLengthCm = 70;
    public double StepLengthCm
    {
        get => _stepLengthCm;
        set
        {
            if (SetProperty(ref _stepLengthCm, value))
                OnPropertyChanged(nameof(DistanceKm));
        }
    }

    private bool _isRunning;
    public bool IsRunning { get => _isRunning; set => SetProperty(ref _isRunning, value); }

    private string _statusMessage = "Nyomd meg a Start gombot!";
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    // Csak olvasható, számolt tulajdonságok
    public double Progress => Math.Min(1.0, (double)Steps / Goal);
    public bool IsGoalReached => Steps >= Goal;
    public string ProgressText => $"{Steps} / {Goal} lépés ({Progress:P0})";
    public double DistanceKm => Steps * StepLengthCm / 100_000.0;

    /// <summary>Korábbi mérések – a CollectionView ehhez kötődik.</summary>
    public ObservableCollection<StepSession> Sessions { get; } = new();

    // ---------- Műveletek (a nézet eseménykezelői hívják) ----------

    public void Start()
    {
        if (!Accelerometer.Default.IsSupported)
        {
            StatusMessage = "Ezen az eszközön nincs gyorsulásmérő.";
            return;
        }

        if (Accelerometer.Default.IsMonitoring)
            return;

        Steps = 0;
        _sessionStart = DateTime.Now;
        Accelerometer.Default.ReadingChanged += OnReadingChanged;
        Accelerometer.Default.Start(SensorSpeed.Game);

        IsRunning = true;
        StatusMessage = "Mérés folyamatban…";
    }

    public void Stop()
    {
        if (!IsRunning)
            return;

        Accelerometer.Default.Stop();
        Accelerometer.Default.ReadingChanged -= OnReadingChanged;
        IsRunning = false;

        // Az elkészült mérést elmentjük a listába (a lista eleje = legújabb)
        Sessions.Insert(0, new StepSession
        {
            StartTime = _sessionStart,
            Duration = DateTime.Now - _sessionStart,
            Steps = Steps,
            GoalReached = IsGoalReached
        });

        StatusMessage = IsGoalReached ? "Gratulálok, elérted a célt! 🎉" : "Mérés leállítva.";
    }

    public void Reset()
    {
        Stop();
        Steps = 0;
        StatusMessage = "Nullázva.";
    }

    /// <summary>Emulátoron nehéz lépést szimulálni, ezért van egy teszt gomb is.</summary>
    public void SimulateStep() => Steps++;

    // ---------- Szenzor feldolgozás ----------

    private void OnReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var a = e.Reading.Acceleration;
        double magnitude = Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);

        // A UI-t mindig a fő szálon frissítjük
        MainThread.BeginInvokeOnMainThread(() =>
        {
            X = a.X;
            Y = a.Y;
            Z = a.Z;
            Magnitude = magnitude;
            DetectStep(magnitude);
        });
    }

    private void DetectStep(double magnitude)
    {
        // Egyszerű aluláteresztő szűrő a zaj csökkentésére
        _filteredMagnitude = _filteredMagnitude * 0.8 + magnitude * 0.2;

        var now = DateTime.Now;
        if (!_isAboveThreshold && _filteredMagnitude > UpperThreshold
            && (now - _lastStepTime).TotalMilliseconds > MinStepIntervalMs)
        {
            _isAboveThreshold = true;
            _lastStepTime = now;
            Steps++;

            if (Steps == Goal)
                StatusMessage = "Cél elérve! 🎉";
        }
        else if (_isAboveThreshold && _filteredMagnitude < LowerThreshold)
        {
            _isAboveThreshold = false;
        }
    }
}
