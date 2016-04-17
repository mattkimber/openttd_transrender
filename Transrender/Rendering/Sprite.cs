using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transrender.Projector;

namespace Transrender.Rendering
{
    public class Sprite
    {
        public int Height;
        public int Width;
        
        public List<byte>[][] PixelLists;

        public Sprite(int projection, BitmapGeometry geometry, VoxelShader shader, IProjector projector)
        {
            var flipX = projection <= 2 || projection >= 6;
            var flipY = projection >= 3;

            var renderScale = (geometry.Scale) * BitmapGeometry.RenderScale;

            var width = (int)(geometry.GetSpriteWidth(projection) * (renderScale / geometry.Scale));
            var height = (int)(geometry.GetSpriteHeight(projection) * (renderScale / geometry.Scale));

            var step = 1.0 / (renderScale * 2.0);

            var result = new byte[width][];
            for (var i = 0; i < width; i++)
            {
                result[i] = new byte[height];
            }

            for (var x = flipX ? (double)shader.Width - 1 : 0.0; flipX ? x >= 0 : x < shader.Width; x += (flipX ? -step : step))
            {
                for (var y = flipY ? (double)shader.Depth - 1 : 0.0; flipY ? y >= 0 : y < shader.Depth; y += (flipY ? -step : step))
                {
                    for (var z = 0.0; z < shader.Height; z += step)
                    {
                        var screenSpace = projector.GetProjectedValues(x, y, z, projection, renderScale);

                        Width = screenSpace[0] * geometry.Scale > Width ? (int)(screenSpace[0] * geometry.Scale) : Width;
                        Height = screenSpace[1] * geometry.Scale > Height ? (int)(screenSpace[1] * geometry.Scale) : Height;

                        if (!shader.IsTransparent((int)x, (int)y, (int)z) && screenSpace[0] < width && screenSpace[1] < height && screenSpace[0] >= 0 && screenSpace[1] >= 0)
                        {
                            var pixel = shader.ShadePixel((int)x, (int)y, (int)z, screenSpace[0], screenSpace[1], projector.GetShadowVector(projection));
                            result[screenSpace[0]][screenSpace[1]] = pixel;
                        }
                    }
                }
            }

            PixelLists = GetPixelLists(projection, result);
        }

        private List<byte>[][] GetPixelLists(int projection, byte[][] pixels)
        {
            var renderFactor = BitmapGeometry.RenderScale;

            var width = pixels.Length / renderFactor + 1;
            var height = pixels[0].Length / renderFactor + 1;

            var list = new List<byte>[width][];

            for (var x = 0; x < pixels.Length; x++)
            {
                if (list[x / renderFactor] == null)
                {
                    list[x / renderFactor] = new List<byte>[height];
                }

                for (var y = 0; y < pixels[x].Length; y++)
                {
                    var source = pixels[x][y];

                    if (list[x / renderFactor][y / renderFactor] == null)
                    {
                        list[x / renderFactor][y / renderFactor] = new List<byte>();
                    }

                    list[x / renderFactor][y / renderFactor].Add(source);
                }
            }

            return list;
        }

    }
}
