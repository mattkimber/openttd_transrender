using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.Projector
{
    public interface IProjector
    {
        int[] GetProjectedValues(double x, double y, double z, int projection, double scale);
        int[][] GetShadowVector(int projection);
        double GetMaxProjectedWidth(int projection);
        double GetMaxProjectedHeight(int projection);
    }
}
