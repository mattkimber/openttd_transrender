using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.VoxelUtils
{
    public static class VoxelThinner
    {
        public static void Thin(this byte[][][] voxels)
        {
            var fullyOccludedLocations = new List<Tuple<int, int, int>>();

            for(var x = 1; x < voxels.Length - 1; x++)
            {
                for (var y = 1; y < voxels[x].Length - 1; y++)
                {
                    for (var z = 1; z < voxels[x][y].Length - 1; z++)
                    {
                        if(
                            voxels[x+1][y][z] !=0 &&
                            voxels[x-1][y][z] !=0 &&
                            voxels[x][y+1][z] !=0 &&
                            voxels[x][y - 1][z] != 0 &&
                            voxels[x][y][z + 1] != 0 &&
                            voxels[x][y][z - 1] != 0
                          )
                        {
                            fullyOccludedLocations.Add(new Tuple<int, int, int>(x, y, z));
                        }

                    }
                }
            }

            foreach(var location in fullyOccludedLocations)
            {
                voxels[location.Item1][location.Item2][location.Item3] = 0;
            }
        }
    }
}
