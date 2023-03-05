using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaNativeApplication1.ViewModels
{
    internal class GameCanvasViewModel: ViewModelBase
    {
        private double _squareSize = 10;
        private int _snakeLength = 0;

        public double SquareSize { get => _squareSize; set => this.RaiseAndSetIfChanged(ref _squareSize, value); }

        public int SnakeLength { get => _snakeLength; set => this.RaiseAndSetIfChanged(ref _snakeLength, value); }
    }
}
