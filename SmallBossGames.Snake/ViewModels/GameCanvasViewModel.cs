using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaNativeApplication1.ViewModels
{
    internal partial class GameCanvasViewModel: ObservableObject
    {
        [ObservableProperty]
        private double _squareSize = 10;
       
        [ObservableProperty]
        private int _snakeLength = 0;
    }
}
