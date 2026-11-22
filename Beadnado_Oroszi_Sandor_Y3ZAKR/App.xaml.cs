namespace Beadnado_Oroszi_Sandor_Y3ZAKR
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // A felület sötét témára készült, ezért rögzítjük
            UserAppTheme = AppTheme.Dark;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
