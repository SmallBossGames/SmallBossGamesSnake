using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData.Binding;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaNativeApplication1.Views
{
    public enum MovingDirection
    {
        Up, Down, Left, Right
    }

    public struct HeadState
    {
        public MovingDirection movingDirection;
        public uint xIndex;
        public uint yIndex;
    }

    public partial class GameCanvas : Window
    {
        private readonly uint blocks = 10;

        private readonly ImmutableArray<Ellipse> _debugPoints;
        private readonly Shape _head;

        private readonly CancellationTokenSource _gameLoopTokenSource = new();


        private HeadState _headState;
        private MovingDirection _nextDirection;


        public GameCanvas()
        {
            InitializeComponent();

            _debugPoints = CreateDebugPoints();
            GrassField.Children.AddRange(_debugPoints);

            _head = CreateHead();
            _headState = new HeadState()
            {
                movingDirection = MovingDirection.Up,
                xIndex = 0,
                yIndex = 0,
            };
            _nextDirection = MovingDirection.Up;
            GrassField.Children.Add(_head);
            RedrawHead();
            
            GrassField.SizeChanged += GrassField_SizeChanged;

            _ = StartGameLoopAsync(_gameLoopTokenSource.Token);
            Closing += GameCanvas_Closing;

            KeyDown += GameCanvas_KeyDown;
        }

        private void GameCanvas_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _gameLoopTokenSource.Cancel();
        }

        private void GameCanvas_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Avalonia.Input.Key.Up when _headState.movingDirection is not MovingDirection.Down:
                    _nextDirection = MovingDirection.Up;
                    break;
                case Avalonia.Input.Key.Down when _headState.movingDirection is not MovingDirection.Up:
                    _nextDirection = MovingDirection.Down;
                    break;
                case Avalonia.Input.Key.Left when _headState.movingDirection is not MovingDirection.Right:
                    _nextDirection = MovingDirection.Left;
                    break;
                case Avalonia.Input.Key.Right when _headState.movingDirection is not MovingDirection.Left:
                    _nextDirection = MovingDirection.Right;
                    break;
            }
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

            for (uint i = 0; i < blocks; i++)
            {
                for (uint j = 0; j < blocks; j++)
                {
                    var dot = _debugPoints[(int)(blocks * i + j)];

                    Canvas.SetLeft(dot, step * i + offset - dot.Width / 2);
                    Canvas.SetTop(dot, step * j + offset - dot.Height/2);
                }
            }
        }

        public void RedrawHead()
        {
            var head = _head;
            var state = _headState;
            var squareSize = GrassField.DesiredSize.Width;
            var step = squareSize / blocks;
            var offset = step / 2;

            Canvas.SetLeft(head, state.xIndex * step + offset - head.Width / 2);
            Canvas.SetTop(head, state.yIndex * step + offset - head.Height / 2);
        }

        private ImmutableArray<Ellipse> CreateDebugPoints()
        {
            return Enumerable
                .Range(0, (int)(blocks * blocks))
                .Select(x =>
                new Ellipse()
                {
                    Fill = Brushes.Blue,
                    Width = 2,
                    Height = 2,
                })
                .ToImmutableArray();
        }

        private Ellipse CreateHead()
        {
            return new Ellipse()
            {
                Fill = Brushes.Orange,
                Width = 10,
                Height = 10,
            };
        }

        private async Task StartGameLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var nextState = _headState;

                nextState.movingDirection = _nextDirection;

                switch (nextState.movingDirection)
                {
                    case MovingDirection.Up:
                        nextState.yIndex = LoopDecrease(nextState.yIndex, blocks);
                        break;
                    case MovingDirection.Down:
                        nextState.yIndex = LoopIncrease(nextState.yIndex, blocks);
                        break;
                    case MovingDirection.Left:
                        nextState.xIndex = LoopDecrease(nextState.xIndex, blocks);
                        break;
                    case MovingDirection.Right:
                        nextState.xIndex = LoopIncrease(nextState.xIndex, blocks);
                        break;
                }

                _headState = nextState;

                RedrawHead();

                await Task.Delay(500, cancellationToken);
            }
        }

        private static uint LoopDecrease(uint value, uint maxValue)
        {
            return value == 0 ? maxValue - 1 : value - 1;
        }

        private static uint LoopIncrease(uint value, uint maxValue)
        {
            return value == (maxValue - 1) ? 0 : value + 1;
        }
    }
}
