using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Transrender.Configuration;
using Transrender.Palettes;
using Transrender.Projector;
using Transrender.Rendering;
using VoxelLoader;

namespace Transrender
{
    class Program
    {
        private static void WriteStatusBar(int current, int total, string file, double scale, int bpp)
        {
            Console.SetCursorPosition(0, 1);
            var pc = (current * 100) / total;
            var bar = new string('#', pc / 5);
            var remainder = new string('-', 20 - (pc / 5));
            Console.WriteLine("[" + bar + remainder + "]");
            Console.WriteLine($"Last File: {file} (scale: {scale:F1}, bpp: {bpp})        ");
        }

        static void Main(string[] args)
        {
            var palette = new TTDPalette();

            var files = Directory.GetFiles(Directory.GetCurrentDirectory()).Where(f => f.EndsWith(".vox"));
            var targets = (RenderTargetsSection)ConfigurationManager.GetSection("renderTargetsSection");
            var overwrite = Boolean.Parse(ConfigurationManager.AppSettings["overwriteExistingFiles"]);

            var total = files.Count() * targets.Targets.Count;
            var current = 0;

            var lockObject = new System.Object();

            Parallel.ForEach(files, file =>
            {
                var voxels = MagicaVoxelFileReader.Read(file);

                var shader = new VoxelShader(palette, voxels);
                var projector = new TTDProjector(shader.Width, shader.Height, shader.Depth);

                foreach(var target in targets.Targets)
                {
                    var renderer = new BitmapRenderer(shader, projector, palette, target.Scale, target.Bpp);

                    if(!Directory.Exists(target.OutputFolder))
                    {
                        Directory.CreateDirectory(target.OutputFolder);
                    }

                    var pathElements = file.Split('\\');
                    var path = string.Join("\\", pathElements.Take(pathElements.Length - 1)) + "\\" + target.OutputFolder + "\\";
                    var fileName = path + pathElements.Last();

                    if (overwrite || !File.Exists(fileName + ".png"))
                    {
                        renderer.RenderToFile(fileName);
                    }

                    lock (lockObject)
                    {
                        current++;
                        
                        WriteStatusBar(current, total, file, target.Scale, target.Bpp);

                    }
                }
            });
        }
    }
}
