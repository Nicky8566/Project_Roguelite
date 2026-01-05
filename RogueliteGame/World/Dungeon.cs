using Microsoft.Xna.Framework;

namespace RogueliteGame.World
{
    public class Dungeon
    {
        private TileType[,] tiles;
        public const int TileSize = 32;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Dungeon(int width, int height)
        {
            Width = width;
            Height = height;
            tiles = new TileType[height, width];
            
            // Initialize all tiles as walls
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tiles[y, x] = TileType.Wall;
                }
            }
        }

        public TileType GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return TileType.Wall;
            
            return tiles[y, x];
        }

        public void SetTile(int x, int y, TileType type)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                tiles[y, x] = type;
            }
        }

        public bool IsWalkable(Vector2 position)
        {
            int tileX = (int)(position.X / TileSize);
            int tileY = (int)(position.Y / TileSize);

            if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
                return false;

            return tiles[tileY, tileX] == TileType.Floor;
        }

        public Vector2 GetRandomFloorPosition(System.Random rng)
        {
            // Find a random floor tile
            int attempts = 0;
            while (attempts < 1000)
            {
                int x = rng.Next(Width);
                int y = rng.Next(Height);
                
                if (tiles[y, x] == TileType.Floor)
                {
                    return new Vector2(x * TileSize + TileSize / 2, y * TileSize + TileSize / 2);
                }
                
                attempts++;
            }
            
            // Fallback
            return new Vector2(Width * TileSize / 2, Height * TileSize / 2);
        }
    }
}