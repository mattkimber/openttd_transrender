using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.VoxelUtils
{
    public class ProcessedVoxelElement
    {
        public Vector3 Normal { get; set; }
        public Vector3 AveragedNormal { get; set; }
        public byte Colour { get; set; }
        public bool IsSurface { get; set; }
        public bool IsShadowed { get; set; }
    }
}
