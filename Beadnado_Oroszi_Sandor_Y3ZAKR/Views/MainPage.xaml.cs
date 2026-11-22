using Beadnado_Oroszi_Sandor_Y3ZAKR.ViewModels;

namespace Beadnado_Oroszi_Sandor_Y3ZAKR.Views;

/// <summary>
/// A főoldal code-behind fájlja. Csak a nézet és a ViewModel összekötését végzi;
/// minden adatkezelési és állapotlogika a MainViewModel-ben van.
/// </summary>
public partial class MainPage : ContentPage
{
    private MainViewModel ViewModel => (MainViewModel)BindingContext;

    public MainPage()
    {
        InitializeComponent();
    }

    // Ha az oldal eltűnik (pl. az alkalmazás háttérbe kerül), a szenzorokat leállítjuk.
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ViewModel.StopAllSensors();
    }
}
