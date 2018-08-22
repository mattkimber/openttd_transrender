using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transrender.VoxelUtils;
using System.Numerics;

namespace Transrender.Projector
{
    public interface IProjector
    {
        int[] GetProjectedValues(double x, double y, double z, int projection, double scale);
        Vector2 GetPreciseProjectedValues(double x, double y, double z, int projection, double scale);

        Vector3 GetLightingVector(int projection);
    }
}
