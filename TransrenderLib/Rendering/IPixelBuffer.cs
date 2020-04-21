using System.Drawing;
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