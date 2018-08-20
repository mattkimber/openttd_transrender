using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transrender.Palettes;
using Transrender.Projector;

namespace Transrender.Rendering
{
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

            var lastYLine = new List<Tuple<double, List<Tuple<double, ShaderResult>>>>();

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
                        foreach (var xLineItem in yLineItem.Item2)
                        {
                            var screenSpace = _projector.GetProjectedValues(xLineItem.Item1, yLineItem.Item1, z, _projection, renderScale);
                            if (screenSpace[0] < width && screenSpace[1] < height && screenSpace[0] >= 0 && screenSpace[1] >= 0)
                            {
                                result[screenSpace[0]][screenSpace[1]] = xLineItem.Item2;
                            }
                        }
                    }
                }
                else
                {
                    lastYLine = new List<Tuple<double, List<Tuple<double, ShaderResult>>>>();
                    var lastXLine = new List<Tuple<double, ShaderResult>>();

                    for (var y = flipY ? (double)_shader.Depth - 1 : 0.0; flipY ? y >= 0 : y < _shader.Depth; y += (flipY ? -step : step))
                    {
                        var roundedY = (int)Math.Round(y);
                        if (roundedY >= _shader.Depth)
                        {
                            roundedY = _shader.Depth - 1;
                        }

                        if (roundedY == lastY)
                        {
                            foreach (var lineItem in lastXLine)
                            {
                                var screenSpace = _projector.GetProjectedValues(lineItem.Item1, y, z, _projection, renderScale);
                                if (screenSpace[0] < width && screenSpace[1] < height && screenSpace[0] >= 0 && screenSpace[1] >= 0)
                                {
                                    result[screenSpace[0]][screenSpace[1]] = lineItem.Item2;
                                }
                            }
                        }
                        else
                        {
                            lastXLine = new List<Tuple<double, ShaderResult>>();

                            var currentProjectedValue = _projector.GetPreciseProjectedValues(xStart, y, z, _projection, renderScale);
                            var projectionStep = _projector.GetPreciseProjectedValues(xStart + xStep, y, z, _projection, renderScale);
                            projectionStep[0] = projectionStep[0] - currentProjectedValue[0];
                            projectionStep[1] = projectionStep[1] - currentProjectedValue[1];

                            for (var x = xStart; flipX ? x >= 0 : x < xGuard; x += xStep)
                            {
                                var roundedX = (int)Math.Round(x);

                                if (roundedX == lastX && roundedY == lastY && roundedZ == lastZ)
                                {
                                    if (lastResult != null)
                                    {
                                        lastXLine.Add(new Tuple<double, ShaderResult>(x, lastResult));
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
                                    lastXLine.Add(new Tuple<double, ShaderResult>(x, pixel));

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
                        lastYLine.Add(new Tuple<double, List<Tuple<double, ShaderResult>>>(y, lastXLine));
                    }
                }
                lastZ = roundedZ;
            }

            return result;
        }
    }
}
