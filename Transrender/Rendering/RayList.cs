using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transrender.Projector;

namespace Transrender.Rendering
{
    public class Location
    {
        public byte X { get; private set; }
        public byte Y { get; private set; }
        public byte Z { get; private set; }

        public Location(byte x, byte y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class RayList
    {
        private byte[][][] _data;
        private List<Location>[][] _locations;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int SizeX { get; private set; }
        public int SizeY { get; private set; }
        public int SizeZ { get; private set; }

        public int Projection { get; private set; }

        public double Scale { get; private set; }

        public RayList(int projection, BitmapGeometry geometry, IProjector projector, int sizeX, int sizeY, int sizeZ)
        {
            Projection = projection;
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
            Scale = geometry.Scale;

            var renderScale = (Scale) * BitmapGeometry.RenderScale;
            
            SetBounds(geometry, renderScale);
            InitialiseScreenSpaceArray();
            PopulateScreenSpaceArray(projector, renderScale);
            ReverseScreenSpaceArray();
        }

        public RayList(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                Width = reader.ReadInt32();
                Height = reader.ReadInt32();
                SizeX = reader.ReadInt32();
                SizeY = reader.ReadInt32();
                SizeZ = reader.ReadInt32();
                Projection = reader.ReadInt32();
                Scale = reader.ReadDouble();

                _data = new byte[Width][][];

                for (var i = 0; i < Width; i++)
                {
                    _data[i] = new byte[Height][];

                    for (var j = 0; j < Height; j++)
                    {
                        var count = reader.ReadInt32();
                        var bytes = reader.ReadBytes(count * 3);
                        _data[i][j] = bytes;                       
                    }
                }

            }
        }

        public void SaveToFile(Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Width);
                writer.Write(Height);
                writer.Write(SizeX);
                writer.Write(SizeY);
                writer.Write(SizeZ);
                writer.Write(Projection);
                writer.Write(Scale);

                for (var i = 0; i < Width; i++)
                {
                    for (var j = 0; j < Height; j++)
                    {
                        writer.Write(_data[i][j].Length / 3);
                        writer.Write(_data[i][j]);
                    }
                }

                writer.Flush();
            }
        }

        public byte[] GetData(int x, int y)
        {
            return _data[x][y];
        }

        private void PopulateScreenSpaceArray(IProjector projector, double renderScale)
        {
            var flipX = Projection <= 2 || Projection >= 6;
            var flipY = Projection >= 3;

            var step = 1.0 / (renderScale);

            var xGuard = SizeX - 0.5;
            var xStep = flipX ? -step : step;
            var xStart = flipX ? (double)SizeX - 1 : 0.0;

            for (var z = (double)SizeZ; z >= 0; z -= step)
            {
                var roundedZ = (byte)Math.Round(z);
                for (var y = flipY ? (double)SizeY - 1 : 0.0; flipY ? y >= 0 : y < SizeY; y += (flipY ? -step : step))
                {
                    var roundedY = (byte)Math.Round(y);
                    var lastX = (byte)255;
                    var lastScreenX = -1;
                    var lastScreenY = -1;

                    for (var x = xStart; flipX ? x >= 0 : x < xGuard; x += xStep)
                    {
                        var roundedX = (byte)Math.Round(x);
                        var screenSpace = projector.GetProjectedValues(x, y, z, Projection, renderScale);
                        var isSameLocation = (roundedX == lastX && lastScreenX == screenSpace[0] && lastScreenY == screenSpace[1]);
                        if (
                            !isSameLocation &&
                            screenSpace[0] < Width && screenSpace[1] < Height && 
                            screenSpace[0] >= 0 && screenSpace[1] >= 0 &&
                            roundedX < SizeX && roundedY < SizeY && roundedZ < SizeZ)
                        {
                            _locations[screenSpace[0]][screenSpace[1]].Add(new Location(roundedX, roundedY, roundedZ));
                        }

                        lastX = roundedX;
                        lastScreenX = screenSpace[0];
                        lastScreenY = screenSpace[1];
                    }
                }
            }
        }

        private void InitialiseScreenSpaceArray()
        {
            _locations = new List<Location>[Width][];
            _data = new byte[Width][][];


            for (var i = 0; i < Width; i++)
            {
                _locations[i] = new List<Location>[Height];
                _data[i] = new byte[Height][];
                for(var j = 0; j < Height; j++)
                {
                    _locations[i][j] = new List<Location>();
                }
            }
        }

        private void ReverseScreenSpaceArray()
        {
            for (var i = 0; i < Width; i++)
            {
                for(var j = 0; j < Height; j++)
                {
                    _locations[i][j].Reverse();
                    _data[i][j] = _locations[i][j].SelectMany((l) => new[] { l.X, l.Y, l.Z }).ToArray();
                }
            }
        }

        private void SetBounds(BitmapGeometry geometry, double renderScale)
        {
            Width = (int)(geometry.GetSpriteWidth(Projection) * (renderScale / geometry.Scale));
            Height = (int)(geometry.GetSpriteHeight(Projection) * (renderScale / geometry.Scale));

            if (Projection == 0 || Projection == 4)
            {
                Height += (int)(4 * (renderScale / geometry.Scale));
            }

            if (SizeX > 64)
            {
                Width = Width * 2;
                Height = Height * 2;
            }
        }
    }
}
