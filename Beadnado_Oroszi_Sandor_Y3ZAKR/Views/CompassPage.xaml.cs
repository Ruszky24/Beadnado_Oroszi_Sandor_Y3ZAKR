using Beadnado_Oroszi_Sandor_Y3ZAKR.ViewModels;

namespace Beadnado_Oroszi_Sandor_Y3ZAKR.Views;

public partial class CompassPage : ContentPage
{
    private CompassViewModel ViewModel => (CompassViewModel)BindingContext;

    public CompassPage()
    {
        InitializeComponent();
    }

    // A szenzort csak akkor figyeljük, amikor az oldal látszik (akkumulátorkímélés)
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ViewModel.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ViewModel.Stop();
    }
}
