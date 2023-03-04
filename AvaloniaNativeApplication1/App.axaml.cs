using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaNativeApplication1.ViewModels;
using AvaloniaNativeApplication1.Views;

namespace AvaloniaNativeApplication1
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new GameCanvas
                {
                    DataContext = new GameCanvasViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}