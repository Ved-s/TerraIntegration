namespace TerraIntegration.DataStructures
{
    public struct SpriteSheetPos 
    {
        public SpriteSheet SpriteSheet;
        public int X;
        public int Y;

        public bool HasValue;

        public SpriteSheetPos(SpriteSheet spriteSheet, int spriteX, int spriteY)
        {
            HasValue = true;
            SpriteSheet = spriteSheet;
            X = spriteX;
            Y = spriteY;
        }

        public SpriteSheetPos(int spriteX, int spriteY)
        {
            HasValue = true;
            SpriteSheet = null;
            X = spriteX;
            Y = spriteY;
        }
    }
}
