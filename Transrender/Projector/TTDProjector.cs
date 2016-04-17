using System;

namespace Transrender.Projector
{
    public class TTDProjector : IProjector
    {
        private int _width;
        private int _height;
        private int _depth;

        public TTDProjector(int width, int height, int depth)
        {
            _width = width;
            _height = height;
            _depth = depth;
        }

        public int[] GetProjectedValues(double x, double y, double z, int projection, double scale)
        {
            var projectedX = (InterpolateProjection(XProjections, x, y, z, projection) * scale);
            var projectedY = (InterpolateProjection(YProjections, x, y, z, projection) * scale);

            return new[] { (int)(projectedX), (int)(projectedY) };
        }

        public int[][] GetShadowVector(int projection)
        {
            return _shadowVectors[projection];
        }

        private int[][][] _shadowVectors = {
            new[]{new[]{0, 1, -1}, new[]{-1, 1, -1}, new[]{1, 1, -1}},
            new[]{new[]{1, 1, -1}, new[]{0, 1, -1}, new[]{1, 0, -1}},
            new[]{new[]{1, 0, -1}, new[]{1, 1, -1}, new[]{1, -1, -1}},
            new[]{new[]{1, -1, -1}, new[]{1, 0, -1}, new[]{0, -1, -1}},
            new[]{new[]{0, -1, -1}, new[]{1, -1, -1}, new[]{-1, -1, -1}},
            new[]{new[]{-1, -1, -1}, new[]{0, -1, -1}, new[]{-1, 0, -1}},
            new[]{new[]{-1, 0, -1}, new[]{-1, 1, -1}, new[]{-1, -1, -1}},
            new[]{new[]{-1, 1, -1}, new[]{-1, 0, -1}, new[]{0, 1, -1}},
            new[]{new[]{0, 1, -1}, new[]{-1, 1, -1}, new[]{1, 1, -1}}
        };

        public double GetMaxProjectedWidth(int projection)
        {
            var allProjections = new[] {
                ((_depth) / 1.5),
                (((_width) / 2.0) + ((_depth) / 2.0)),
                (_width),
                (((_width) / 2.0) + ((_depth) / 2.0)),
                ((_depth) / 1.5),
                (((_width) / 2.0) + ((_depth) / 2.0)),
                (_width),
                (((_width) / 2.0) + ((_depth) / 2.0)),
                ((_depth) / 1.5)
            };

            return allProjections[projection];
        }


        public double GetMaxProjectedHeight(int projection)
        {
            var allProjections = new[] {
                (((_width) / 2.0) + (_height)),
                (((_width) / 4.0) + ((_depth) / 4.0) + (_height)),
                (((_depth) / 4.0) + (_height)),
                (((_width) / 4.0) + ((_depth) / 4.0) + (_height)),
                (((_width) / 2.0) + (_height)),
                (((_width) / 4.0) + ((_depth) / 4.0) + (_height)),
                (((_depth) / 4.0) + (_height)),
                (((_width) / 4.0) + ((_depth) / 4.0) + (_height)),
                (((_width) / 2.0) + (_height))
            };

            return allProjections[projection];
        }

        public Func<double, double, double, int, int, int, double>[] XProjections = {
            (x, y, z, width, height, depth) => (y / 1.5),
            (x, y, z, width, height, depth) => ((x / 2.0) + (y / 2.0)),
            (x, y, z, width, height, depth) => x,
            (x, y, z, width, height, depth) => ((x / 2.0) + ((depth - y - 1) / 2.0)),
            (x, y, z, width, height, depth) => ((depth - y - 1) / 1.5),
            (x, y, z, width, height, depth) => (((width - x - 1) / 2.0) + ((depth - y - 1) / 2.0)),
            (x, y, z, width, height, depth) => width - x - 1,
            (x, y, z, width, height, depth) => (((width - x - 1) / 2.0) + (y / 2.0)),
            (x, y, z, width, height, depth) => (y / 1.5)
        };

        public Func<double, double, double, int, int, int, double>[] YProjections = {
            (x, y, z, width, height, depth) => (((width - x - 1) / 2.0) + z),
            (x, y, z, width, height, depth) => ((width / 4.0) - (x / 4.0) + (y / 4.0) + z),
            (x, y, z, width, height, depth) => ((y / 4.0) + z),
            (x, y, z, width, height, depth) => (((height / 4.0) + (x / 4.0)-((height - y - 1) / 4.0)) + z),
            (x, y, z, width, height, depth) => ((x / 2.0) + z),
            (x, y, z, width, height, depth) => ((width / 4.0) - ((width - x - 1) / 4.0) + ((depth - y - 1) / 4.0) + z),
            (x, y, z, width, height, depth) => (((depth - y - 1) / 4.0) + z),
            (x, y, z, width, height, depth) => ((depth / 4.0) + ((width - x - 1) / 4.0) - (y / 4.0) + z),
            (x, y, z, width, height, depth) => (((width - x - 1) / 2.0) + z)
        };


        public double InterpolateProjection(Func<double, double, double, int, int, int, double>[] projectionFunctions,
                                         double x, double y, double z,
                                         int projection)
        {
            return projectionFunctions[projection](x, y, z, _width, _height, _depth);
        }
    }
}
