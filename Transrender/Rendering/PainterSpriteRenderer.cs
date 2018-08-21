using System;
using System.Collections.Generic;
using System.Linq;
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
                        var projectionStep = _projector.GetPreciseProjectedValues(xStart + xStep, yLineItem.Item1, z, _projection, renderScale);
                        projectionStep[0] = projectionStep[0] - currentProjectedValue[0];
                        projectionStep[1] = projectionStep[1] - currentProjectedValue[1];

                        foreach (var xLineItem in yLineItem.Item2)
                        {
                            var screenSpace = new[] {
                                    (int)(currentProjectedValue[0] + (projectionStep[0] * xLineItem.Steps)),
                                    (int)(currentProjectedValue[1] + (projectionStep[1] * xLineItem.Steps))
                                };

                            if (screenSpace[0] < width && screenSpace[1] < height && screenSpace[0] >= 0 && screenSpace[1] >= 0)
                            {
                                result[screenSpace[0]][screenSpace[1]] = xLineItem.Result;
                            }
                        }
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
                        var projectionStep = _projector.GetPreciseProjectedValues(xStart + xStep, y, z, _projection, renderScale);
                        projectionStep[0] = projectionStep[0] - currentProjectedValue[0];
                        projectionStep[1] = projectionStep[1] - currentProjectedValue[1];

                        if (roundedY == lastY)
                        {
                            foreach (var lineItem in lastXLine)
                            {
                                var screenSpace = new[] {
                                    (int)(currentProjectedValue[0] + (projectionStep[0] * lineItem.Steps)),
                                    (int)(currentProjectedValue[1] + (projectionStep[1] * lineItem.Steps))
                                };

                                if (screenSpace[0] < width && screenSpace[1] < height && screenSpace[0] >= 0 && screenSpace[1] >= 0)
                                {
                                    result[screenSpace[0]][screenSpace[1]] = lineItem.Result;
                                }
                            }
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
                                    var screenSpace = currentProjectedValue; // _projector.GetProjectedValues(x, y, z, _projection, renderScale);

                                    var pixel = _shader.ShadePixel(roundedX, roundedY, roundedZ, _projection, _projector.GetLightingVector(_projection));
                                    lastXLine.Add(new ShaderLine(x, steps, lastResult));

                                    if (screenSpace[0] < width && screenSpace[1] < height && screenSpace[0] >= 0 && screenSpace[1] >= 0)
                                    {
                                        result[(int)screenSpace[0]][(int)screenSpace[1]] = pixel;
                                        lastResult = pixel;
                                    }
                                }
                                else
                                {
                                    lastResult = null;
                                }

                                lastX = roundedX;
                                currentProjectedValue[0] += projectionStep[0];
                                currentProjectedValue[1] += projectionStep[1];


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
    }
}
