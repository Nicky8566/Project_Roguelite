using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueliteGame.World;

namespace RogueliteGame.Systems
{
    public class DungeonRenderSystem
    {
        private Dungeon dungeon;
        private Texture2D pixelTexture;

        public DungeonRenderSystem(Dungeon dungeon, GraphicsDevice graphicsDevice)
        {
            this.dungeon = dungeon;
            
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < dungeon.Height; y++)
            {
                for (int x = 0; x < dungeon.Width; x++)
                {
                    TileType tile = dungeon.GetTile(x, y);
                   Color color = tile == TileType.Wall ? Color.DarkGray : Color.Gray;
                    
                    Rectangle rect = new Rectangle(
                        x * Dungeon.TileSize,
                        y * Dungeon.TileSize,
                        Dungeon.TileSize,
                        Dungeon.TileSize
                    );
                    
                    spriteBatch.Draw(pixelTexture, rect, color);
                }
            }
        }
    }
}