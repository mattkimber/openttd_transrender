using System.Collections.Generic;
using System.Drawing.Imaging;

namespace Transrender.Palettes
{
    public interface IPalette
    {
        ColorPalette Palette { get; }
        int GetRange(double index);
        double GetRangeMaximum(int rangeIndex);
        byte GetRangeMidpoint(double index);
        byte GetGreyscaleEquivalent(double index);

        ShaderResult GetCombinedColour(List<ShaderResult> colours);
        bool IsSpecialColour(byte colour);
        bool IsMaskColour(byte colour);
    }
}
