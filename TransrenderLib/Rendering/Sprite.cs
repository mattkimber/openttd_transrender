using System.Collections.Generic;
using Transrender.Palettes;
using Transrender.Lighting;
using TransrenderLib.Rendering;

namespace Transrender.Rendering
{
    public class Sprite
    {
        public int Height;
        public int Width;
        
        public List<ShaderResult>[][] PixelLists;

        private ISpriteRenderer _renderer;


        public Sprite(int projection, BitmapGeometry geometry, VoxelShader shader, ILightingVectors lightingVectors, string rendererChoice)
        {
            SetRenderer(projection, geometry, shader, lightingVectors, rendererChoice);

            var pixels = _renderer.GetPixels();

            PixelLists = GetPixelLists(projection, pixels);
        }

        private void SetRenderer(int projection, BitmapGeometry geometry, VoxelShader shader, ILightingVectors lightingVectors, string rendererChoice)
        {
            switch (rendererChoice.ToLower())
            {
                default:
                    _renderer = new SimpleRaycastRenderer(projection, geometry, shader, lightingVectors);
                    break;
            }
        }

        private List<ShaderResult>[][] GetPixelLists(int projection, ShaderResult[][] pixels)
        {
            var renderFactor = BitmapGeometry.RenderScale;

            var width = pixels.Length / renderFactor + 1;
            var height = pixels[0].Length / renderFactor + 1;

            var list = new List<ShaderResult>[width][];

            for (var x = 0; x < pixels.Length; x++)
            {
                if (list[x / renderFactor] == null)
                {
                    list[x / renderFactor] = new List<ShaderResult>[height];
                }

                for (var y = 0; y < pixels[x].Length; y++)
                {
                    var source = pixels[x][y];

                    if (list[x / renderFactor][y / renderFactor] == null)
                    {
                        list[x / renderFactor][y / renderFactor] = new List<ShaderResult>();
                    }

                    list[x / renderFactor][y / renderFactor].Add(source);
                }
            }

            return list;
        }

    }
}
