using System.Collections.Generic;
using System.Drawing.Imaging;

namespace Transrender.Palettes
{
    public interface IPalette
    {
        ColorPalette Palette { get; }
        int GetRange(double index);
        double GetRangeMaximum(int rangeIndex);

        byte GetCombinedColour(List<byte> colours);
        bool IsSpecialColour(byte colour);
    }
}
