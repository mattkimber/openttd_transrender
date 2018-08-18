using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.VoxelUtils
{
    public class Vector
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public class ProcessedVoxelElement
    {
        public Vector Normal { get; set; }
        public Vector AveragedNormal { get; set; }
        public byte Colour { get; set; }
        public bool IsSurface { get; set; }
        public bool IsShadowed { get; set; }
    }
}
