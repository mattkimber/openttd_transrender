using System;
using System.Linq;
using Transrender.Palettes;
using Transrender.Projector;
using Transrender.VoxelUtils;

namespace Transrender.Rendering
{
    public class VoxelShader
    {
        private IPalette _palette;
        private ProcessedVoxelObject _voxels;
        private double?[][][] _occlusionCache;
        private ShaderResult[][][][] _shaderCache;
        
        public int Width { get { return _voxels.Width; } }
        public int Height { get { return _voxels.Height; } }
        public int Depth { get { return _voxels.Depth; } }

        public VoxelShader(IPalette palette, ProcessedVoxelObject voxels)
        {
            _palette = palette;
            _voxels = voxels;

            BuildOcclusionCache();
            BuildShaderCache();
        }

        private void BuildOcclusionCache()
        {
            _occlusionCache = new double?[Width][][];

            for (var i = 0; i < Width; i++)
            {
                _occlusionCache[i] = new double?[Depth][];
                for (var j = 0; j < Depth; j++)
                {
                    _occlusionCache[i][j] = new double?[Height];
                }
            }
        }


        private void BuildShaderCache()
        {
            _shaderCache = new ShaderResult[8][][][];

            for (var i = 0; i < 8; i++)
            {
                _shaderCache[i] = new ShaderResult[Width][][];
                for (var j = 0; j < Width; j++)
                {
                    _shaderCache[i][j] = new ShaderResult[Depth][];
                    for (var k = 0; k < Depth; k++)
                    {
                        _shaderCache[i][j][k] = new ShaderResult[Height];
                    }
                }
            }
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
            if (_occlusionCache[x][y][z] == null)
            {

                var occlusionCount = 0;

                for (var i = x > 1 ? -1 : 0; i < (x >= (Width - 2) ? Width - x : 2); i += 2)
                {
                    for (var j = y > 1 ? -1 : 0; j < (y >= (Depth - 2) ? Depth - y : 2); j += 2)
                    {
                        for (var k = z > 1 ? -1 : 0; k < (z >= (Height - 2) ? Height - z : 2); k += 2)
                        {
                            if (GetRawPixel(x + i, y + j, z + k) != 0)
                            {
                                occlusionCount--;
                            }
                        }
                    }
                }

                _occlusionCache[x][y][z] = occlusionCount / 4.0;
            }

            return (double)_occlusionCache[x][y][z];
        }

        private double GetShadowOffset(int x, int y, int z, ShadowVector shadowVector)
        {
            var offset = 0.0;

            offset -= shadowVector.Vectors
                .Where(t => x + t[0] >= 0 && x + t[0] < Width && y + t[1] >= 0 && y + t[1] < Depth && z + t[2] >= 0 && z + t[2] < Height)
                .Count(t => GetRawPixel(x + t[0],y + t[1],z + t[2]) != 0);

            return offset;
        }

        public byte GetRawPixel(int x, int y, int z)
        {
            try
            {
                return _voxels.Data[x][y][z];
            }
            catch
            {
                return 0;
            }
        }

        public ShaderResult ShadePixel(int x, int y, int z, ShadowVector shadowVector)
        {

            if(_shaderCache[shadowVector.Id][x][y][z] != null)
            {
                return _shaderCache[shadowVector.Id][x][y][z];
            }

            //var originalColor = GetRawPixel(x,y, z);
            var originalColor = (byte)_voxels.Voxels[x][y][z].Normal.X;

            return new ShaderResult
            {
                PaletteColour = originalColor,
                R = originalColor,
                G = originalColor,
                B = originalColor,
                M = 0,
                Has32BitData = true
            };

            byte r, g, b, m;

            if (_palette.IsMaskColour(originalColor))
            {
                var midpoint = _palette.GetRangeMidpoint(originalColor);
                var diff = midpoint - originalColor;

                r = _palette.GetGreyscaleEquivalent(originalColor);
                g = _palette.GetGreyscaleEquivalent(originalColor);
                b = _palette.GetGreyscaleEquivalent(originalColor);
                //m = _palette.IsMaskColour(originalColor) ? _palette.GetRangeMidpoint(originalColor) : (byte)0;
                m = (byte)(originalColor + (diff / 2));

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
            var offset = 0.0;
            //var offset = 2.5 + GetAmbientOcclusionOffset(x, y, z);

            //var offset = GetShadowOffset(x, y, z, shadowVector);
            //finalColor += offset;
            
            var ditheredTtdColour = GetDitheredColour(originalColor, finalColor, true);

            var result = new ShaderResult
            {
                PaletteColour = ditheredTtdColour,
                R = GetSafeOffsetColour(r, offset * 25),
                G = GetSafeOffsetColour(g, offset * 25),
                B = GetSafeOffsetColour(b, offset * 25),
                M = m, Has32BitData = true
            };

            _shaderCache[shadowVector.Id][x][y][z] = result;
            return result;

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
