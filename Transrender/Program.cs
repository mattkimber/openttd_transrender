using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Transrender.Palettes;
using Transrender.Projector;
using Transrender.Rendering;
using VoxelLoader;

namespace Transrender
{
    class Program
    {
        static void Main(string[] args)
        {
            var palette = new TTDPalette();

            var files = Directory.GetFiles(Directory.GetCurrentDirectory()).Where(f => f.EndsWith(".vox"));

            Parallel.ForEach(files, file =>
            {
                var voxels = MagicaVoxelFileReader.Read(file);

                var shader = new VoxelShader(palette, voxels);
                var projector = new TTDProjector(shader.Width, shader.Height, shader.Depth);

                var renderer = new BitmapRenderer(shader, projector, palette, Convert.ToDouble(ConfigurationManager.AppSettings["renderScale"]));

                renderer.RenderToFile(file + ".png");

                Console.WriteLine(file);
            });
        }
    }
}
