using DefaultEcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueliteGame.Components;
using System;
using System.Linq;

namespace RogueliteGame.Systems
{
    public class UISystem
    {
        private DefaultEcs.World world;
        private Texture2D pixelTexture;
        private SpriteFont font;

        public UISystem(DefaultEcs.World world, GraphicsDevice graphicsDevice, SpriteFont font)
        {
            this.world = world;
            this.font = font;
            
            // Create 1x1 white pixel for drawing rectangles
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
        }

        public void Draw(SpriteBatch spriteBatch, WaveComponent? wave = null)
        {
            // === HEALTH BAR ===
            foreach (var player in world.GetEntities().With<PlayerTag>().With<Health>().AsEnumerable())
            {
                ref Health health = ref player.Get<Health>();
                
                // Health bar background
                Rectangle bgRect = new Rectangle(20, 20, 200, 30);
                spriteBatch.Draw(pixelTexture, bgRect, Color.Black);
                DrawRectangleBorder(spriteBatch, bgRect, 3, Color.White);
                
                // Health bar fill (green to red based on health)
                float healthPercent = (float)health.Current / health.Max;
                int fillWidth = (int)(194 * healthPercent);
                
                Color healthColor = Color.Lerp(Color.Red, Color.Lime, healthPercent);
                
                if (fillWidth > 0)
                {
                    Rectangle fillRect = new Rectangle(23, 23, fillWidth, 24);
                    spriteBatch.Draw(pixelTexture, fillRect, healthColor);
                }
                
                // ACTUAL TEXT
                spriteBatch.DrawString(font, $"HP: {health.Current}/{health.Max}", 
                    new Vector2(230, 27), Color.White);
                
                break;
            }
            
            // === WAVE INFO ===
            if (wave.HasValue)
            {
                WaveComponent waveData = wave.Value;
                
                // Wave number
                spriteBatch.DrawString(font, $"WAVE: {waveData.CurrentWave}", 
                    new Vector2(20, 65), Color.Cyan);
                
                // Enemy count (RED bar shows this)
                spriteBatch.DrawString(font, $"ENEMIES: {waveData.EnemiesRemaining}", 
                    new Vector2(20, 90), Color.Red);
                
                // Kill count (YELLOW bar shows this)
                spriteBatch.DrawString(font, $"KILLS: {waveData.TotalKills}", 
                    new Vector2(20, 115), Color.Yellow);
                
                // Wave delay countdown
                if (waveData.WaveDelay > 0f)
                {
                    int seconds = (int)Math.Ceiling(waveData.WaveDelay);
                    spriteBatch.DrawString(font, $"NEXT WAVE IN {seconds}...", 
                        new Vector2(20, 145), Color.Lime);
                }
            }
        }

        private void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle rect, int thickness, Color color)
        {
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }
    }
}