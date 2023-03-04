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

        public double SquareSize { get => _squareSize; set => this.RaiseAndSetIfChanged(ref _squareSize, value); }
    }
}
