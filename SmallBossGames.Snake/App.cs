using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using SmallBossGames.Snake.ViewModels;
using SmallBossGames.Snake.Views;

namespace SmallBossGames.Snake;

public partial class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        DataTemplates.Add(new ViewLocator());
        RequestedThemeVariant = ThemeVariant.Default;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new MainWindow()
                    .Content(new GameCanvasView(new GameCanvasViewModel()));
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new MainWindow()
                    .Content(new GameCanvasView(new GameCanvasViewModel()));
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}