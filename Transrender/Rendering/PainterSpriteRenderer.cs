using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Transrender.Palettes;
using Transrender.Projector;

namespace Transrender.Rendering
{
    public class ShaderLine
    {
        public double X { get; private set; }
        public int Steps { get; private set; }
        public ShaderResult Result { get; private set; }

        public ShaderLine(double x, int steps, ShaderResult result)
        {
            X = x;
            Steps = steps;
            Result = result;
        }
    }

    public class PainterSpriteRenderer : ISpriteRenderer
    {
        private int _projection;
        private BitmapGeometry _geometry;
        private VoxelShader _shader;
        private IProjector _projector;

        public PainterSpriteRenderer(int projection, BitmapGeometry geometry, VoxelShader shader, IProjector projector)
        {
            _projection = projection;
            _geometry = geometry;
            _shader = shader;
            _projector = projector;
        }

        public ShaderResult[][] GetPixels()
        {
            var flipX = _projection <= 2 || _projection >= 6;
            var flipY = _projection >= 3;

            var renderScale = (_geometry.Scale) * BitmapGeometry.RenderScale;

            var width = (int)(_geometry.GetSpriteWidth(_projection) * (renderScale / _geometry.Scale));
            var height = (int)(_geometry.GetSpriteHeight(_projection) * (renderScale / _geometry.Scale));

            if(_projection == 0 || _projection == 4)
            {
                height += (int)(4 * (renderScale / _geometry.Scale));
            }

            if(_shader.Width > 64)
            {
                width = width * 2;
                height = height * 2;
            }

            var step = 1.0 / (renderScale);

            var result = new ShaderResult[width][];
            for (var i = 0; i < width; i++)
            {
                result[i] = new ShaderResult[height];
            }

            int lastZ = -1, lastY = -1, lastX = -1;
            var lastResult = new ShaderResult();
            var xGuard = _shader.Width - 0.5;
            var xStep = flipX ? -step : step;
            var xStart = flipX ? (double)_shader.Width - 1 : 0.0;

            var lastYLine = new List<Tuple<double, List<ShaderLine>>>();

            for (var z = (double)_shader.Height; z >= 0; z -= step)
            {
                var roundedZ = (int)Math.Round(z);
                if (roundedZ >= _shader.Height)
                {
                    roundedZ = _shader.Height - 1;
                }

                if (roundedZ == lastZ)
                {
                    foreach (var yLineItem in lastYLine)
                    {
                        var currentProjectedValue = _projector.GetPreciseProjectedValues(xStart, yLineItem.Item1, z, _projection, renderScale);
                        var projectionStep = GetProjectionStep(xStep, xStart, yLineItem.Item1, z, currentProjectedValue, renderScale);
                        RenderLine(width, height, result, yLineItem.Item2, currentProjectedValue, projectionStep);
                    }
                }
                else
                {
                    lastYLine = new List<Tuple<double, List<ShaderLine>>>();
                    var lastXLine = new List<ShaderLine>();

                    for (var y = flipY ? (double)_shader.Depth - 1 : 0.0; flipY ? y >= 0 : y < _shader.Depth; y += (flipY ? -step : step))
                    {
                        var roundedY = (int)Math.Round(y);
                        if (roundedY >= _shader.Depth)
                        {
                            roundedY = _shader.Depth - 1;
                        }

                        var currentProjectedValue = _projector.GetPreciseProjectedValues(xStart, y, z, _projection, renderScale);
                        var projectionStep = GetProjectionStep(xStep, xStart, y, z, currentProjectedValue, renderScale);

                        if (roundedY == lastY)
                        {
                            RenderLine(width, height, result, lastXLine, currentProjectedValue, projectionStep);                           
                        }
                        else
                        {
                            lastXLine = new List<ShaderLine>();

                            var steps = 0;

                            for (var x = xStart; flipX ? x >= 0 : x < xGuard; x += xStep)
                            {
                                var roundedX = (int)Math.Round(x);
                                steps++;

                                if (roundedX == lastX && roundedY == lastY && roundedZ == lastZ)
                                {
                                    if (lastResult != null)
                                    {
                                        lastXLine.Add(new ShaderLine(x, steps, lastResult));
                                        var screenSpace = _projector.GetProjectedValues(x, y, z, _projection, renderScale);
                                        if (screenSpace[0] < width && screenSpace[1] < height && screenSpace[0] >= 0 && screenSpace[1] >= 0)
                                        {
                                            result[screenSpace[0]][screenSpace[1]] = lastResult;
                                        }
                                    }
                                }
                                else if (!_shader.IsTransparent(roundedX, roundedY, roundedZ))
                                {
                                    var screenSpace = currentProjectedValue; 

                                    var pixel = _shader.ShadePixel(roundedX, roundedY, roundedZ, _projection, _projector.GetLightingVector(_projection));
                                    lastXLine.Add(new ShaderLine(x, steps, lastResult));

                                    if (screenSpace.X < width && screenSpace.Y < height && screenSpace.X >= 0 && screenSpace.Y >= 0)
                                    {
                                        result[(int)screenSpace.X][(int)screenSpace.Y] = pixel;
                                        lastResult = pixel;
                                    }
                                }
                                else
                                {
                                    lastResult = null;
                                }

                                lastX = roundedX;
                                currentProjectedValue += projectionStep;


                            }
                        }

                        lastY = roundedY;
                        lastYLine.Add(new Tuple<double, List<ShaderLine>>(y, lastXLine));
                    }
                }
                lastZ = roundedZ;
            }

            return result;
        }

        private static void RenderLine(int width, int height, ShaderResult[][] result, List<ShaderLine> line, Vector2 currentProjectedValue, Vector2 projectionStep)
        {
            foreach (var lineItem in line)
            {
                var screenSpace = currentProjectedValue + (projectionStep * lineItem.Steps);
 
                if (screenSpace.X < width && screenSpace.Y < height && screenSpace.X >= 0 && screenSpace.Y >= 0)
                {
                    result[(int)screenSpace.X][(int)screenSpace.Y] = lineItem.Result;
                }
            }
        }

        private Vector2 GetProjectionStep(double xStep, double xStart, double y, double z, Vector2 currentProjectedValue, double renderScale)
        {
            return _projector.GetPreciseProjectedValues(xStart + xStep, y, z, _projection, renderScale) - currentProjectedValue;
        }
    }
}
