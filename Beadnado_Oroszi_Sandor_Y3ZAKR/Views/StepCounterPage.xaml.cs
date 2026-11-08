using Beadnado_Oroszi_Sandor_Y3ZAKR.ViewModels;

namespace Beadnado_Oroszi_Sandor_Y3ZAKR.Views;

/// <summary>
/// Code-behind: csak továbbítja a gombnyomásokat a ViewModelnek
/// (Commands helyett egyszerű Clicked eseménykezelők). Logika nincs itt.
/// </summary>
public partial class StepCounterPage : ContentPage
{
    private StepCounterViewModel ViewModel => (StepCounterViewModel)BindingContext;

    public StepCounterPage()
    {
        InitializeComponent();
    }

    private void OnStartClicked(object? sender, EventArgs e) => ViewModel.Start();
    private void OnStopClicked(object? sender, EventArgs e) => ViewModel.Stop();
    private void OnResetClicked(object? sender, EventArgs e) => ViewModel.Reset();
    private void OnSimulateClicked(object? sender, EventArgs e) => ViewModel.SimulateStep();
}
