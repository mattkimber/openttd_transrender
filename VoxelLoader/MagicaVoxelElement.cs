using System.IO;

namespace VoxelLoader
{
    public class MagicaVoxelElement
    {
        public byte X;
        public byte Y;
        public byte Z;
        public byte Colour;

        public MagicaVoxelElement(BinaryReader stream)
        {
            X = stream.ReadByte();
            Y = stream.ReadByte();
            Z = stream.ReadByte();
            Colour = stream.ReadByte();
        }
    }
}
