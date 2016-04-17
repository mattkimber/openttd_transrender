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

            for (var i = x > 1 ? -1 : 0; i < (x >= (Width - 2) ? Width - x : 2); i++)
            {
                for (var j = y > 1 ? -1 : 0; j < (y >= (Depth - 2) ? Depth - y : 2); j++)
                {
                    for (var k = z > 1 ? -1 : 0; k < (z >= (Height - 2) ? Height - z : 2); k++)
                    {
                        if (_voxels[x + i][y + j][z + k] != 0)
                        {
                            occlusionCount--;
                        }
                    }
                }
            }

            return occlusionCount / 18.0;
        }

        private double GetShadowOffset(int x, int y, int z, int[][] shadowVector)
        {
            var offset = 0.0;

            offset -= shadowVector
                .Where(t => x + t[0] >= 0 && x + t[0] < Width && y + t[1] >= 0 && y + t[1] < Depth && z + t[2] >= 0 && z + t[2] < Height)
                .Count(t => _voxels[x + t[0]][y + t[1]][z + t[2]] != 0);

            return offset;
        }

        public byte GetRawPixel(int x, int y, int z)
        {
            return _voxels[x][y][z];
        }

        public byte ShadePixel(int x, int y, int z, int screenX, int screenY, int[][] shadowVector)
        {
            var originalColor = (double)_voxels[x][y][z];

            if(_palette.IsSpecialColour((byte)originalColor))
            {
                return (byte)originalColor;
            }

            var finalColor = originalColor;

            finalColor += 2.5;
            finalColor += GetAmbientOcclusionOffset(x, y, z);
            finalColor += GetShadowOffset(x, y, z, shadowVector);
            return GetDitheredColour(originalColor, finalColor, true);
        }

        public bool IsTransparent(int x, int y, int z)
        {
            return _voxels[x][y][z] == 0;
        }

        /*
         * This stuff is needed for packing multiple objects together, but not right now
        public VoxelObject Composite(VoxelObject input, int xOffset, int yOffset, int zOffset, string pngFile)
        {
            if (xOffset >= Width || yOffset >= Height || zOffset >= Depth)
            {
                return this;
            }

            var voxelObject = new VoxelObject { Width = Width, Height = Height, Depth = Depth, PngFile = pngFile };

            var maxX = (xOffset + input.Width) > Width ? Width : input.Width + xOffset;
            var maxY = (yOffset + input.Depth) > Depth ? Depth : input.Depth + yOffset;
            var maxZ = (zOffset + input.Height) > Height ? Height : input.Height + zOffset;

            voxelObject.Pixels = new byte[Width][][];

            for (int x = 0; x < Width; x++)
            {
                voxelObject.Pixels[x] = new byte[Depth][];

                for (int y = 0; y < Depth; y++)
                {
                    voxelObject.Pixels[x][y] = new byte[Height];

                    for (int z = 0; z < Height; z++)
                    {
                        if (Pixels[x][y][z] == 0 &&
                           x >= xOffset && y >= yOffset && z >= zOffset
                           && x < maxX && y < maxY && z < maxZ)
                        {
                            voxelObject.Pixels[x][y][z] = input.Pixels[x - xOffset][y - yOffset][z - zOffset];
                        }
                        else
                        {
                            voxelObject.Pixels[x][y][z] = Pixels[x][y][z];
                        }
                    }
                }
            }

            return voxelObject;
        }

        public VoxelObject Scale(double factor)
        {
            return Scale(factor, factor, factor);
        }

        public VoxelObject Pad(int height, int depth)
        {
            if (this.Depth > depth || this.Height > height)
            {
                throw new ApplicationException("Input voxel object is larger than the maximum allowed.");
            }

            var paddedVoxels = new VoxelObject { Width = Width, Height = height, Depth = depth, PngFile = PngFile };

            paddedVoxels.Pixels = new byte[paddedVoxels.Width][][];

            var depthMargin = (depth - Depth) / 2;
            var heightMargin = (height - Height);

            for (int x = 0; x < Width; x++)
            {
                paddedVoxels.Pixels[x] = new byte[paddedVoxels.Depth][];

                for (int y = 0; y < depth; y++)
                {
                    paddedVoxels.Pixels[x][y] = new byte[paddedVoxels.Height];

                    for (int z = 0; z < height; z++)
                    {
                        if (y >= depthMargin && y < Depth + depthMargin && z >= heightMargin && z < Height + heightMargin)
                        {
                            paddedVoxels.Pixels[x][y][z] = Pixels[x][y - depthMargin][z - heightMargin];
                        }
                    }
                }
            }

            return paddedVoxels;
        }

        public VoxelObject Scale(double xFactor, double yFactor, double zFactor)
        {
            var scaledVoxels = new VoxelObject
            {
                Width = (int)(Width * xFactor),
                Height = (int)(Height * zFactor),
                Depth = (int)(Depth * yFactor),
                PngFile = PngFile
            };

            scaledVoxels.Pixels = new byte[scaledVoxels.Width][][];

            for (int x = 0; x < (int)(Width * xFactor); x++)
            {
                scaledVoxels.Pixels[x] = new byte[scaledVoxels.Depth][];

                for (int y = 0; y < (int)(Depth * yFactor); y++)
                {
                    scaledVoxels.Pixels[x][y] = new byte[scaledVoxels.Height];

                    for (int z = 0; z < (int)(Height * zFactor); z++)
                    {
                        scaledVoxels.Pixels[x][y][z] = Pixels[(int)((x + 0.5) / xFactor)][(int)((y + 0.5) / yFactor)][(int)((z + 0.5) / zFactor)];
                    }
                }
            }

            return scaledVoxels;
        }

    */
    }
}
