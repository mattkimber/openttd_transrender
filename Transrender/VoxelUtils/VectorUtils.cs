using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.VoxelUtils
{
    public static class VectorUtils
    {
        public static Vector3 Round(this Vector3 vector)
        {
            return new Vector3((float)Math.Round(vector.X), (float)Math.Round(vector.Y), (float)Math.Round(vector.Z));
        }
    }
}
