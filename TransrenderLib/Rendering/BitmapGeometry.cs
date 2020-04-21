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
            var allProjections = new[] { 24, 22, 32, 22, 24, 22, 32, 22 };
            return (int)(allProjections[projection] * 2 * Scale);
        }


        public int GetSpriteHeight(int projection)
        {
            var allProjections = new[] { 26, 22, 24, 22, 26, 22, 24, 22 };
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
