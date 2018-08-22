﻿using System;
using System.Numerics;
using Transrender.VoxelUtils;

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
            var projectedX = (GetProjection(XProjections, x, y, z, projection) * scale);
            var projectedY = (GetProjection(YProjections, x, y, z, projection) * scale);

            return new[] { (int)(projectedX), (int)(projectedY) };
        }

        public Vector2 GetPreciseProjectedValues(double x, double y, double z, int projection, double scale)
        {
            return new Vector2(
                (float)(GetProjection(XProjections, x, y, z, projection) * scale),
                (float)(GetProjection(YProjections, x, y, z, projection) * scale)
            );
        }

        public Vector3 GetLightingVector(int projection)
        {
            return _lightingVectors[projection];
        }

        private Vector3[] _lightingVectors = new[]
        {
            new Vector3 { X = 0, Y = -1, Z = 2},
            new Vector3 { X = -1, Y = -1, Z = 2},
            new Vector3 { X = -1, Y = 0, Z = 2},
            new Vector3 { X = -1, Y = 1, Z = 2},
            new Vector3 { X = 0, Y = 1, Z = 2},
            new Vector3 { X = 1, Y = 1, Z = 2},
            new Vector3 { X = 1, Y = 0, Z = 2},
            new Vector3 { X = 1, Y = -1, Z = 2}
        };
        
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
            (x, y, z, width, height, depth) => (((width - x - 1) / 2.25) + z),
            (x, y, z, width, height, depth) => ((width / 4.0) - (x / 4.0) + (y / 4.0) + z),
            (x, y, z, width, height, depth) => ((y / 4.0) + z),
            (x, y, z, width, height, depth) => (((height / 4.0) + (x / 4.0)-((height - y - 1) / 4.0)) + z),
            (x, y, z, width, height, depth) => ((x / 2.25) + z),
            (x, y, z, width, height, depth) => ((width / 4.0) - ((width - x - 1) / 4.0) + ((depth - y - 1) / 4.0) + z),
            (x, y, z, width, height, depth) => (((depth - y - 1) / 4.0) + z),
            (x, y, z, width, height, depth) => ((depth / 4.0) + ((width - x - 1) / 4.0) - (y / 4.0) + z),
            (x, y, z, width, height, depth) => (((width - x - 1) / 2.0) + z)
        };


        public double GetProjection(Func<double, double, double, int, int, int, double>[] projectionFunctions,
                                         double x, double y, double z,
                                         int projection)
        {
            return projectionFunctions[projection](x, y, z, _width, _height, _depth);
        }
    }
}
