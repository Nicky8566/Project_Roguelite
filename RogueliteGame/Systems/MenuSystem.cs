using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RogueliteGame.Systems
{
    public class MenuSystem
    {
        private Texture2D pixelTexture;
        private SpriteFont font;
        private MouseState previousMouseState;
        
        // Menu buttons
        private Rectangle playButton;
        private Rectangle multiplayerButton;
        private Rectangle exitButton;
        
        // Death menu buttons
        private Rectangle respawnButton;
        private Rectangle exitButton2;

        public MenuSystem(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            this.font = font;
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
            previousMouseState = Mouse.GetState();
            
            // Main menu buttons (centered)
            int centerX = 640;
            int centerY = 360;
            
            playButton = new Rectangle(centerX - 150, centerY - 80, 300, 60);
            multiplayerButton = new Rectangle(centerX - 150, centerY - 10, 300, 60);
            exitButton = new Rectangle(centerX - 150, centerY + 60, 300, 60);
            
            // Death menu buttons
            respawnButton = new Rectangle(centerX - 150, centerY - 30, 300, 60);
            exitButton2 = new Rectangle(centerX - 150, centerY + 40, 300, 60);
        }

        public MenuAction DrawMainMenu(SpriteBatch spriteBatch)
        {
            MouseState mouseState = Mouse.GetState();
            
            // Title
            string title = "ROGUELITE DUNGEON";
            Vector2 titleSize = font.MeasureString(title);
            Vector2 titlePos = new Vector2((1280 - titleSize.X * 2) / 2, 150);
            spriteBatch.DrawString(font, title, titlePos, Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 0);
            
            // Play Solo button
            Color playColor = IsHovering(playButton, mouseState) ? Color.Lime : Color.Gray;
            DrawButton(spriteBatch, playButton, "PLAY SOLO", playColor);
            
            if (IsClicked(playButton, mouseState))
            {
                previousMouseState = mouseState;
                return MenuAction.PlaySolo;
            }
            
            // Multiplayer button (disabled)
            DrawButton(spriteBatch, multiplayerButton, "MULTIPLAYER", Color.DarkGray);
            spriteBatch.DrawString(font, "(Coming Soon)", new Vector2(640 - 50, 380), Color.Gray);
            
            // Exit button
            Color exitColor = IsHovering(exitButton, mouseState) ? Color.Red : Color.Gray;
            DrawButton(spriteBatch, exitButton, "EXIT", exitColor);
            
            if (IsClicked(exitButton, mouseState))
            {
                previousMouseState = mouseState;
                return MenuAction.Exit;
            }
            
            previousMouseState = mouseState;
            return MenuAction.None;
        }

        public MenuAction DrawDeathMenu(SpriteBatch spriteBatch)
        {
            MouseState mouseState = Mouse.GetState();
            
            // Dark overlay to make death screen obvious
            Rectangle fullScreen = new Rectangle(0, 0, 1280, 720);
            spriteBatch.Draw(pixelTexture, fullScreen, Color.Black * 0.7f);
            
            // Game Over title
            string title = "YOU DIED";
            Vector2 titleSize = font.MeasureString(title);
            Vector2 titlePos = new Vector2((1280 - titleSize.X * 3) / 2, 150); // Doubled size = multiply by 2
            spriteBatch.DrawString(font, title, titlePos, Color.Red, 0, Vector2.Zero, 3f, SpriteEffects.None, 0);
            
            // Respawn button (GREEN/BLACK)
            Color respawnColor = IsHovering(respawnButton, mouseState) ? Color.Lime : Color.DarkGray;
            DrawButton(spriteBatch, respawnButton, "RESPAWN", respawnColor);
            
            if (IsClicked(respawnButton, mouseState))
            {
                previousMouseState = mouseState;
                return MenuAction.Respawn;
            }
            
            // Exit button (RED/BLACK)
            Color exitColor = IsHovering(exitButton2, mouseState) ? Color.Red : Color.DarkGray;
            DrawButton(spriteBatch, exitButton2, "EXIT TO MENU", exitColor);
            
            if (IsClicked(exitButton2, mouseState))
            {
                previousMouseState = mouseState;
                return MenuAction.ExitToMenu;
            }
            
            previousMouseState = mouseState;
            return MenuAction.None;
        }

        private void DrawButton(SpriteBatch spriteBatch, Rectangle rect, string text, Color color)
        {
            // Button background (DARK)
            spriteBatch.Draw(pixelTexture, rect, Color.Black * 0.8f);
            
            // Button border (COLORED)
            DrawRectangleBorder(spriteBatch, rect, 4, color);
            
            // Button text (CENTERED)
            Vector2 textSize = font.MeasureString(text);
            Vector2 textPos = new Vector2(
                rect.X + rect.Width / 2 - textSize.X / 2,
                rect.Y + rect.Height / 2 - textSize.Y / 2
            );
            spriteBatch.DrawString(font, text, textPos, color);
        }

        private void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle rect, int thickness, Color color)
        {
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            spriteBatch.Draw(pixelTexture, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }

        private bool IsHovering(Rectangle button, MouseState mouseState)
        {
            return button.Contains(mouseState.Position);
        }

        private bool IsClicked(Rectangle button, MouseState mouseState)
        {
            return button.Contains(mouseState.Position) &&
                   mouseState.LeftButton == ButtonState.Released &&
                   previousMouseState.LeftButton == ButtonState.Pressed;
        }
    }

    public enum MenuAction
    {
        None,
        PlaySolo,
        Multiplayer,
        Exit,
        Respawn,
        ExitToMenu
    }
}