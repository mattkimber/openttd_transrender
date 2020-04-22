namespace Transrender.Rendering
{
    public class BitmapGeometry
    {
        public double Scale { get; private set; }
        public const int RenderScale = 4;

        public BitmapGeometry(double scale)
        {
            Scale = scale;
        }

        public int GetSpriteWidth(int projection)
        {
            var allProjections = new[] { 24, 26, 32, 26, 24, 26, 32, 26 };
            return (int)(allProjections[projection] * 2 * Scale);
        }


        public int GetSpriteHeight(int projection)
        {
            var allProjections = new[] { 26, 26, 24, 26, 26, 26, 24, 26 };
            return (int)(allProjections[projection] * 2 * Scale);
        }

        public int GetSpriteLeft(int projection)
        {
            var allSpriteLeftPositions = new[] { 0, 50, 100, 150, 200, 250, 300, 350 };
            return (int)(allSpriteLeftPositions[projection] * 2 * Scale);
        }

        public int GetTotalWidth()
        {
            return (int)(400 * 2 * Scale);
        }

        public int GetTotalHeight()
        {
            return (int)(40 * 2 * Scale);
        }

    }
}
