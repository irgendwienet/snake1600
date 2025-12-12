using System.Drawing;

namespace HardwareLayer;

public static class ColorUtils
{
    public static Color ReduceBrightness(this Color color, double percent)
    {
        if (percent < 0 || percent > 100)
            throw new ArgumentOutOfRangeException(nameof(percent));

        // percent z.B. 20 => 20 % dunkler
        double factor = 1.0 - (percent / 100.0);

        double h = color.GetHue();            // 0..360
        double s = color.GetSaturation();     // 0..1
        double l = color.GetBrightness();     // 0..1

        l *= factor;
        l = Math.Clamp(l, 0, 1);

        return FromHsl(h, s, l, color.A);
    }
    
    private static Color FromHsl(double h, double s, double l, int alpha)
    {
        if (s == 0)
        {
            int gray = (int)(l * 255);
            return Color.FromArgb(alpha, gray, gray, gray);
        }

        double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        double p = 2 * l - q;

        double hk = h / 360.0;

        double[] t = { hk + 1.0 / 3.0, hk, hk - 1.0 / 3.0 };

        for (int i = 0; i < 3; i++)
        {
            if (t[i] < 0) t[i] += 1;
            if (t[i] > 1) t[i] -= 1;

            if (t[i] < 1.0 / 6.0) t[i] = p + (q - p) * 6 * t[i];
            else if (t[i] < 1.0 / 2.0) t[i] = q;
            else if (t[i] < 2.0 / 3.0) t[i] = p + (q - p) * (2.0 / 3.0 - t[i]) * 6;
            else t[i] = p;
        }

        return Color.FromArgb(
            alpha,
            (int)(t[0] * 255),
            (int)(t[1] * 255),
            (int)(t[2] * 255)
        );
    }    
}