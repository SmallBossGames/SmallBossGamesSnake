using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaNativeApplication1.ViewModels;
using DynamicData.Binding;
using System;
using System.Collections.Generic;
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

    public struct IndexedPoint 
    { 
        public uint xIndex;
        public uint yIndex;
    }

    public struct HeadState
    {
        public MovingDirection movingDirection;
        public uint xIndex;
        public uint yIndex;
    }

    public struct TailState
    {
        public IndexedPoint[] points;
        public uint startIndex;
    }

    public struct AppleState
    {
        public uint xIndex;
        public uint yIndex;
    }

    public partial class GameCanvas : Window
    {
        private const int TickDelay = 100;

        private const uint Blocks = 50;

        private readonly ImmutableArray<Ellipse> _debugPoints;

        private readonly CancellationTokenSource _gameLoopTokenSource = new();

        private readonly Shape _head;
        private HeadState _headState;
        private MovingDirection _nextDirection;

        private readonly List<Polyline> _tailLines = new();
        private TailState _tailState;

        private AppleState _appleState;
        private readonly Shape _apple;

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

            _tailState = new TailState()
            {
                points = new IndexedPoint[2],
                startIndex = 0,
            };
            RedrawTail();

            _apple = CreateApple();
            _appleState = new AppleState();
            GrassField.Children.Add(_apple);
            PlaceApple();

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

            }

            UpdateScore();

            RedrawGrid();
            RedrawTail();
            RedrawHead();
            RedrawApple();
        }

        public void RedrawGrid() {
            var squareSize = GrassField.DesiredSize.Width;
            var step = squareSize / Blocks;
            var offset = step / 2;

            for (uint i = 0; i < Blocks; i++)
            {
                for (uint j = 0; j < Blocks; j++)
                {
                    var dot = _debugPoints[(int)(Blocks * i + j)];

                    Canvas.SetLeft(dot, step * i + offset - dot.Width / 2);
                    Canvas.SetTop(dot, step * j + offset - dot.Height/2);
                }
            }
        }

        public Polyline CreateTail(IList<Point> points)
        {
            return new Polyline()
            {
                Stroke = Brushes.Orange,
                StrokeThickness = 6,
                Points = points
            };
        }

        public void RedrawHead()
        {
            var head = _head;
            var state = _headState;
            var squareSize = GrassField.DesiredSize.Width;
            var step = squareSize / Blocks;
            var offset = step / 2;

            Canvas.SetLeft(head, state.xIndex * step + offset - head.Width / 2);
            Canvas.SetTop(head, state.yIndex * step + offset - head.Height / 2);
        }

        public void RedrawTail()
        {
            var squareSize = GrassField.DesiredSize.Width;
            var step = squareSize / Blocks;
            var offset = step / 2;

            var newPoints = new List<Point>(_tailState.points.Length);
            var lastPoint = (IndexedPoint?)null;
            var fragments = new List<Polyline>();

            for (int i = 0; i < _tailState.points.Length; i++)
            {
                var index = (i + _tailState.startIndex) % _tailState.points.Length;
                var point = _tailState.points[index];

                if (lastPoint is null)
                {
                    newPoints.Add(new Point(point.xIndex * step + offset, point.yIndex * step + offset));
                    lastPoint = point;

                }
                else if (lastPoint is IndexedPoint ip)
                {
                    if (Math.Abs((int)ip.xIndex - (int)point.xIndex) > 1 || Math.Abs((int)ip.yIndex - (int)point.yIndex) > 1)
                    {
                        var fragment = CreateTail(newPoints.ToList());
                        newPoints.Clear();

                        fragments.Add(fragment);
                    }

                    newPoints.Add(new Point(point.xIndex * step + offset, point.yIndex * step + offset));
                    lastPoint = point;
                }
            }

            var lastFragment = CreateTail(newPoints);

            fragments.Add(lastFragment);

            var oldFragments = _tailLines.ToArray();
            
            _tailLines.Clear();
            _tailLines.AddRange(fragments);

            GrassField.Children.RemoveAll(oldFragments);
            GrassField.Children.AddRange(fragments);
        }

        private ImmutableArray<Ellipse> CreateDebugPoints()
        {
            return Enumerable
                .Range(0, (int)(Blocks * Blocks))
                .Select(x =>
                new Ellipse()
                {
                    Fill = Brushes.GreenYellow,
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

        private Ellipse CreateApple()
        {
            return new Ellipse()
            {
                Fill = Brushes.Green,
                Width = 8,
                Height = 8,
            };
        }

        private void PlaceApple()
        {
            var allPoints = Enumerable.Range(0, (int)(Blocks * Blocks)).ToHashSet();
            var snakePoints = _tailState.points.Select(x => (int)(x.xIndex * Blocks + x.yIndex)).ToHashSet();

            allPoints.ExceptWith(snakePoints);
            var availablePoints = allPoints.ToImmutableArray();

            var index = Random.Shared.Next(availablePoints.Length);
            var pointX = availablePoints[index] / Blocks;
            var pointY = availablePoints[index] - pointX * Blocks;
            var state = new AppleState() 
            {
                xIndex = (uint)pointX,
                yIndex = (uint)pointY,
            };

            _appleState = state;

        }

        private void RedrawApple()
        {
            var apple = _apple;
            var state = _appleState;
            var squareSize = GrassField.DesiredSize.Width;
            var step = squareSize / Blocks;
            var offset = step / 2;

            Canvas.SetLeft(apple, state.xIndex * step + offset - apple.Width / 2);
            Canvas.SetTop(apple, state.yIndex * step + offset - apple.Height / 2);
        }

        private async Task StartGameLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var nextHeadState = _headState;

                nextHeadState.movingDirection = _nextDirection;

                switch (nextHeadState.movingDirection)
                {
                    case MovingDirection.Up:
                        nextHeadState.yIndex = LoopDecrease(nextHeadState.yIndex, Blocks);
                        break;
                    case MovingDirection.Down:
                        nextHeadState.yIndex = LoopIncrease(nextHeadState.yIndex, Blocks);
                        break;
                    case MovingDirection.Left:
                        nextHeadState.xIndex = LoopDecrease(nextHeadState.xIndex, Blocks);
                        break;
                    case MovingDirection.Right:
                        nextHeadState.xIndex = LoopIncrease(nextHeadState.xIndex, Blocks);
                        break;
                }

                if(nextHeadState.xIndex == _appleState.xIndex && nextHeadState.yIndex == _appleState.yIndex)
                {

                }

                _headState = nextHeadState;

                var nextTailState = _tailState;
                var nextPointIndex = nextTailState.startIndex;

                static IndexedPoint[] CreatePoints(TailState tailState, HeadState headState)
                {
                    var oldPoints = tailState.points.AsSpan();
                    var newPoints = new IndexedPoint[tailState.points.Length + 1];
                    var newPointsSpan = newPoints.AsSpan();


                    var firstPart = oldPoints[(int)tailState.startIndex..];
                    var secondPart = oldPoints[..(int)tailState.startIndex];

                    firstPart.CopyTo(newPointsSpan);
                    secondPart.CopyTo(newPointsSpan[firstPart.Length..]);
                    newPointsSpan[^1] = new IndexedPoint
                    {
                        xIndex = headState.xIndex,
                        yIndex = headState.yIndex,
                    };

                    return newPoints;
                }

                if (nextHeadState.xIndex == _appleState.xIndex && nextHeadState.yIndex == _appleState.yIndex)
                {
                    nextTailState.points = CreatePoints(nextTailState, nextHeadState);
                    nextTailState.startIndex = 0;

                    _tailState = nextTailState;

                    UpdateScore();

                    PlaceApple();
                    RedrawApple();
                    RedrawTail();
                    RedrawHead();
                }
                else
                {
                    nextTailState.startIndex = LoopIncrease(nextPointIndex, (uint)nextTailState.points.Length);
                    nextTailState.points[nextPointIndex] = new IndexedPoint
                    {
                        xIndex = nextHeadState.xIndex,
                        yIndex = nextHeadState.yIndex,
                    };

                    _tailState = nextTailState;

                    RedrawTail();
                    RedrawHead();
                }

                

                await Task.Delay(TickDelay, cancellationToken);
            }
        }

        private void UpdateScore()
        {
            if (DataContext is GameCanvasViewModel vm)
            {
                var snakeLength = _tailState.points.Length - 1;

                vm.SnakeLength = snakeLength;
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
