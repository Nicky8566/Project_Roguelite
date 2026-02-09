using DefaultEcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueliteGame.Components;
using System.Linq;

namespace RogueliteGame.Systems
{
    public class UISystem
    {
        private DefaultEcs.World world;
        private Texture2D pixelTexture;
        private SpriteFont font;  // We'll create a simple one

        public UISystem(DefaultEcs.World world, GraphicsDevice graphicsDevice)
        {
            this.world = world;
            
            // Create 1x1 white pixel for drawing rectangles
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Find player
            foreach (var player in world.GetEntities().With<PlayerTag>().With<Health>().AsEnumerable())
            {
                ref Health health = ref player.Get<Health>();
                
                // Health bar background (black)
                Rectangle bgRect = new Rectangle(20, 20, 200, 30);
                spriteBatch.Draw(pixelTexture, bgRect, Color.Black);
                
                // Health bar border (white)
                DrawRectangleBorder(spriteBatch, bgRect, 2, Color.White);
                
                // Health bar fill (green to red based on health)
                float healthPercent = (float)health.Current / health.Max;
                int fillWidth = (int)(196 * healthPercent);  // 196 = 200 - 4 (border)
                
                Color healthColor = Color.Lerp(Color.Red, Color.Lime, healthPercent);
                
                if (fillWidth > 0)
                {
                    Rectangle fillRect = new Rectangle(22, 22, fillWidth, 26);
                    spriteBatch.Draw(pixelTexture, fillRect, healthColor);
                }
                
                // Health text (we'll draw it manually without font for now)
                // Or just show the bar - it's clear enough
                
                break;  // Only one player
            }
            
            // Enemy count
            int enemyCount = world.GetEntities().With<AIState>().AsEnumerable().Count();
            
            // Enemy counter background
            Rectangle enemyBgRect = new Rectangle(20, 60, 150, 25);
            spriteBatch.Draw(pixelTexture, enemyBgRect, Color.Black);
            DrawRectangleBorder(spriteBatch, enemyBgRect, 2, Color.White);
            
            // You can add text here if you have a font, for now the bar is enough
        }

        private void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle rect, int thickness, Color color)
        {
            // Top
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            // Left
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }
    }
}