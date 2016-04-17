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
        private VoxelShader _shader;
        private IProjector _projector;
        private IPalette _palette;
        private BitmapGeometry _geometry;
        private double _scale;

        private const int _renderScale = 2;

        public BitmapRenderer(VoxelShader shader, IProjector projector, IPalette palette, double scale = 1.0)
        {
            _shader = shader;
            _projector = projector;
            _palette = palette;
            _scale = scale;
            _geometry = new BitmapGeometry(scale);
        }

        private void RenderProjection(
            byte[] pixels,
            int xOffset,
            int yOffset,
            int stride,
            int projection)
        {
            var sprite = new Sprite(projection, _geometry, _shader, _projector);

            var finalXOffset = xOffset + (_geometry.GetSpriteWidth(projection) - (sprite.Width + 3));
            var finalYOffset = yOffset + (_geometry.GetSpriteHeight(projection) - (sprite.Height + 3));

            for (var x = 0; x < sprite.PixelLists.Length; x++)
            {
                for (var y = 0; y < (sprite.PixelLists[x] == null ? 0 : sprite.PixelLists[x].Length); y++)
                {
                    if(sprite.PixelLists[x][y] != null)
                    { 
                        var colour = _palette.GetCombinedColour(sprite.PixelLists[x][y]);
                        var destinationPixel = finalXOffset + x + ((finalYOffset + y) * stride);

                        if (destinationPixel < pixels.Length && (finalXOffset + x) >= 0 && (finalYOffset + y) >= 0 && colour != 0)
                        {
                            pixels[destinationPixel] = colour;
                        }
                    }
                }
            }
        }
        
        private static void RenderBox(byte[] pixels, int x1, int y1, int width, int height, int stride)
        {
            for (var x = x1; x < x1 + width; x++)
            {
                for (var y = y1; y < y1 + height; y++)
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

            for (int i = 0; i < 8; i ++)
            {
                var x = _geometry.GetSpriteLeft(i);
                var width = _geometry.GetSpriteWidth(i);
                var height = _geometry.GetSpriteHeight(i);
                RenderBox(pixels, x, 0, width, height, bitmap.Width);
                RenderProjection(pixels, x, 0, bitmap.Width, i);
            }


            Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);

            bitmap.UnlockBits(bitmapData);
        }

        public void RenderToFile(string fileName)
        {
            var width = _geometry.GetTotalWidth();
            var height = _geometry.GetTotalHeight();

            using (var b = new Bitmap(width, height, PixelFormat.Format8bppIndexed))
            {
                b.Palette = _palette.Palette;
                RenderVoxelObject(b);
                b.Save(fileName, ImageFormat.Png);
            }
        }
    }
}
