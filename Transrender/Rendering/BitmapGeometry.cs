using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transrender.Rendering
{
    public class BitmapGeometry
    {
        public double Scale { get; private set; }
        public const int RenderScale = 2;

        public BitmapGeometry(double scale)
        {
            Scale = scale;
        }

        public int GetSpriteWidth(int projection)
        {
            var allProjections = new[] { 10, 26, 36, 26, 10, 26, 36, 26 };

            return (int)(allProjections[projection] * 2 * Scale);
        }


        public int GetSpriteHeight(int projection)
        {
            return (int)(28 * 2 * Scale);
        }

        public int GetSpriteLeft(int projection)
        {
            var allSpriteLeftPositions = new[] { 0, 20, 50, 90, 120, 140, 170, 210 };
            return (int)(allSpriteLeftPositions[projection] * 2 * Scale);
        }

        public int GetTotalWidth()
        {
            return (int)(260 * 2 * Scale);
        }

        public int GetTotalHeight()
        {
            return (int)(32 * 2 * Scale);
        }

    }
}
