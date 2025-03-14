using System.Drawing;

namespace HardwareLayer;

/// <summary>
/// Displays a LedImage on the console using colored blocks
/// </summary>
public class ConsoleDisplay : IDisplay
{
    private LedImage? _lastImage;
    private readonly bool _useColors;
    private readonly char _blockChar = '█'; // Unicode block character
        
    public ConsoleDisplay(bool useColors = true)
    {
        _useColors = useColors;
        Console.CursorVisible = false;
        Clear();
    }

    public void Update(LedImage image)
    {
        if (_lastImage == null || HasChanges(image))
        {
            DrawImage(image);
            _lastImage = image.Clone();
        }
    }

    public void ForceUpdate(LedImage image)
    {
        DrawImage(image);
        _lastImage = image.Clone();
    }

    private bool HasChanges(LedImage image)
    {
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                if (_lastImage!.GetPixel(x, y) != image.GetPixel(x, y))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void DrawImage(LedImage image)
    {
        Console.SetCursorPosition(0, 0);
            
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var color = image.GetPixel(y, x);
                    
                if (_useColors)
                {
                    // Set console color based on the pixel color
                    Console.ForegroundColor = GetClosestConsoleColor(color);
                }
                    
                // Only print a block if color is not black (transparent)
                if (color.R > 0 || color.G > 0 || color.B > 0)
                {
                    Console.Write(_blockChar);
                }
                else
                {
                    Console.Write(' ');
                }
            }
            Console.WriteLine();
        }
            
        // Reset color
        Console.ResetColor();
    }

    private ConsoleColor GetClosestConsoleColor(Color color)
    {
        if (color.R == 0 && color.G == 0 && color.B == 0) return ConsoleColor.Black;
        if (color.R == 255 && color.G == 255 && color.B == 255) return ConsoleColor.White;
            
        // Simple algorithm to find the closest console color
        if (color.R > 200 && color.G < 100 && color.B < 100) return ConsoleColor.Red;
        if (color.R < 100 && color.G > 200 && color.B < 100) return ConsoleColor.Green;
        if (color.R < 100 && color.G < 100 && color.B > 200) return ConsoleColor.Blue;
        if (color.R > 200 && color.G > 200 && color.B < 100) return ConsoleColor.Yellow;
        if (color.R > 200 && color.G < 100 && color.B > 200) return ConsoleColor.Magenta;
        if (color.R < 100 && color.G > 200 && color.B > 200) return ConsoleColor.Cyan;
            
        // Fallback
        int brightness = (color.R + color.G + color.B) / 3;
        return brightness < 128 ? ConsoleColor.DarkGray : ConsoleColor.Gray;
    }

    private void Clear()
    {
        Console.Clear();
    }
}

public class ConsoleKeyboardEventArgs : EventArgs
    {
        public ConsoleKey KeyCode { get; }

        public ConsoleKeyboardEventArgs(ConsoleKey keyCode)
        {
            KeyCode = keyCode;
        }
    }
