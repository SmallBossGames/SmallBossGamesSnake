using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SmallBossGames.Snake.Views;

public class MainWindow : Window
{
    public MainWindow()
    {
        Title = "SmallBossGames': The Snake";
        Icon = new WindowIcon(
            AssetLoader.Open(
                new Uri("avares://SmallBossGames.Snake/Assets/avalonia-logo.ico")
            )
        );
    }
}