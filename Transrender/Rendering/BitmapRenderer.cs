using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Transrender.Palettes;
using Transrender.Projector;

namespace Transrender.Rendering
{
    public class BitmapRenderer
    {
        private const int interSpriteWidth = 8;
        private const int leftBorder = 0;
        private const int topBorder = 0;

        private VoxelShader _shader;
        private IProjector _projector;
        private IPalette _palette;
        private double _scale;

        private const int _renderScale = 2;

        public BitmapRenderer(VoxelShader shader, IProjector projector, IPalette palette, double scale = 1.0)
        {
            _shader = shader;
            _projector = projector;
            _palette = palette;
            _scale = scale;
        }

        private byte[][] GetRenderedPixels(int projection)
        {
            var flipX = projection <= 2 || projection >= 6;
            var flipY = projection >= 3;

            var renderScale = _scale * (double)_renderScale;

            var width = (int)(_projector.GetMaxProjectedWidth(projection) * renderScale);
            var height = (int)(_projector.GetMaxProjectedHeight(projection) * renderScale);

            var step = 1.0 / (renderScale * 2.0);

            var result = new byte[width][];
            for(var i = 0; i < width; i++)
            {
                result[i] = new byte[height];
            }

            for (var x = flipX ? (double)_shader.Width - 1 : 0.0; flipX ? x >= 0 : x < _shader.Width; x += (flipX ? -step : step))
            {
                for (var y = flipY ? (double)_shader.Depth - 1 : 0.0; flipY ? y >= 0 : y < _shader.Depth; y += (flipY ? -step : step))
                {
                    for (var z = 0.0; z < _shader.Height; z+= step)
                    {
                        var screenSpace = _projector.GetProjectedValues(x, y, z, projection, renderScale);
                        if (!_shader.IsTransparent((int)x, (int)y, (int)z) && screenSpace[0] < width && screenSpace[1] < height)
                        {
                            var pixel = _shader.ShadePixel((int)x, (int)y, (int)z, screenSpace[0], screenSpace[1], _projector.GetShadowVector(projection));
                            if (screenSpace[0] > 0 && screenSpace[1] > 0)
                            {
                                result[screenSpace[0]][screenSpace[1]] = pixel;
                            }   
                        }
                    }
                }
            }

            return result;
        }
        
        private List<byte>[][] GetPixelLists(int projection)
        {
            var pixels = GetRenderedPixels(projection);

            var renderFactor = _renderScale;

            var width = pixels.Length / renderFactor + 1;
            var height = pixels[0].Length / renderFactor + 1;

            var list = new List<byte>[width][];

            for (var x = 0; x < pixels.Length; x++)
            {
                if (list[x/renderFactor] == null)
                {
                    list[x/renderFactor] = new List<byte>[height];
                }

                for (var y = 0; y < pixels[x].Length; y++)
                {
                    var source = pixels[x][y];
 
                    if(list[x/renderFactor][y/renderFactor] == null)
                    {
                        list[x/renderFactor][y/renderFactor] = new List<byte>();
                    }

                    list[x/renderFactor][y/renderFactor].Add(source);
                }
            }

            return list;
        }

        private void RenderProjection(
            byte[] pixels,
            int xOffset,
            int yOffset,
            int stride,
            int projection)
        {
            var pixelList = GetPixelLists(projection);

            for (var x = 0; x < pixelList.Length; x++)
            {
                for (var y = 0; y < (pixelList[x] == null ? 0 : pixelList[x].Length); y++)
                {
                    if(pixelList[x][y] != null)
                    { 
                        var colour = _palette.GetCombinedColour(pixelList[x][y]);
                        var destinationPixel = xOffset + x + ((y + yOffset) * stride);

                        if (destinationPixel < pixels.Length)
                        {
                            pixels[destinationPixel] = colour;
                        }
                    }
                }
            }
        }
        
        private static void RenderBox(byte[] pixels, int x1, int y1, int width, int height, int stride)
        {
            for (var x = x1; x <= x1 + width; x++)
            {
                for (var y = y1; y <= y1 + height; y++)
                {
                    pixels[x + (y * stride)] = 0;
                }
            }
        }

        private void RenderVoxelObject(Bitmap bitmap)
        {
            var bitmapRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(bitmapRectangle, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            var pixels = new byte[bitmap.Width * bitmap.Height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = 255;
            }

            int x = leftBorder;

            for (int i = 0; i < 8; i ++)
            {
                var width = (int)(_projector.GetMaxProjectedWidth(i) * _scale);
                var height = (int)(_projector.GetMaxProjectedHeight(i) * _scale);
                RenderBox(pixels, x, topBorder, width, height, bitmap.Width);
                RenderProjection(pixels, x, topBorder, bitmap.Width, i);
                x = x + width + interSpriteWidth;
            }


            Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);

            bitmap.UnlockBits(bitmapData);
        }

        public int GetBitmapWidth()
        {
            var width = 0;
            width += leftBorder;

            for (int i = 0; i < 8; i ++)
            {
                width += (int)(_projector.GetMaxProjectedWidth(i) * _scale) + interSpriteWidth;
            }

            return ((width + 1) / 4) * 4;
        }

        public void RenderToFile(string fileName)
        {
            var width = GetBitmapWidth();

            var maxWidth = _shader.Depth;

            var height = 2 *  (int)((_shader.Depth + maxWidth) * _scale) + (interSpriteWidth * 2) + topBorder;

            using (var b = new Bitmap(width, height, PixelFormat.Format8bppIndexed))
            {
                b.Palette = _palette.Palette;
                RenderVoxelObject(b);
                b.Save(fileName, ImageFormat.Png);
            }
        }

        /*
        public void RenderBlankFile(string fileName)
        {
            using (var b = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                b.Palette = _palette.Palette;
                if (!Directory.Exists("output/png"))
                {
                    Directory.CreateDirectory("output/png");
                }

                b.Save("output/" + fileName, ImageFormat.Png);
            }
        }
        */
    }
}
