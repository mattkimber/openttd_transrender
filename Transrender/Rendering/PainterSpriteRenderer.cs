﻿using System;
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

            for (var x = flipX ? (double)_shader.Width - 1 : 0.0; flipX ? x >= 0 : x < _shader.Width; x += (flipX ? -step : step))
            {
                var roundedX = (int)Math.Round(x);
                if(roundedX >= _shader.Width)
                {
                    roundedX = _shader.Width - 1;
                }

                for (var y = flipY ? (double)_shader.Depth - 1 : 0.0; flipY ? y >= 0 : y < _shader.Depth; y += (flipY ? -step : step))
                {
                    var roundedY = (int)Math.Round(y);
                    if (roundedY >= _shader.Depth)
                    {
                        roundedY = _shader.Depth - 1;
                    }

                    for (var z = 0.0; z < _shader.Height; z += step)
                    {
                        var roundedZ = (int)Math.Round(z);
                        if (roundedZ >= _shader.Height)
                        {
                            roundedZ = _shader.Height - 1;
                        }

                        if (!_shader.IsTransparent(roundedX, roundedY, roundedZ))
                        {
                            var screenSpace = _projector.GetProjectedValues(x, y, z, _projection, renderScale);

                            if (screenSpace[0] < width && screenSpace[1] < height && screenSpace[0] >= 0 && screenSpace[1] >= 0)
                            {
                                var pixel = _shader.ShadePixel(roundedX, roundedY, roundedZ, _projector.GetShadowVector(_projection));
                                result[screenSpace[0]][screenSpace[1]] = pixel;
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
