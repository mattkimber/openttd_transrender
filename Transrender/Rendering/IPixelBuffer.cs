using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transrender.Palettes;

namespace Transrender.Rendering
{
    public interface IPixelBuffer
    {
        void CreateBuffer(int length);
        void SetPixelToColour(int location, ShaderResult value);

        void CopyToBitmap(Bitmap bitmap);
        void CopyToMask(Bitmap mask);

        int GetLength();
    }
}