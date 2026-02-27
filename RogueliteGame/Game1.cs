using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RogueliteGame.Networking;
using RogueliteGame.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueliteGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;


        // Network
        private NetworkClient networkClient;
        private int myKills = 0;
        private Dictionary<uint, bool> wasAlive = new Dictionary<uint, bool>();

        // Rendering
        private Texture2D pixelTexture;
        private Camera camera;

        // Sprite textures
        private Texture2D playerSprite;
        private Texture2D player2Sprite;
        private Texture2D player3Sprite;
        private Texture2D player4Sprite;
        private Texture2D enemySprite;
        private Texture2D projectileSprite;

        // Entities
        private Dictionary<uint, InterpolatedEntity> entities;
        private uint myPlayerId;
        private Dictionary<uint, string> playerNames;

        // NEW: Game state and name input
        private enum GameState
        {
            EnteringName,
            Playing
        }

        private GameState currentState = GameState.EnteringName;
        private string playerName = "";
        private KeyboardState previousKeyState;  // ← ADD THIS
        private SpriteFont font;
        
        // Wave system tracking
        private byte currentWave = 0;
        private bool waveActive = false;
        private float waveCountdown = 0f;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }

        protected override void Initialize()
        {
            networkClient = new NetworkClient();
            entities = new Dictionary<uint, InterpolatedEntity>();
            playerNames = new Dictionary<uint, string>();
            camera = new Camera(1280, 720);
            previousKeyState = Keyboard.GetState();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            // Load font
            try
            {
                font = Content.Load<SpriteFont>("Font");
            }
            catch
            {
                Console.WriteLine("Font not found");
            }

            // Load sprites
            try
            {
                playerSprite = Content.Load<Texture2D>("Sprites/player");
                player2Sprite = Content.Load<Texture2D>("Sprites/player2");
                player3Sprite = Content.Load<Texture2D>("Sprites/player3");
                player4Sprite = Content.Load<Texture2D>("Sprites/player4");
                enemySprite = Content.Load<Texture2D>("Sprites/enemy");
                Console.WriteLine("Sprites loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load sprites: {ex.Message}");
                playerSprite = CreateColoredSquare(Color.LimeGreen, 32);
                player2Sprite = CreateColoredSquare(Color.Blue, 32);
                player3Sprite = CreateColoredSquare(Color.Purple, 32);
                player4Sprite = CreateColoredSquare(Color.Orange, 32);
                enemySprite = CreateColoredSquare(Color.Red, 32);
            }

            projectileSprite = CreateColoredSquare(Color.Yellow, 8);
        }

        private Texture2D CreateColoredSquare(Color color, int size)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            for (int i = 0; i < data.Length; i++)
                data[i] = color;
            texture.SetData(data);
            return texture;
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (currentState == GameState.EnteringName)
            {
                KeyboardState keyState = Keyboard.GetState();
                Keys[] pressedKeys = keyState.GetPressedKeys();

                foreach (Keys key in pressedKeys)
                {
                    if (!previousKeyState.IsKeyDown(key))
                    {
                        if (key == Keys.Enter && playerName.Length > 0)
                        {
                            networkClient.Connect("127.0.0.1", 12345, playerName);
                            currentState = GameState.Playing;

                            playerNames[0] = playerName;  // Temporary, will update with real ID
                        }
                        else if (key == Keys.Back && playerName.Length > 0)
                        {
                            playerName = playerName.Substring(0, playerName.Length - 1);
                        }
                        else if (playerName.Length < 15)
                        {
                            char? character = GetCharFromKey(key, keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift));
                            if (character.HasValue)
                            {
                                playerName += character.Value;
                            }
                        }
                    }
                }

                previousKeyState = keyState;
                base.Update(gameTime);
                return;
            }

            // PLAYING STATE
            MouseState mouseState = Mouse.GetState();
            Vector2 mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);
            Vector2 mouseWorldPos = camera.ScreenToWorld(mouseScreenPos);

            KeyboardState keyState2 = Keyboard.GetState();
            InputKeys keys = InputKeys.None;

            if (keyState2.IsKeyDown(Keys.W)) keys |= InputKeys.W;
            if (keyState2.IsKeyDown(Keys.A)) keys |= InputKeys.A;
            if (keyState2.IsKeyDown(Keys.S)) keys |= InputKeys.S;
            if (keyState2.IsKeyDown(Keys.D)) keys |= InputKeys.D;
            if (keyState2.IsKeyDown(Keys.Space)) keys |= InputKeys.Space;

            networkClient.SendInput(keys, mouseWorldPos.X, mouseWorldPos.Y);
            networkClient.Update();

            if (networkClient.HasNewState)
            {
                StateMessage state = networkClient.LastState;
                HashSet<uint> receivedIds = new HashSet<uint>();
                
                // NEW: Update wave system state
                currentWave = state.CurrentWave;
                waveActive = state.WaveActive;
                waveCountdown = state.WaveCountdown;

                foreach (var entityState in state.Entities)
                {
                    receivedIds.Add(entityState.EntityId);

                    if (entities.ContainsKey(entityState.EntityId))
                    {
                        var entity = entities[entityState.EntityId];
                        entity.SetTargetPosition(new Vector2(entityState.X, entityState.Y));
                        entity.Health = entityState.Health;
                        entity.MaxHealth = entityState.MaxHealth;
                        entity.Active = entityState.Active;

                        // NEW: Track enemy deaths for kill counting
                        if (entity.Type == EntityType.Enemy)
                        {
                            bool wasAliveLastFrame = wasAlive.ContainsKey(entity.EntityId) && wasAlive[entity.EntityId];
                            bool isAliveNow = entity.Health > 0;

                            // If it was alive last frame but dead now, count as kill
                            if (wasAliveLastFrame && !isAliveNow)
                            {
                                myKills++;
                                Console.WriteLine($"Enemy died! Total kills: {myKills}");
                            }

                            wasAlive[entity.EntityId] = isAliveNow;
                        }
                    }
                    else
                    {
                        var entity = new InterpolatedEntity(
                            entityState.EntityId,
                            entityState.Type,
                            new Vector2(entityState.X, entityState.Y)
                        );
                        entity.Health = entityState.Health;
                        entity.MaxHealth = entityState.MaxHealth;  // ADD THIS LINE
                        entity.Active = entityState.Active;
                        entities[entityState.EntityId] = entity;

                        if (entityState.Type == EntityType.Player && myPlayerId == 0)
                        {
                            myPlayerId = entityState.EntityId;
                            if (playerNames.ContainsKey(0))
                            {
                                playerNames[myPlayerId] = playerNames[0];
                                playerNames.Remove(0);
                                entity.Name = playerNames[myPlayerId];
                            }
                        }
                        // Set name if we have it
                        if (playerNames.ContainsKey(entityState.EntityId))
                        {
                            entity.Name = playerNames[entityState.EntityId];
                        }
                    }
                }

                var toRemove = entities.Keys.Except(receivedIds).ToList();
                foreach (var id in toRemove)
                {
                    entities.Remove(id);
                }
            }

            foreach (var entity in entities.Values)
            {
                entity.Update(gameTime);
            }

            if (myPlayerId != 0 && entities.ContainsKey(myPlayerId))
            {
                camera.Follow(entities[myPlayerId].Position, 0.1f);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            if (currentState == GameState.EnteringName)
            {
                _spriteBatch.Begin();

                if (font != null)
                {
                    string prompt = "Enter your name:";
                    string nameDisplay = playerName + "_";
                    string instruction = "Press ENTER to join";

                    Vector2 promptSize = font.MeasureString(prompt);
                    Vector2 nameSize = font.MeasureString(nameDisplay);
                    Vector2 instructionSize = font.MeasureString(instruction);

                    _spriteBatch.DrawString(font, prompt,
                        new Vector2(640 - promptSize.X / 2, 300), Color.White);
                    _spriteBatch.DrawString(font, nameDisplay,
                        new Vector2(640 - nameSize.X / 2, 340), Color.Yellow);
                    _spriteBatch.DrawString(font, instruction,
                        new Vector2(640 - instructionSize.X / 2, 400), Color.Gray);
                }

                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            _spriteBatch.Begin(transformMatrix: camera.GetTransformMatrix());

            DrawGrid();

            foreach (var entity in entities.Values)
            {
                if (!entity.Active) continue;

                Texture2D sprite = GetSpriteForEntity(entity);

                Vector2 drawPos = new Vector2(
                    entity.Position.X - sprite.Width / 2,
                    entity.Position.Y - sprite.Height / 2
                );

                _spriteBatch.Draw(sprite, drawPos, Color.White);

                if (entity.Type != EntityType.Projectile)
                {
                    DrawHealthBar(entity);
                    if (entity.Type == EntityType.Player && !string.IsNullOrEmpty(entity.Name) && font != null)
                    {
                        DrawPlayerName(entity);
                    }
                }
            }

            _spriteBatch.End();

            // Draw UI overlay (no camera transform)
            _spriteBatch.Begin();
            DrawMinimap();
            DrawConnectionStatus();
            DrawWaveInfo();  // NEW: Wave countdown
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private char? GetCharFromKey(Keys key, bool shift)
        {
            if (key >= Keys.A && key <= Keys.Z)
            {
                char c = (char)('a' + (key - Keys.A));
                if (shift) c = char.ToUpper(c);
                return c;
            }
            if (key >= Keys.D0 && key <= Keys.D9 && !shift)
                return (char)('0' + (key - Keys.D0));
            if (key == Keys.Space)
                return ' ';
            return null;
        }

        private Texture2D GetSpriteForEntity(InterpolatedEntity entity)
        {
            if (entity.Type == EntityType.Player)
            {
                int playerIndex = (int)(entity.EntityId % 4);
                return playerIndex switch
                {
                    0 => playerSprite,
                    1 => player2Sprite,
                    2 => player3Sprite,
                    3 => player4Sprite,
                    _ => playerSprite
                };
            }
            else if (entity.Type == EntityType.Enemy)
            {
                return enemySprite;
            }
            else if (entity.Type == EntityType.Projectile)
            {
                return projectileSprite;
            }

            return pixelTexture;
        }

        private void DrawGrid()
        {
            Rectangle worldBounds = new Rectangle(-400, -300, 1600, 1200);

            for (int x = worldBounds.Left; x <= worldBounds.Right; x += 100)
            {
                Rectangle line = new Rectangle(x, worldBounds.Top, 1, worldBounds.Height);
                _spriteBatch.Draw(pixelTexture, line, Color.Gray * 0.3f);
            }

            for (int y = worldBounds.Top; y <= worldBounds.Bottom; y += 100)
            {
                Rectangle line = new Rectangle(worldBounds.Left, y, worldBounds.Width, 1);
                _spriteBatch.Draw(pixelTexture, line, Color.Gray * 0.3f);
            }
        }

        private void DrawHealthBar(InterpolatedEntity entity)
        {
            float barWidth = 40;
            float barHeight = 4;

            Vector2 barPos = new Vector2(
                entity.Position.X - barWidth / 2,
                entity.Position.Y - 25
            );

            // Background (red)
            Rectangle bgRect = new Rectangle((int)barPos.X, (int)barPos.Y, (int)barWidth, (int)barHeight);
            _spriteBatch.Draw(pixelTexture, bgRect, Color.Red);

            // Foreground (green) - based on health percentage
            float healthPercent = entity.MaxHealth > 0 ? (float)entity.Health / (float)entity.MaxHealth : 0f;
            Rectangle fgRect = new Rectangle(
                (int)barPos.X,
                (int)barPos.Y,
                (int)(barWidth * healthPercent),
                (int)barHeight
            );
            _spriteBatch.Draw(pixelTexture, fgRect, Color.LimeGreen);
        }
        // Add this method to Game1 class:
        private void DrawPlayerName(InterpolatedEntity entity)
        {
            if (font == null) return;

            // Measure text
            Vector2 textSize = font.MeasureString(entity.Name);

            // Scale down if needed (keep it small)
            float scale = 0.5f;
            Vector2 scaledSize = textSize * scale;

            // Position above health bar
            Vector2 textPos = new Vector2(
                entity.Position.X - scaledSize.X / 2,  // Center horizontally
                entity.Position.Y - 35                  // Above health bar
            );

            // Draw black shadow for readability
            _spriteBatch.DrawString(font, entity.Name, textPos + new Vector2(1, 1), Color.Black,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            // Draw white text
            _spriteBatch.DrawString(font, entity.Name, textPos, Color.White,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        private void DrawConnectionStatus()
        {
            // Top-right corner
            int x = 1200;
            int y = 20;
            int size = 12;

            // Status dot
            Color statusColor = networkClient.IsConnected ? Color.LimeGreen : Color.Red;
            Rectangle dot = new Rectangle(x, y, size, size);
            _spriteBatch.Draw(pixelTexture, dot, statusColor);

            // Status text (if font available)
            if (font != null)
            {
                string statusText = networkClient.IsConnected ? "CONNECTED" : "DISCONNECTED";
                Vector2 textPos = new Vector2(x + size + 5, y - 2);
                _spriteBatch.DrawString(font, statusText, textPos, statusColor,
                    0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0f);

                // NEW: Draw kill count below connection status
                if (networkClient.IsConnected && myKills >= 0)
                {
                    string killText = $"KILLS: {myKills}";
                    Vector2 killPos = new Vector2(x + size + 5, y + 18);
                    _spriteBatch.DrawString(font, killText, killPos, Color.Yellow,
                        0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0f);
                }
            }
        }
        
        private void DrawWaveInfo()
        {
            if (font == null) return;
            
            // Center top of screen
            int centerX = 640;
            int y = 20;
            
            if (!waveActive && waveCountdown > 0)
            {
                // Countdown between waves
                string countdownText = $"NEXT WAVE IN {(int)Math.Ceiling(waveCountdown)}";
                Vector2 textSize = font.MeasureString(countdownText);
                Vector2 textPos = new Vector2(centerX - textSize.X * 0.3f, y);
                
                // Draw with pulsing effect
                float pulse = (float)Math.Abs(Math.Sin(waveCountdown * 3)) * 0.3f + 0.7f;
                Color pulseColor = Color.Yellow * pulse;
                
                _spriteBatch.DrawString(font, countdownText, textPos, pulseColor,
                    0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
            }
            else if (waveActive && currentWave > 0)
            {
                // Show current wave
                string waveText = $"WAVE {currentWave}";
                Vector2 textSize = font.MeasureString(waveText);
                Vector2 textPos = new Vector2(centerX - textSize.X * 0.25f, y);
                
                _spriteBatch.DrawString(font, waveText, textPos, Color.Cyan,
                    0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
            }
        }
        
        private void DrawMinimap()
        {
            // Top-left corner
            int mapX = 20;
            int mapY = 20;
            int mapWidth = 200;
            int mapHeight = 150;

            // Semi-transparent background
            Rectangle mapBg = new Rectangle(mapX, mapY, mapWidth, mapHeight);
            _spriteBatch.Draw(pixelTexture, mapBg, Color.Black * 0.5f);

            // Border
            DrawRectangleOutline(mapX, mapY, mapWidth, mapHeight, Color.White);

            // World bounds (-400, -300 to 1200, 900)
            float worldWidth = 1600f;
            float worldHeight = 1200f;
            float worldOffsetX = 400f;
            float worldOffsetY = 300f;

            // Draw entities on minimap
            foreach (var entity in entities.Values)
            {
                if (!entity.Active) continue;

                // Convert world position to minimap position
                float normalizedX = (entity.Position.X + worldOffsetX) / worldWidth;
                float normalizedY = (entity.Position.Y + worldOffsetY) / worldHeight;

                int dotX = mapX + (int)(normalizedX * mapWidth);
                int dotY = mapY + (int)(normalizedY * mapHeight);

                // Clamp to minimap bounds
                dotX = Math.Clamp(dotX, mapX, mapX + mapWidth);
                dotY = Math.Clamp(dotY, mapY, mapY + mapHeight);

                // Color based on type
                Color dotColor = entity.Type switch
                {
                    EntityType.Player => entity.EntityId == myPlayerId ? Color.Yellow : Color.LimeGreen,
                    EntityType.Enemy => Color.Red,
                    _ => Color.White
                };

                // Draw dot (3x3 pixels)
                Rectangle dot = new Rectangle(dotX - 1, dotY - 1, 3, 3);
                _spriteBatch.Draw(pixelTexture, dot, dotColor);
            }
        }

        // Helper method to draw rectangle outline
        private void DrawRectangleOutline(int x, int y, int width, int height, Color color)
        {
            _spriteBatch.Draw(pixelTexture, new Rectangle(x, y, width, 1), color);           // Top
            _spriteBatch.Draw(pixelTexture, new Rectangle(x, y + height, width, 1), color);  // Bottom
            _spriteBatch.Draw(pixelTexture, new Rectangle(x, y, 1, height), color);          // Left
            _spriteBatch.Draw(pixelTexture, new Rectangle(x + width, y, 1, height), color);  // Right
        }
    }
}