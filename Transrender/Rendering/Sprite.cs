using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transrender.Projector;

namespace Transrender.Rendering
{
    public class Sprite
    {
        public int Height;
        public int Width;
        
        public List<byte>[][] PixelLists;

        private ISpriteRenderer _renderer;

        public Sprite(int projection, BitmapGeometry geometry, VoxelShader shader, IProjector projector)
        {
            SetRenderer(projection, geometry, shader, projector);

            var pixels = _renderer.GetPixels();

            for (var x = 0; x <= shader.Width; x += shader.Width)
            {
                for (var y = 0; y <= shader.Depth; y+= shader.Depth)
                {
                    for (var z = 0; z <= shader.Height; z+= shader.Height)
                    {
                        var screenSpace = projector.GetProjectedValues(x, y, z, projection, geometry.Scale);

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

        private List<byte>[][] GetPixelLists(int projection, byte[][] pixels)
        {
            var renderFactor = BitmapGeometry.RenderScale;

            var width = pixels.Length / renderFactor + 1;
            var height = pixels[0].Length / renderFactor + 1;

            var list = new List<byte>[width][];

            for (var x = 0; x < pixels.Length; x++)
            {
                if (list[x / renderFactor] == null)
                {
                    list[x / renderFactor] = new List<byte>[height];
                }

                for (var y = 0; y < pixels[x].Length; y++)
                {
                    var source = pixels[x][y];

                    if (list[x / renderFactor][y / renderFactor] == null)
                    {
                        list[x / renderFactor][y / renderFactor] = new List<byte>();
                    }

                    list[x / renderFactor][y / renderFactor].Add(source);
                }
            }

            return list;
        }

    }
}
