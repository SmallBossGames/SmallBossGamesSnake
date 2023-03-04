using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using DynamicData.Binding;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace AvaloniaNativeApplication1.Views
{
    public partial class GameCanvas : Window
    {
        private readonly int blocks = 10;

        private readonly ImmutableArray<Ellipse> _debugPoints;

        public GameCanvas()
        {
            InitializeComponent();

            _debugPoints = CreateDebugPoints();

            GrassField.Children.AddRange(_debugPoints);
            GrassField.SizeChanged += GrassField_SizeChanged;
        }

        private void GrassField_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged && sender is Canvas canvas)
            {
                var squareSize = e.NewSize.Height;

                canvas.Width = squareSize;

                RedrawGrid(squareSize);
            }
        }

        public void RedrawGrid(double squareSize) {
            var step = squareSize / blocks;
            var offset = step / 2;

            for (int i = 0; i < blocks; i++)
            {
                for (int j = 0; j < blocks; j++)
                {
                    var dot = _debugPoints[blocks * i + j];

                    Canvas.SetLeft(dot, step * i + offset - dot.Width / 2);

                    Canvas.SetTop(dot, step * j + offset - dot.Height/2);
                }
            }
        }

        private ImmutableArray<Ellipse> CreateDebugPoints()
        {
            return Enumerable
                .Range(0, blocks * blocks).Select(x =>
                new Ellipse()
                {
                    Fill = Brushes.Blue,
                    Width = 5,
                    Height = 5,
                    
                })
                .ToImmutableArray();
        }
    }
}
