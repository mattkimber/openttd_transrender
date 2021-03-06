﻿using System;
using System.Numerics;

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
                            Colour = GetThinnedColour(x, y, z)
                        };

                        Voxels[x][y][z].IsSurface = Voxels[x][y][z].Colour != 0 && Voxels[x][y][z].Colour == Data[x][y][z];
                        Voxels[x][y][z].Normal = CalculateNormal(x, y, z);

                        if(Voxels[x][y][z].IsSurface)
                        {
                            Voxels[x][y][z].IsShadowed = GetIsShadowed(x, y, z);
                        }
                    }
                }
            }

            // Apply thinning and averaging to the data
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Depth; y++)
                {
                    for (var z = 0; z < Height; z++)
                    {
                        Data[x][y][z] = Voxels[x][y][z].Colour;
                        Voxels[x][y][z].AveragedNormal = GetAveragedNormal(x, y, z);
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
                    Data[x][y][z - 1] != 0 &&
                    Data[x-1][y][z - 1] != 0 &&
                    Data[x][y][z - 1] != 0 &&
                    Data[x+1][y][z - 1] != 0
                    )
            {
                return 0;
            }

            return Data[x][y][z];
        }

        private byte SafeGetData(int x, int y, int z)
        {
            if(x < 0 || y < 0 || z < 0 || x >= Width || y >= Depth || z >= Height)
            {
                return 0;
            }

            return Data[x][y][z];
        }

        private Vector3? SafeGetNormal(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= Width || y >= Depth || z >= Height)
            {
                return null;
            }

            return Voxels[x][y][z].Normal;
        }

        private Vector3 CalculateNormal(int x, int y, int z)
        {
            if(!Voxels[x][y][z].IsSurface)
            {
                return new Vector3();
            }

            var xVector = 0;
            var yVector = 0;
            var zVector = 0;
            var distance = 3;

            // Sum the directions of "open" voxels
            for(var i = -distance; i <= distance; i++)
            {
                for (var j = -distance; j <= distance; j++)
                {
                    for(var k = -distance; k <= distance; k++)
                    {
                        if((i*i)+(j*j)+(k*k) <= (distance*distance) && SafeGetData(x+i,y+j,z-k) == 0)
                        {
                            xVector -= i;
                            yVector -= j;
                            zVector -= k;
                        }
                    }
                }
            }

            return Vector3.Normalize(new Vector3(xVector, yVector, zVector));
        }

        private Vector3 GetAveragedNormal(int x, int y, int z)
        {
            var result = new Vector3();
            var distance = 2;

            for (var i = -distance; i <= distance; i++)
            {
                for (var j = -distance; j <= distance; j++)
                {
                    for (var k = -distance; k <= distance; k++)
                    {
                        var normal = SafeGetNormal(x + i, y + j, z + k);
                        if(normal != null)
                        {
                            result += normal.Value;
                        }
                    }
                }
            }
            
            if(result.Length() < 0.5)
            {
                return Voxels[x][y][z].Normal;
            }

            return Vector3.Normalize(result);
        }

        private bool GetIsShadowed(int x, int y, int z)
        {
            if(SafeGetData(x,y,z+1) != 0)
            {
                return false;
            }

            for(var i = z+1; i <= Height; i++)
            {
                if(SafeGetData(x,y,i) != 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}