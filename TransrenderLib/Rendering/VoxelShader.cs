using System;
using System.Numerics;
using Transrender.Palettes;
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

        public ShaderResult ShadePixel(int x, int y, int z, int projection, Vector3 lightingVector)
        {

            if(_shaderCache[projection][x][y][z] != null)
            {
                return _shaderCache[projection][x][y][z];
            }

            var originalColor = GetRawPixel(x,y,z);
            
            byte r, g, b, m;

            if (_palette.IsMaskColour(originalColor))
            {
                var midpoint = _palette.GetRangeMidpoint(originalColor);
                var diff = midpoint - originalColor;

                r = _palette.GetGreyscaleEquivalent(originalColor);
                g = _palette.GetGreyscaleEquivalent(originalColor);
                b = _palette.GetGreyscaleEquivalent(originalColor);
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
            
            var offset = GetLighting(x,y,z,lightingVector) / 1.5;
            offset = (offset + 1.0);

            if(_voxels.Voxels[x][y][z].IsShadowed)
            {
                offset = offset * 0.75;
            }

            //offset = (offset - 1.0);

            var finalColor = (double)originalColor + ((offset - 1.0) * 4.0);
            
            var ditheredTtdColour = GetDitheredColour(originalColor, finalColor, true);

            var result = new ShaderResult
            {
                PaletteColour = ditheredTtdColour,
                R = GetClampedColour(r, offset),
                G = GetClampedColour(g, offset),
                B = GetClampedColour(b, offset),
                M = m,
                Has32BitData = true
            };

            _shaderCache[projection][x][y][z] = result;
            return result;

        }

        private byte GetClampedColour(byte value, double multiplier)
        {
            var result = value * multiplier;
            if(result > 255) return 255;
            if (result < 0) return 0;
            return (byte)result;
        }

        private double GetLighting(int x, int y, int z, Vector3 lightingVector)
        {
            var normal = _voxels.Voxels[x][y][z].AveragedNormal;
            var dotProduct = (normal.X * (lightingVector.X)) 
                + (normal.Y * (lightingVector.Y)) 
                + (normal.Z * (lightingVector.Z));

            var magnitude = normal.Length() * lightingVector.Length();
            return dotProduct / magnitude;
        }

        public bool IsTransparent(int x, int y, int z)
        {
            return _voxels.Data[x][y][z] == 0;
        }
    }
}
