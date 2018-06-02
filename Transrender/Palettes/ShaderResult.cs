using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.Palettes
{
    public class ShaderResult
    {
        public byte PaletteColour { get; set; }
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte M { get; set; }

        public bool Has32BitData { get; set; }

        public static ShaderResult White()
        {
            return new ShaderResult { PaletteColour = 255, A = 0, R = 255, G = 255, B = 255, M = 0, Has32BitData = true };
        }

        public static ShaderResult Transparent()
        {
            return new ShaderResult { PaletteColour = 0, A = 255, R = 0, G = 0, B = 0, M = 0, Has32BitData = true };
        }
    }
}
