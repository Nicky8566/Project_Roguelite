using Microsoft.Xna.Framework;

namespace RogueliteGame.World
{
    public class Dungeon
    {
        // This is the final result of generation. A 2D array of tiles.
        private TileType[,] tiles;
        public const int TileSize = 32;
        public int Width { get; private set; }
        public int Height { get; private set; }

        // instialise all tiles as walls
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

        // Check if a position in world coordinates is walkable
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
    const int entitySize = 32;
    const int margin = 2;
    
    int attempts = 0;
    while (attempts < 1000)
    {
        // Pick a random tile (not on edges)
        int x = rng.Next(1, Width - 1);
        int y = rng.Next(1, Height - 1);
        
        // Check if this tile AND all 8 neighbors are floor (3x3 area)
        if (tiles[y, x] == TileType.Floor &&
            tiles[y - 1, x - 1] == TileType.Floor &&  // Top-left
            tiles[y - 1, x] == TileType.Floor &&      // Top
            tiles[y - 1, x + 1] == TileType.Floor &&  // Top-right
            tiles[y, x - 1] == TileType.Floor &&      // Left
            tiles[y, x + 1] == TileType.Floor &&      // Right
            tiles[y + 1, x - 1] == TileType.Floor &&  // Bottom-left
            tiles[y + 1, x] == TileType.Floor &&      // Bottom
            tiles[y + 1, x + 1] == TileType.Floor)    // Bottom-right
        {
            // Position at top-left corner of tile
            Vector2 position = new Vector2(x * TileSize, y * TileSize);
            
            // Double-check all 4 corners of the 32x32 entity are walkable
            bool topLeft = IsWalkable(new Vector2(position.X + margin, position.Y + margin));
            bool topRight = IsWalkable(new Vector2(position.X + entitySize - margin, position.Y + margin));
            bool bottomLeft = IsWalkable(new Vector2(position.X + margin, position.Y + entitySize - margin));
            bool bottomRight = IsWalkable(new Vector2(position.X + entitySize - margin, position.Y + entitySize - margin));
            
            if (topLeft && topRight && bottomLeft && bottomRight)
            {
                return position;
            }
        }
        
        attempts++;
    }
    
    // If we fail 1000 times, do exhaustive search
    System.Console.WriteLine("WARNING: Searching entire map for safe spawn...");
    for (int y = 2; y < Height - 2; y++)
    {
        for (int x = 2; x < Width - 2; x++)
        {
            // Check 3x3 area
            bool allFloor = true;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (tiles[y + dy, x + dx] != TileType.Floor)
                    {
                        allFloor = false;
                        break;
                    }
                }
                if (!allFloor) break;
            }
            
            if (allFloor)
            {
                return new Vector2(x * TileSize, y * TileSize);
            }
        }
    }
    
    // Last resort fallback
    System.Console.WriteLine("ERROR: Could not find ANY safe spawn position!");
    return new Vector2(Width * TileSize / 2, Height * TileSize / 2);
}
    }
}