using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices.JavaScript;

namespace hello_ws2812;


/// <summary>
/// Base class for LED image manipulation that handles the core data structure and operations
/// </summary>
public class LedImage
{
    public int Width { get; }
    public int Height { get; }
    public Color[,] Data { get; }

    public LedImage(int width, int height)
    {
        Width = width;
        Height = height;
        Data = new Color[Width, Height];
        Clear();
    }

    public void Clear(Color color = default)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Data[x, y] = color;
            }
        }
    }

    public void SetPixel(int x, int y, Color c)
    {
        if (x < 0 || x >= Width)
        {
            throw new ArgumentOutOfRangeException(nameof(x));
        }

        if (y < 0 || y >= Height)
        {
            throw new ArgumentOutOfRangeException(nameof(y));
        }

        Data[x, y] = c;
    }

    public Color GetPixel(int x, int y)
    {
        if (x < 0 || x >= Width)
        {
            throw new ArgumentOutOfRangeException(nameof(x));
        }

        if (y < 0 || y >= Height)
        {
            throw new ArgumentOutOfRangeException(nameof(y));
        }

        return Data[x, y];
    }
    
    /// <summary>
    /// Creates a deep copy of this LedImage
    /// </summary>
    /// <returns>A new LedImage with the same dimensions and pixel data</returns>
    public LedImage Clone()
    {
        LedImage clone = new LedImage(Width, Height);
        
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                clone.Data[x, y] = Data[x, y];
            }
        }
        
        return clone;
    }
}