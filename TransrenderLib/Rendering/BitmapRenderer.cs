﻿using System.Drawing;
using System.Drawing.Imaging;
using Transrender.Palettes;
using Transrender.Lighting;

namespace Transrender.Rendering
{
    public class BitmapRenderer
    {
        private VoxelShader _shader;
        private ILightingVectors _lightingVectors;
        private IPalette _palette;
        private BitmapGeometry _geometry;
        private int _bitsPerPixel;
        private string _rendererChoice;
        

        public BitmapRenderer(VoxelShader shader, ILightingVectors lightingVectors, IPalette palette, string rendererChoice, double scale = 1.0, int bitsPerPixel = 8)
        {
            _shader = shader;
            _lightingVectors = lightingVectors;
            _palette = palette;
            _bitsPerPixel = bitsPerPixel;
            _geometry = new BitmapGeometry(scale);
            _rendererChoice = rendererChoice;
        }

        private void RenderProjection(
            IPixelBuffer buffer,
            int xOffset,
            int yOffset,
            int stride,
            int projection)
        {
            var sprite = new Sprite(projection, _geometry, _shader, _lightingVectors, _rendererChoice);

            for (var x = 0; x < sprite.PixelLists.Length; x++)
            {
                for (var y = 0; y < (sprite.PixelLists[x] == null ? 0 : sprite.PixelLists[x].Length); y++)
                {
                    if(sprite.PixelLists[x][y] != null)
                    { 
                        var colour = _palette.GetCombinedColour(sprite.PixelLists[x][y]);
                        var destinationPixel = xOffset + x + ((yOffset + y) * stride);

                        if (destinationPixel < buffer.GetLength() && (xOffset + x) >= 0 && (yOffset + y) >= 0 && colour.PaletteColour != 0)
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
