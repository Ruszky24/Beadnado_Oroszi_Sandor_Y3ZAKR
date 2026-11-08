using Microsoft.Extensions.DependencyInjection;

namespace Beadnado_Oroszi_Sandor_Y3ZAKR
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}