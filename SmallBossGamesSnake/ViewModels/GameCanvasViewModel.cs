using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
