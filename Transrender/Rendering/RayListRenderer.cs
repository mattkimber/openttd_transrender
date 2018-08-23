using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transrender.Palettes;
using Transrender.Projector;

namespace Transrender.Rendering
{
    public class RayListRenderer : ISpriteRenderer
    {
        private int _projection;
        private BitmapGeometry _geometry;
        private VoxelShader _shader;
        private IProjector _projector;
        private RayListCache _cache;

        public RayListRenderer(int projection, BitmapGeometry geometry, VoxelShader shader, IProjector projector, RayListCache cache)
        {
            _projection = projection;
            _geometry = geometry;
            _shader = shader;
            _projector = projector;
            _cache = cache;
        }

        public ShaderResult[][] GetPixels()
        {
            var rayList = _cache.GetRayList(_projection, _geometry, _projector, _shader.Width, _shader.Depth, _shader.Height);
            var result = new ShaderResult[rayList.Width][];
            var lightingVector = _projector.GetLightingVector(_projection);

            for(var i = 0; i < rayList.Width; i++)
            {
                result[i] = new ShaderResult[rayList.Height];
                for(var j = 0; j < rayList.Height; j++)
                {
                    var data = rayList.GetData(i, j);
                    for(var c = 0; c < data.Length; c+= 3)
                    {
                        if (!_shader.IsTransparent(data[c], data[c+1], data[c+2]))
                        {
                            var pixel = _shader.ShadePixel(data[c], data[c + 1], data[c + 2], _projection, lightingVector);
                            result[i][j] = pixel;
                            
                            // We can stop at the first non-zero pixel
                            break;
                        }
                    }
                }
            }

            return result;
        }
    }
}
