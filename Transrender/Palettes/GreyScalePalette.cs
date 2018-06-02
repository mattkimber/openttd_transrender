using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.Palettes
{
    public class GreyScalePalette : IPalette
    {
        private ColorPalette _palette;

        public GreyScalePalette()
        {
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        private void SetPalette()
        {
            // Need to create a bitmap as palettes cannot be instantiated.
            var bitmap = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
            _palette = bitmap.Palette;

            for (var i = 0; i < 255; i++)
            {
                _palette.Entries[i] = Color.FromArgb(i, i, i);
            }
        }

        public ColorPalette Palette
        {
            get
            {
                if (_palette == null)
                {
                    SetPalette();
                }

                return _palette;
            }
        }

        public ShaderResult GetCombinedColour(List<ShaderResult> colours)
        {
            return new ShaderResult
            {
                PaletteColour = (byte)colours.Average(c => c.PaletteColour),
                R = (byte)colours.Average(c => c.R),
                G = (byte)colours.Average(c => c.G),
                B = (byte)colours.Average(c => c.B),
                M = (byte)colours.Average(c => c.M),
                Has32BitData = true
            };
        }

        public int GetRange(double index)
        {
            return 0;
        }

        public double GetRangeMaximum(int rangeIndex)
        {
            return 255;
        }

        public bool IsMaskColour(byte colour)
        {
            return false;
        }

        public bool IsSpecialColour(byte colour)
        {
            return false;
        }
    }
}
