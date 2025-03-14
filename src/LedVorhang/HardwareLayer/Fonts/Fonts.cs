namespace HardwareLayer.Fonts;

public enum Font
{
    Font_4x6,
    Font_5x8,
    Font_6x8,
    Font_5x12,
    Font_7x9,
}

public partial class Fonts
{
    public static (byte[], int) Get(Font font, char ch)
    { if(ch < 32 || ch > 127)
        {
            throw new ArgumentOutOfRangeException("ch");
        }

        byte[] bytes = GetFont(font);
        (int width, int height) = GetSize(font);

        int startIndex = (ch - 32) * height;
        byte[] result = new byte[height];

        Array.Copy(bytes, startIndex, result, 0, height);
        
        return (result, width);
    }

    public static (int, int) GetSize(Font font)
    {
        return font switch
        {
            Font.Font_4x6 => (4,6),
            Font.Font_5x8 => (5,8),
            Font.Font_6x8 => (6,8),
            Font.Font_5x12 => (5,12),
            Font.Font_7x9 => (7,9),
            _ => throw new ArgumentOutOfRangeException(nameof(font), font, null)
        }; ;
    }

    private static byte[] GetFont(Font font)
    {
        return font switch
        {
            Font.Font_4x6 => Font_4x6,
            Font.Font_5x8 => Font_5x8,
            Font.Font_6x8 => Font_6x8,
            Font.Font_5x12 => Font_5x12,
            Font.Font_7x9 => Font_7x9,
            _ => throw new ArgumentOutOfRangeException(nameof(font), font, null)
        }; ;
    }
}