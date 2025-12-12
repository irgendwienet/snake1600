using System.Drawing;

namespace HardwareLayer;

public class MatrixScreensaver : IScreensaver
{
    private readonly int _width;
    private readonly int _height;
    private readonly int _trailLength;
    
    private readonly Color _headColor;
    private readonly Color _trailColor;
    
    private readonly Random _random = new();
    
    private readonly List<Point> _heads = new();
    
    public MatrixScreensaver(
        int width,
        int height,
        Color? spawnColor = null,
        Color? trailColor = null,
        int trailLength = 6)
    {
        _width = width;
        _height = height;
        _trailLength = trailLength;

        _headColor = spawnColor ?? Color.FromArgb(175, 255, 175);
        _trailColor = trailColor ?? Color.FromArgb(27, 130, 39);
    }

    private bool b;
    
    public void Update()
    {
        b = !b;
        
        //Thread.Sleep(120);
        if (b)
        {
            RemoveOldHeads();
            SpanHead();
        }
        else
        {
            MoveHeads();
        }
    }

    public void Draw(LedImage image)
    {
        foreach (var head in _heads)
        {
            Draw(image, head);
        }
    }

    private void MoveHeads()
    {
        for (int i = 0; i < _heads.Count; i++)
        {
            var p = _heads[i];
            _heads[i] = p with { Y = p.Y + 1 };
        }
    }

    private void Draw(LedImage image, Point head)
    {
        SetPixelSave(image, head.X, head.Y, _headColor);

        var c = _trailColor;
        for (int i = 1; i < _trailLength; i++)
        {
            SetPixelSave(image, head.X, head.Y-i, c);
            
            c = c.ReduceBrightness(40);
        }
    }

    private void SetPixelSave(LedImage image, int x, int y, Color color)
    {
        if (y < _height-1 && y > 0)
        {
            image.SetPixel(y, x, color);
        }
    }


    private void SpanHead()
    {
        int itemsInToRows = _heads.Count(p => p.Y < _trailLength/2);
        
        if (itemsInToRows >= _width / 8)
            return;

        int x = _random.Next(1, _width - 1);
        _heads.Add(new Point(x, 2));
    }

    private void RemoveOldHeads()
    {
        var border = _height + _trailLength + 2;
        _heads.RemoveAll(p => p.Y > border);
    }
    

}