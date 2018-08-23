using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Transrender.Projector;

namespace Transrender.Rendering
{
    public class RayListCache
    {
        private List<RayList> _cache;
        private Object lockObject = new System.Object();

        public RayListCache()
        {
            _cache = new List<RayList>();
        }

        public RayList GetRayList(int projection, BitmapGeometry geometry, IProjector projector, int sizeX, int sizeY, int sizeZ)
        {
            lock (lockObject)
            {
                var result = _cache.Where(c =>
                    c.SizeX == sizeX &&
                    c.SizeY == sizeY &&
                    c.SizeZ == sizeZ &&
                    c.Projection == projection &&
                    c.Scale == geometry.Scale).FirstOrDefault();

                if (result == null)
                {
                    var filename = $"_cache/{sizeX}_{sizeY}_{sizeZ}_{projection}_{geometry.Scale:N2}.voxcache";
                    if (File.Exists(filename))
                    {
                        using (var file = File.OpenRead(filename))
                        {
                            result = new RayList(file);
                        }
                    }
                    else
                    {
                        result = new RayList(projection, geometry, projector, sizeX, sizeY, sizeZ);

                        if (!Directory.Exists("_cache"))
                        {
                            Directory.CreateDirectory("_cache");
                        }

                        using (var file = File.Create(filename))
                        {
                            result.SaveToFile(file);
                        }
                    }
                    _cache.Add(result);
                }

                return result;
            }
        }
    }
}
