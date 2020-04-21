using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Transrender.Palettes;

namespace Transrender.Rendering
{
    public class PixelBuffer32Bit : IPixelBuffer
    {
        private byte[] _pixels;
        private byte[] _mask;

        void IPixelBuffer.CopyToBitmap(Bitmap bitmap)
        {
            var bitmapRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(bitmapRectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(_pixels, 0, bitmapData.Scan0, _pixels.Length);
            bitmap.UnlockBits(bitmapData);
        }

        void IPixelBuffer.CopyToMask(Bitmap mask)
        {
            var bitmapRectangle = new Rectangle(0, 0, mask.Width, mask.Height);
            var bitmapData = mask.LockBits(bitmapRectangle, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            Marshal.Copy(_mask, 0, bitmapData.Scan0, _mask.Length);
            mask.UnlockBits(bitmapData);
        }

        void IPixelBuffer.CreateBuffer(int length)
        {
            _mask = new byte[length];
            _pixels = new byte[length * 4];
        }

        void IPixelBuffer.SetPixelToColour(int location, ShaderResult value)
        {
            _mask[location] = value.M;

            var loc = location * 4;
            _pixels[loc] = value.B;
            _pixels[loc + 1] = value.G;
            _pixels[loc + 2] = value.R;
            _pixels[loc + 3] = (byte)(255 - value.A);
        }

        public int GetLength()
        {
            return _mask.Length;
        }
    }
}
