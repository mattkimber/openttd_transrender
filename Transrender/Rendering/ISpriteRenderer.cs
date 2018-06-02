using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transrender.Palettes;

namespace Transrender.Rendering
{
    public interface ISpriteRenderer
    {
        ShaderResult[][] GetPixels();
    }
}
