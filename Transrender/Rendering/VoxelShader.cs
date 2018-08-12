using System;
using System.Linq;
using Transrender.Palettes;

namespace Transrender.Rendering
{
    public class VoxelShader
    {
        private IPalette _palette;
        private byte[][][] _voxels;

        public int Width { get; set; }    

        public int Height { get; set; }
        
        public int Depth { get; set; }
        
        public VoxelShader(IPalette palette, byte[][][] voxelData)
        {
            if(voxelData.Length == 0 || voxelData[0].Length == 0 || voxelData[0][0].Length == 0)
            {
                throw new ArgumentException("Voxel object dimensions must be at least 1x1x1");
            }

            Width = voxelData.Length;
            Depth = voxelData[0].Length;
            Height = voxelData[0][0].Length;

            _palette = palette;
            _voxels = voxelData;
        }

        private byte GetDitheredColour(double originalColor, double finalColor, bool dither)
        {
            var error = (Math.Round(finalColor) - finalColor);

            // Simplistic dithering works better at this resolution than anything complex!
            if (error > 0.25 && dither)
            {
                finalColor -= 0.5;
            }
            else if (error < -0.25 && dither)
            {
                finalColor += 0.5;
            }

            var originalRange = _palette.GetRange(originalColor);
            var finalRange = _palette.GetRange(Math.Round(finalColor));

            var maxColor = _palette.GetRangeMaximum(originalRange);
            var minColor = _palette.GetRangeMaximum(originalRange - 1) + 1;

            if (originalRange < finalRange)
            {
                finalColor = maxColor;
            }
            else if (originalRange > finalRange)
            {
                finalColor = minColor;
            }

            return (byte)Math.Round(finalColor);
        }

        private double GetAmbientOcclusionOffset(int x, int y, int z)
        {
            var occlusionCount = 0;

            for (var i = x > 1 ? -1 : 0; i < (x >= (Width - 2) ? Width - x : 2); i+=2)
            {
                for (var j = y > 1 ? -1 : 0; j < (y >= (Depth - 2) ? Depth - y : 2); j+=2)
                {
                    for (var k = z > 1 ? -1 : 0; k < (z >= (Height - 2) ? Height - z : 2); k+=2)
                    {
                        if (GetRawPixel(x + i,y + j, z + k) != 0)
                        {
                            occlusionCount--;
                        }
                    }
                }
            }

            return occlusionCount / 4.0;
        }

        private double GetShadowOffset(int x, int y, int z, int[][] shadowVector)
        {
            var offset = 0.0;

            offset -= shadowVector
                .Where(t => x + t[0] >= 0 && x + t[0] < Width && y + t[1] >= 0 && y + t[1] < Depth && z + t[2] >= 0 && z + t[2] < Height)
                .Count(t => GetRawPixel(x + t[0],y + t[1],z + t[2]) != 0);

            return offset;
        }

        public byte GetRawPixel(int x, int y, int z)
        {
            if(x >= Width || y >= Depth || z >= Height || x < 0 || y < 0 || z < 0)
            {
                return 0;
            }

            return _voxels[x][y][z];
        }

        public ShaderResult ShadePixel(int x, int y, int z, int[][] shadowVector)
        {
            var originalColor = GetRawPixel(x,y, z);

            byte r, g, b, m;

            if (_palette.IsMaskColour(originalColor))
            {
                r = _palette.GetGreyscaleEquivalent(originalColor);
                g = _palette.GetGreyscaleEquivalent(originalColor);
                b = _palette.GetGreyscaleEquivalent(originalColor);
                m = _palette.IsMaskColour(originalColor) ? _palette.GetRangeMidpoint(originalColor) : (byte)0;
            }
            else
            {
                r = _palette.Palette.Entries[originalColor].R;
                g = _palette.Palette.Entries[originalColor].G;
                b = _palette.Palette.Entries[originalColor].B;
                m = 0;
            }

            if (_palette.IsSpecialColour(originalColor))
            {
                return new ShaderResult
                {
                    PaletteColour = originalColor,
                    R = r, G = g, B = b, A = 0, M = m, Has32BitData = true
                };
            }

            var finalColor = (double)originalColor;

            var offset = 2.5 + GetAmbientOcclusionOffset(x, y, z) + GetShadowOffset(x, y, z, shadowVector);
            finalColor += offset;

            var ditheredTtdColour = GetDitheredColour(originalColor, finalColor, true);

            return new ShaderResult
            {
                PaletteColour = ditheredTtdColour,
                R = GetSafeOffsetColour(r, offset * 25),
                G = GetSafeOffsetColour(g, offset * 25),
                B = GetSafeOffsetColour(b, offset * 25),
                M = m, Has32BitData = true
            };

        }

        private byte GetSafeOffsetColour(byte value, double offset)
        {
            if((double)value + offset > 255)
            {
                return 255;
            }

            if((double)value + offset < 0)
            {
                return 0;
            }

            return (byte)(value + offset);
        }

        public bool IsTransparent(int x, int y, int z)
        {
            return GetRawPixel(x,y, z) == 0;
        }
    }
}
