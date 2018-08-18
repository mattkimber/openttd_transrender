using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.Util
{
    public static class ColourUtil
    {
        public static double GetCorrectBrightness(int r, int g, int b)
        {
            return (r * 0.299 + g * 0.587 + b * 0.114) / 256;
        }

        public static Color FromHSL(float hue, float saturation, float lightness)
        {
            if(lightness > 1.0)
            {
                lightness = 1.0f;
            }

            if (lightness < 0 || float.IsNaN(lightness))
            {
                lightness = 0.0f;
            }

            if (saturation == 0)
            {
                return Color.FromArgb((int)(lightness*255), (int)(lightness*255), (int)(lightness*255));
            }

            double min, max, h;
            h = hue / 360d;

            max = lightness < 0.5d ? lightness * (1 + saturation) : (lightness + saturation) - (lightness * saturation);
            min = (lightness * 2d) - max;

            Color c = Color.FromArgb(255, (int)(255 * RGBChannelFromHue(min, max, h + 1 / 3d)),
                                          (int)(255 * RGBChannelFromHue(min, max, h)),
                                          (int)(255 * RGBChannelFromHue(min, max, h - 1 / 3d)));
            return c;
        }

        private static double RGBChannelFromHue(double m1, double m2, double h)
        {
            h = (h + 1d) % 1d;

            if (h < 0)
            {
                h += 1;
            }

            if (h * 6 < 1)
            {
                return m1 + (m2 - m1) * 6 * h;
            }
            else if (h * 2 < 1)
            {
                return m2;
            }
            else if (h * 3 < 2)
            {
                return m1 + (m2 - m1) * 6 * (2d / 3d - h);
            }

            return m1;
        }
    }
}
