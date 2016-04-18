using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public byte[][] GetPixels()
        {
            var flipX = _projection <= 2 || _projection >= 6;
            var flipY = _projection >= 3;

            var renderScale = (_geometry.Scale) * BitmapGeometry.RenderScale;

            var width = (int)(_geometry.GetSpriteWidth(_projection) * (renderScale / _geometry.Scale));
            var height = (int)(_geometry.GetSpriteHeight(_projection) * (renderScale / _geometry.Scale));

            var step = 1.0 / (renderScale);

            var result = new byte[width][];
            for (var i = 0; i < width; i++)
            {
                result[i] = new byte[height];
            }

            for (var x = flipX ? (double)_shader.Width - 1 : 0.0; flipX ? x >= 0 : x < _shader.Width; x += (flipX ? -step : step))
            {
                for (var y = flipY ? (double)_shader.Depth - 1 : 0.0; flipY ? y >= 0 : y < _shader.Depth; y += (flipY ? -step : step))
                {
                    for (var z = 0.0; z < _shader.Height; z += step)
                    {
                        var screenSpace = _projector.GetProjectedValues(x, y, z, _projection, renderScale);

                        if (!_shader.IsTransparent((int)x, (int)y, (int)z) && screenSpace[0] < width && screenSpace[1] < height && screenSpace[0] >= 0 && screenSpace[1] >= 0)
                        {
                            var pixel = _shader.ShadePixel((int)x, (int)y, (int)z, _projector.GetShadowVector(_projection));
                            result[screenSpace[0]][screenSpace[1]] = pixel;
                        }
                    }
                }
            }

            return result;
        }
    }
}
