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

        private Vector SafeGetNormal(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= Width || y >= Depth || z >= Height)
            {
                return null;
            }

            return Voxels[x][y][z].Normal;
        }

        private Vector CalculateNormal(int x, int y, int z)
        {
            if(!Voxels[x][y][z].IsSurface)
            {
                return new Vector();
            }

            var xVector = 0;
            var yVector = 0;
            var zVector = 0;
            var distance = 3;

            // Sum the directions of "open" voxels
            // TODO: this should be a sphere not a cube
            for(var i = -distance; i <= distance; i++)
            {
                for (var j = -distance; j <= distance; j++)
                {
                    for(var k = -distance; k <= distance; k++)
                    {
                        if((i*i)+(j*j)+(k*k) <= (distance*distance) && SafeGetData(x+i,y+j,z+k) == 0)
                        {
                            xVector -= i;
                            yVector -= j;
                            zVector -= k;
                        }
                    }
                }
            }

            var magnitude = Math.Sqrt((xVector * xVector) + (yVector * yVector) + (zVector * zVector));

            return new Vector
            {
                X = xVector / magnitude,
                Y = yVector / magnitude,
                Z = zVector / magnitude
            };
        }

        private Vector GetAveragedNormal(int x, int y, int z)
        {
            var result = new Vector();
            var distance = 1;

            for (var i = -distance; i <= distance; i++)
            {
                for (var j = -distance; j <= distance; j++)
                {
                    for (var k = -distance; k <= distance; k++)
                    {
                        var normal = SafeGetNormal(x + i, y + j, z + k);
                        if(normal != null)
                        {
                            result.X += normal.X;
                            result.Y += normal.Y;
                            result.Z += normal.Z;
                        }
                    }
                }
            }


            var magnitude = Math.Sqrt((result.X * result.X) + (result.Y * result.Y) + (result.Z * result.Z));

            return new Vector
            {
                X = result.X / magnitude,
                Y = result.Y / magnitude,
                Z = result.Z / magnitude
            };
        }

        private bool GetIsShadowed(int x, int y, int z)
        {
            if(SafeGetData(x,y,z-1) != 0)
            {
                return false;
            }

            for(var i = 0; i < z; i++)
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