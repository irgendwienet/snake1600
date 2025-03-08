using System.Drawing;

namespace hello_ws2812;

internal class BitmapImageNeo3RgbVorhang : BitmapImageNeo3Vorhang
{
    public BitmapImageNeo3RgbVorhang(int width, int height)
        : base(width, height)
    {
    }

    public override void SetPixel(int x, int y, Color c)
    {
        var offset = y * Stride + x * BytesPerPixel;
        Data[offset++] = _lookup[c.R * BytesPerComponent + 0];
        Data[offset++] = _lookup[c.R * BytesPerComponent + 1];
        Data[offset++] = _lookup[c.R * BytesPerComponent + 2];
        Data[offset++] = _lookup[c.G * BytesPerComponent + 0];
        Data[offset++] = _lookup[c.G * BytesPerComponent + 1];
        Data[offset++] = _lookup[c.G * BytesPerComponent + 2];
        Data[offset++] = _lookup[c.B * BytesPerComponent + 0];
        Data[offset++] = _lookup[c.B * BytesPerComponent + 1];
        Data[offset++] = _lookup[c.B * BytesPerComponent + 2];
    }
}