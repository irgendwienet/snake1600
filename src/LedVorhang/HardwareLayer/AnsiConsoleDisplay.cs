using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace HardwareLayer;

/// <summary>
/// Displays a LedImage on the console using ANSI/VT escape sequences (supports truecolor).
/// Mirrors ConsoleDisplay behavior but emits VT sequences instead of using ConsoleColor.
/// </summary>
public class AnsiConsoleDisplay : IDisplay
{
    private LedImage? _lastImage;
    private readonly bool _useColors;
    private readonly char _blockChar = '█';
    private const bool _isMirrored = false;

    public AnsiConsoleDisplay(bool useColors = true)
    {
        _useColors = useColors;
        EnableVirtualTerminalProcessing();
        // Hide cursor and clear screen
        Console.Write("\x1b[?25l\x1b[2J\x1b[H");
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
                if (_lastImage!.GetPixel(x, y, _isMirrored) != image.GetPixel(x, y, _isMirrored))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void DrawImage(LedImage image)
    {
        var sw = Stopwatch.StartNew();

        // Move cursor to home (1,1)
        var frame = new StringBuilder(image.Height * (image.Width + 16));
        frame.Append("\x1b[H");

        for (int y = 0; y < image.Height; y++)
        {
            if (!_useColors)
            {
                // No color path: build row and append newline
                for (int x = 0; x < image.Width; x++)
                {
                    var color = image.GetPixel(y, x, _isMirrored); // keep x,y order
                    frame.Append((color.R | color.G | color.B) != 0 ? _blockChar : ' ');
                }
                frame.Append('\n');
            }
            else
            {
                // Color path: use 24-bit ANSI color; minimize color changes
                int? curR = null, curG = null, curB = null; // null means no color (spaces)
                bool inColor = false;

                for (int x = 0; x < image.Width; x++)
                {
                    var c = image.GetPixel(y, x, _isMirrored);
                    bool isBlack = (c.R | c.G | c.B) == 0;

                    if (isBlack)
                    {
                        if (inColor)
                        {
                            // switch to default color for spaces or simply stop coloring
                            frame.Append("\x1b[39m");
                            inColor = false;
                            curR = curG = curB = null;
                        }
                        frame.Append(' ');
                    }
                    else
                    {
                        if (!inColor || c.R != curR || c.G != curG || c.B != curB)
                        {
                            frame.Append("\x1b[38;2;");
                            frame.Append(c.R);
                            frame.Append(';');
                            frame.Append(c.G);
                            frame.Append(';');
                            frame.Append(c.B);
                            frame.Append('m');
                            curR = c.R; curG = c.G; curB = c.B;
                            inColor = true;
                        }
                        frame.Append(_blockChar);
                    }
                }

                if (inColor)
                {
                    frame.Append("\x1b[39m"); // reset foreground color for next line
                }

                frame.Append('\n');
            }
        }

        // Reset attributes but keep cursor hidden
        frame.Append("\x1b[0m");

        Console.Write(frame.ToString());

        Debug.WriteLine($"Ansi DrawImage took {sw.ElapsedMilliseconds} ms");
    }

    // Enable ANSI/VT on Windows console
    private static void EnableVirtualTerminalProcessing()
    {
        try
        {
            Console.OutputEncoding = new UTF8Encoding(false);
            var hOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (GetConsoleMode(hOut, out uint mode))
            {
                mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING; // | DISABLE_NEWLINE_AUTO_RETURN;
                SetConsoleMode(hOut, mode);
            }
        }
        catch
        {
            // Ignore; if VT can't be enabled, output will still be readable though colors may show codes
        }
    }

    private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    // private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
}
