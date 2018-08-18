using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.VoxelUtils
{
    public class ProcessedVoxelObject
    {
        public ProcessedVoxelElement[][][] Voxels { get; private set; }

        public int Width { get; private set; }
        public int Depth { get; private set; }
        public int Height { get; private set; }

        public byte[][][] Data { get; private set; }

        

        public ProcessedVoxelObject(byte[][][] voxels)
        {
            Data = voxels;
            Voxels = new ProcessedVoxelElement[Data.Length][][];

            if (Data.Length == 0 || Data[0].Length == 0 || Data[0][0].Length == 0)
            {
                throw new ArgumentException("Voxel object dimensions must be at least 1x1x1");
            }

            Width = Data.Length;
            Depth = Data[0].Length;
            Height = Data[0][0].Length;

            for (var x = 0; x < Width; x++)
            {
                Voxels[x] = new ProcessedVoxelElement[Data[x].Length][];

                for (var y = 0; y < Depth; y++)
                {
                    Voxels[x][y] = new ProcessedVoxelElement[Data[x][y].Length];

                    for (var z = 0; z < Height; z++)
                    {
                        Voxels[x][y][z] = new ProcessedVoxelElement
                        {
                            Colour = GetThinnedColour(x, y, z),
                            Normal = GetNormal(x, y, z)
                        };
                    }
                }
            }

            // Apply thinning to the data
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Depth; y++)
                {
                    for (var z = 0; z < Height; z++)
                    {
                        Data[x][y][z] = Voxels[x][y][z].Colour;
                    }
                }
            }
        }

        private byte GetThinnedColour(int x, int y, int z)
        {
            if (
                    x > 0 && x < Width - 1 &&
                    y > 0 && y < Depth - 1 &&
                    z > 0 && z < Height - 1 &&
                    Data[x + 1][y][z] != 0 &&
                    Data[x - 1][y][z] != 0 &&
                    Data[x][y + 1][z] != 0 &&
                    Data[x][y - 1][z] != 0 &&
                    Data[x][y][z + 1] != 0 &&
                    Data[x][y][z - 1] != 0
                    )
            {
                return 0;
            }

            return Data[x][y][z];
        }

        private Vector GetNormal(int x, int y, int z)
        {
            return new Vector
            {
                X = x,
                Y = 0,
                Z = 0
            };
        }
    }
}
