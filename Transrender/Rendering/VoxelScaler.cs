using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.Rendering
{
    public static class VoxelScaler
    {
        public static byte[][][] Scale(byte[][][] input, double factor)
        {
            return Scale(input, factor, factor, factor);
        }

        public static byte[][][] Scale(byte[][][] input, double xFactor, double yFactor, double zFactor)
        {
            var output = new byte[(int)(input.Length * xFactor)][][];

            for (int x = 0; x < (int)(input.Length * xFactor); x++)
            {
                output[x] = new byte[(int)(input[0].Length * yFactor)][];

                for (int y = 0; y < (int)(input[0].Length * yFactor); y++)
                {
                    output[x][y] = new byte[(int)(input[0][0].Length * zFactor)];

                    for (int z = 0; z < (int)(input[0][0].Length * zFactor); z++)
                    {
                        output[x][y][z] = input[(int)((x + 0.5) / xFactor)][(int)((y + 0.5) / yFactor)][(int)((z + 0.5) / zFactor)];
                    }
                }
            }

            return output;
        }
    }
}
