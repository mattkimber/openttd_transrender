using Colorspace;
using System;
using System.Drawing;

namespace Transrender.Util
{
    public static class ColourUtil
    {
        public static double GetCorrectBrightness(int r, int g, int b)
        {
            return (r * 0.299 + g * 0.587 + b * 0.114) / 256;
        }

        public static Color Light(int r, int g, int b, double amount)
        {
            var rgb1 = new ColorRGB(r / 255.0, g / 255.0, b / 255.0);
            var hsl = new ColorHSL(rgb1);

            var newValue = Math.Max(Math.Min(hsl.L + (0.5 * amount), 1.0), 0.0);
            var lit = new ColorHSL(hsl.H, hsl.S, newValue);
            var rgb = new ColorRGB(lit);
            return Color.FromArgb((int)(rgb.R * 255), (int)(rgb.G * 255), (int)(rgb.B * 255));
        }
    }
}
