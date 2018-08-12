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
        private int _bitsPerPixel;
        
        private const int _renderScale = 2;

        public BitmapRenderer(VoxelShader shader, IProjector projector, IPalette palette, double scale = 1.0, int bitsPerPixel = 8)
        {
            _shader = shader;
            _projector = projector;
            _palette = palette;
            _scale = scale;
            _bitsPerPixel = bitsPerPixel;
            _geometry = new BitmapGeometry(scale);
        }

        private void RenderProjection(
            IPixelBuffer buffer,
            int xOffset,
            int yOffset,
            int stride,
            int projection)
        {
            var sprite = new Sprite(projection, _geometry, _shader, _projector);

            var finalXOffset = (int)(xOffset + (_geometry.GetSpriteWidth(projection) - (sprite.Width + (4 * _scale))));
            var finalYOffset = (int)(yOffset + (_geometry.GetSpriteHeight(projection) - (sprite.Height + (4 * _scale))));

            if(projection == 0)
            {
                finalYOffset += (int)(2 * _scale);
            }
            else if(projection == 4)
            {
                finalYOffset += (int)(4 * _scale);
            }

            for (var x = 0; x < sprite.PixelLists.Length; x++)
            {
                for (var y = 0; y < (sprite.PixelLists[x] == null ? 0 : sprite.PixelLists[x].Length); y++)
                {
                    if(sprite.PixelLists[x][y] != null)
                    { 
                        var colour = _palette.GetCombinedColour(sprite.PixelLists[x][y]);
                        var destinationPixel = finalXOffset + x + ((finalYOffset + y) * stride);

                        if (destinationPixel < buffer.GetLength() && (finalXOffset + x) >= 0 && (finalYOffset + y) >= 0 && colour.PaletteColour != 0)
                        {
                            buffer.SetPixelToColour(destinationPixel, colour);
                        }
                    }
                }
            }
        }
        
        private static void RenderBox(IPixelBuffer pixelBuffer, int x1, int y1, int width, int height, int stride)
        {
            for (var x = x1; x < x1 + width; x++)
            {
                for (var y = y1; y < y1 + height; y++)
                {
                    pixelBuffer.SetPixelToColour(x + (y * stride), ShaderResult.Transparent());
                }
            }
        }

        private void RenderVoxelObject(Bitmap bitmap, Bitmap mask = null)
        {

            IPixelBuffer pixelBuffer;
            
            pixelBuffer = _bitsPerPixel == 8 ? (IPixelBuffer)(new PixelBuffer8Bit()) : (IPixelBuffer)(new PixelBuffer32Bit());
            pixelBuffer.CreateBuffer(bitmap.Width * bitmap.Height);

            for (int i = 0; i < pixelBuffer.GetLength(); i++)
            {
                pixelBuffer.SetPixelToColour(i, ShaderResult.White());
            }

            for (int i = 0; i < 8; i ++)
            {
                var x = _geometry.GetSpriteLeft(i);
                var width = _geometry.GetSpriteWidth(i);
                var height = _geometry.GetSpriteHeight(i);
                RenderBox(pixelBuffer, x, 0, width, height, bitmap.Width);
                RenderProjection(pixelBuffer, x, 0, bitmap.Width, i);
            }


            pixelBuffer.CopyToBitmap(bitmap);

            if(mask != null)
            {
                pixelBuffer.CopyToMask(mask);
            }
        }

        public void RenderToFile(string fileName)
        {
            var width = _geometry.GetTotalWidth();
            var height = _geometry.GetTotalHeight();

            if (_bitsPerPixel == 8)
            {
                using (var b = new Bitmap(width, height, PixelFormat.Format8bppIndexed))
                {
                    b.Palette = _palette.Palette;
                    RenderVoxelObject(b);
                    b.Save(fileName + ".png", ImageFormat.Png);
                }
            }
            else
            {
                var b = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                var m = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

                // This needs to be the TTD palette or NML will complain
                m.Palette = _palette.Palette;

                RenderVoxelObject(b, m);
 
                b.Save(fileName + ".png", ImageFormat.Png);
                m.Save(fileName + ".mask.png", ImageFormat.Png);
            }
        }
    }
}
