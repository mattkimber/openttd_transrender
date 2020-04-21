﻿namespace Transrender.Util
{
    public static class ColourUtil
    {
        public static double GetCorrectBrightness(int r, int g, int b)
        {
            return (r * 0.299 + g * 0.587 + b * 0.114) / 256;
        }
    }
}