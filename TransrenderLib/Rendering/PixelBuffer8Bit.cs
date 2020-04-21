using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Transrender.Palettes;

namespace Transrender.Rendering
{
    public class PixelBuffer8Bit : IPixelBuffer
    {
        private byte[] _pixels;

        void IPixelBuffer.CopyToBitmap(Bitmap bitmap)
        {
            var bitmapRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(bitmapRectangle, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            Marshal.Copy(_pixels, 0, bitmapData.Scan0, _pixels.Length);
            bitmap.UnlockBits(bitmapData);
        }

        void IPixelBuffer.CopyToMask(Bitmap mask)
        {
            throw new NotImplementedException("Cannot create a mask for an 8-bit sprite");
        }

        void IPixelBuffer.CreateBuffer(int length)
        {
            _pixels = new byte[length];
        }

        void IPixelBuffer.SetPixelToColour(int location, ShaderResult value)
        {
            _pixels[location] = value.PaletteColour;
        }

        public int GetLength()
        {
            return _pixels.Length;
        }
    }
}
