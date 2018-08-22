using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transrender.Palettes;
using Transrender.Projector;

namespace Transrender.Rendering
{
    public class Sprite
    {
        public int Height;
        public int Width;
        
        public List<ShaderResult>[][] PixelLists;

        private ISpriteRenderer _renderer;

        private bool _isDoubleSize;

        public Sprite(int projection, BitmapGeometry geometry, VoxelShader shader, IProjector projector)
        {
            SetRenderer(projection, geometry, shader, projector);
            _isDoubleSize = shader.Width > 64;

            var pixels = _renderer.GetPixels();

            for (var x = 0; x <= shader.Width; x += shader.Width)
            {
                for (var y = 0; y <= shader.Depth; y+= shader.Depth)
                {
                    for (var z = 0; z <= shader.Height; z+= shader.Height)
                    {
                        var screenSpace = projector.GetProjectedValues(x, y, z, projection, geometry.Scale);

                        if(_isDoubleSize)
                        {
                            screenSpace[0] = screenSpace[0] / 2;
                            screenSpace[1] = screenSpace[1] / 2;
                        }

                        Width = screenSpace[0] > Width ? screenSpace[0] : Width;
                        Height = screenSpace[1] > Height ? screenSpace[1] : Height;
                    }
                }
            }

            PixelLists = GetPixelLists(projection, pixels);
        }

        private void SetRenderer(int projection, BitmapGeometry geometry, VoxelShader shader, IProjector projector)
        {
            var rendererChoice = ConfigurationManager.AppSettings["renderer"] ?? "default";

            switch (rendererChoice.ToLower())
            {
                case "raycast":
                    _renderer = new RaycastRenderer(projection, geometry, shader, projector);
                    break;
                default:
                    _renderer = new PainterSpriteRenderer(projection, geometry, shader, projector);
                    break;
            }
        }

        private List<ShaderResult>[][] GetPixelLists(int projection, ShaderResult[][] pixels)
        {
            var renderFactor = BitmapGeometry.RenderScale;

            if(_isDoubleSize)
            {
                renderFactor = renderFactor * 2;
            }

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
