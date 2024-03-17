using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using SmallBossGames.Snake.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmallBossGames.Snake.Views;

public struct IndexedPoint 
{ 
    public uint xIndex;
    public uint yIndex;
}

public struct HeadState
{
    public Moving moving;
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

public record struct Moving(int X, int Y)
{
    public static Moving Up => new(0, -1);
    public static Moving Down => new(0, 1);
    public static Moving Left => new(-1, 0);
    public static Moving Right => new(1, 0);
}

public partial class GameCanvasView : UserControl
{
    private const int TickDelay = 150;

    private const uint Blocks = 20;

    private readonly ImmutableArray<Ellipse> _debugPoints;
    private readonly Shape _head;
    private readonly Shape _apple;
    private readonly List<Polyline> _tailLines = [];

    private CancellationTokenSource _gameLoopTokenSource = new();
    private Moving _nextDirection;
    private HeadState _headState;
    private TailState _tailState;
    private AppleState _appleState;

    private readonly IDisposable _keyEventHandler;

    public GameCanvasView()
    {
        InitializeComponent();

        _debugPoints = CreateDebugPoints();
        GrassField.Children.AddRange(_debugPoints);

        _head = CreateHead();
        GrassField.Children.Add(_head);

        _apple = CreateApple();
        GrassField.Children.Add(_apple);

        DetachedFromLogicalTree += GameCanvas_Closing;

        GrassField.SizeChanged += GrassField_SizeChanged;

        RestartButton.Click += RestartButton_Click;

        _keyEventHandler = KeyDownEvent.AddClassHandler<TopLevel>(GameCanvas_KeyDown, handledEventsToo: true);

        ResetState();
    }

    private void RestartButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
       ResetState();
    }

    private void GameCanvas_Closing(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        _gameLoopTokenSource.Cancel();
        _keyEventHandler.Dispose();
    }

    private void GameCanvas_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Avalonia.Input.Key.Up when _headState.moving != Moving.Down:
                _nextDirection = Moving.Up;
                break;
            case Avalonia.Input.Key.Down when _headState.moving != Moving.Up:
                _nextDirection = Moving.Down;
                break;
            case Avalonia.Input.Key.Left when _headState.moving != Moving.Right:
                _nextDirection = Moving.Left;
                break;
            case Avalonia.Input.Key.Right when _headState.moving != Moving.Left:
                _nextDirection = Moving.Right;
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

        RedrawDebugGrid();
        RedrawTail();
        RedrawHead();
        RedrawApple();
    }

    private void ResetState()
    {
        _nextDirection = Moving.Up;

        _headState = new HeadState()
        {
            moving = Moving.Up,
            xIndex = 1,
            yIndex = Blocks - 1,
        };
       
        _tailState = new TailState()
        {
            points = new IndexedPoint[2],
            startIndex = 0,
        };

        _appleState = new AppleState();
        PlaceApple();

        UpdateScore();

        RedrawDebugGrid();
        RedrawTail();
        RedrawHead();
        RedrawApple();

        _gameLoopTokenSource.Cancel();
        _gameLoopTokenSource = new();

        _ = StartGameLoopAsync(_gameLoopTokenSource.Token);
    }

    public void RedrawDebugGrid() {
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
        var fieldScale = (int)(Blocks * Blocks);
        var bitmap = new BitArray(fieldScale);

        var availablePointsCount = fieldScale;
        foreach (var item in _tailState.points)
        {
            var valueIndex = (int)(item.xIndex * Blocks + item.yIndex);

            bitmap[valueIndex] = true;
            availablePointsCount--;
        }

        var index = Random.Shared.Next(availablePointsCount);

        var counter = 0;
        for (var i = 0; i < fieldScale; i++)
        {
            if (bitmap[i])
            {
                continue;
            }

            if(counter == index)
            {
                var pointX = i / Blocks;
                var pointY = i - pointX * Blocks;
                var state = new AppleState()
                {
                    xIndex = (uint)pointX,
                    yIndex = (uint)pointY,
                };

                _appleState = state;

                break;
            }

            counter++;
        }
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
            var nextHeadState = LoopSum(_headState, _nextDirection, Blocks);

            _headState = nextHeadState;

            var nextTailState = _tailState;
            var nextPointIndex = nextTailState.startIndex;

            static bool DetectCollision(ReadOnlySpan<IndexedPoint> points, IndexedPoint headPoint)
            {
                var collisionCount = 0;

                foreach (var item in points[1..])
                {
                    if (item.xIndex == headPoint.xIndex && item.yIndex == headPoint.yIndex && ++collisionCount > 1)
                    {
                        return true;
                    }
                }

                return false;
            }

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

            if (DetectCollision(nextTailState.points, new IndexedPoint { xIndex = nextHeadState.xIndex, yIndex = nextHeadState.yIndex }))
            {
                _gameLoopTokenSource.Cancel();
            }

            await Task.Delay(TickDelay, cancellationToken);
        }
    }

    private void UpdateScore()
    {
        if (DataContext is GameCanvasViewModel vm)
        {
            var snakeLength = _tailState.points.Length - 2;

            vm.SnakeLength = snakeLength;
        }
    }

    private static HeadState LoopSum(HeadState value, Moving moving, uint maxValue)
    {
        return value with
        {
            moving = moving,
            xIndex = (uint)((maxValue + value.xIndex + moving.X) % maxValue),
            yIndex = (uint)((maxValue + value.yIndex + moving.Y) % maxValue)
        };
    }

    private static uint LoopIncrease(uint value, uint maxValue)
    {
        return value == (maxValue - 1) ? 0 : value + 1;
    }
}
