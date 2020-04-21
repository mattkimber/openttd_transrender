using System;
using System.IO;

namespace VoxelLoader
{
    public static class MagicaVoxelFileReader
    {
        private static byte[][][] ReadFromStream(BinaryReader stream)
        {
            var magic = new string(stream.ReadChars(4));
            stream.ReadInt32();

            if (magic != "VOX ")
            {
                throw new ApplicationException("Not a MagicaVoxel file.");
            }

            int sizeX = 0, sizeY = 0, sizeZ = 0;

            while (stream.BaseStream.Position < stream.BaseStream.Length)
            {
                var chunkId = new string(stream.ReadChars(4));
                var chunkSize = stream.ReadInt32();
                stream.ReadInt32();

                switch (chunkId)
                {
                    case "SIZE":
                        sizeX = stream.ReadInt32();
                        sizeY = stream.ReadInt32();
                        sizeZ = stream.ReadInt32();
                        stream.ReadBytes(chunkSize - 4 * 3);
                        break;
                    case "XYZI":
                        {
                            var numVoxels = stream.ReadInt32();
                            var voxelData = new MagicaVoxelElement[numVoxels];
                            for (var i = 0; i < voxelData.Length; i++)
                            {
                                voxelData[i] = new MagicaVoxelElement(stream);
                            }

                            return GetVoxelArray(voxelData, sizeX, sizeY, sizeZ);
                        }
                    default:
                        stream.ReadBytes(chunkSize);
                        break;
                }
            }

            throw new ApplicationException("No voxel chunk in file.");

        }

        private static byte[][][] GetVoxelArray(MagicaVoxelElement[] voxels, int sizeX, int sizeY, int sizeZ)
        {
            var voxelArray = new byte[sizeX][][];

            for (var x = 0; x < sizeX; x++)
            {
                voxelArray[x] = new byte[sizeY][];
                for (var y = 0; y < sizeY; y++)
                {
                    voxelArray[x][y] = new byte[sizeZ];
                }
            }

            foreach (var voxel in voxels)
            {
                voxelArray[voxel.X][voxel.Y][voxel.Z] = (byte)(voxel.Colour - 2);
            }

            return voxelArray;
        }

        public static byte[][][] Read(string fileName)
        {
            using (var stream = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                return ReadFromStream(stream);
            }
        }

    }
}
