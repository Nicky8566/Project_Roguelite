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

        // Rendering
        private Texture2D pixelTexture;
        private Camera camera;

        // Sprite textures (ADD THESE VARIABLES)
        private Texture2D playerSprite;
        private Texture2D player2Sprite;
        private Texture2D player3Sprite;
        private Texture2D player4Sprite;
        private Texture2D enemySprite;
        private Texture2D projectileSprite;

        // Entities
        private Dictionary<uint, InterpolatedEntity> entities;
        private uint myPlayerId;

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
            camera = new Camera(1280, 720);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create 1x1 white pixel (fallback)
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            // Load sprites (with fallback)
            try
            {
                // Load player sprites
                playerSprite = Content.Load<Texture2D>("Sprites/player");
                player2Sprite = Content.Load<Texture2D>("Sprites/player2");
                player3Sprite = Content.Load<Texture2D>("Sprites/player3");
                player4Sprite = Content.Load<Texture2D>("Sprites/player4");

                // Load enemy sprite
                enemySprite = Content.Load<Texture2D>("Sprites/enemy");

                Console.WriteLine("Sprites loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load sprites: {ex.Message}");
                Console.WriteLine("Using colored squares as fallback.");

                // Fallback to colored squares
                playerSprite = CreateColoredSquare(Color.LimeGreen, 32);
                player2Sprite = CreateColoredSquare(Color.Blue, 32);
                player3Sprite = CreateColoredSquare(Color.Purple, 32);
                player4Sprite = CreateColoredSquare(Color.Orange, 32);
                enemySprite = CreateColoredSquare(Color.Red, 32);
            }

            // Always use yellow square for projectile
            projectileSprite = CreateColoredSquare(Color.Yellow, 8);

            // Connect to server
            networkClient.Connect("127.0.0.1", 12345, "Player1");
        }

        // HELPER METHOD: Create colored square texture (ADD THIS METHOD)
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

            // Get keyboard input
            KeyboardState keyState = Keyboard.GetState();
            InputKeys keys = InputKeys.None;

            if (keyState.IsKeyDown(Keys.W)) keys |= InputKeys.W;
            if (keyState.IsKeyDown(Keys.A)) keys |= InputKeys.A;
            if (keyState.IsKeyDown(Keys.S)) keys |= InputKeys.S;
            if (keyState.IsKeyDown(Keys.D)) keys |= InputKeys.D;
            if (keyState.IsKeyDown(Keys.Space)) keys |= InputKeys.Space;

            // // Send input to server
            // networkClient.SendInput(keys, 0, 0);

            // Get mouse position in world coordinates
            MouseState mouseState = Mouse.GetState();
            Vector2 mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);
            Vector2 mouseWorldPos = camera.ScreenToWorld(mouseScreenPos);

            // Send input with actual mouse position
            networkClient.SendInput(keys, mouseWorldPos.X, mouseWorldPos.Y);

            // Receive state from server
            networkClient.Update();

            // Update entities from server state
            if (networkClient.HasNewState)
            {
                StateMessage state = networkClient.LastState;

                HashSet<uint> receivedIds = new HashSet<uint>();

                foreach (var entityState in state.Entities)
                {
                    receivedIds.Add(entityState.EntityId);

                    if (entities.ContainsKey(entityState.EntityId))
                    {
                        var entity = entities[entityState.EntityId];
                        entity.SetTargetPosition(new Vector2(entityState.X, entityState.Y));
                        entity.Health = entityState.Health;
                        entity.Active = entityState.Active;
                    }
                    else
                    {
                        var entity = new InterpolatedEntity(
                            entityState.EntityId,
                            entityState.Type,
                            new Vector2(entityState.X, entityState.Y)
                        );
                        entity.Health = entityState.Health;
                        entity.Active = entityState.Active;
                        entities[entityState.EntityId] = entity;

                        if (entityState.Type == EntityType.Player && myPlayerId == 0)
                        {
                            myPlayerId = entityState.EntityId;
                        }
                    }
                }

                var toRemove = entities.Keys.Except(receivedIds).ToList();
                foreach (var id in toRemove)
                {
                    entities.Remove(id);
                }
            }

            // Update all entities (interpolation)
            foreach (var entity in entities.Values)
            {
                entity.Update(gameTime);
            }

            // Camera follows player
            if (myPlayerId != 0 && entities.ContainsKey(myPlayerId))
            {
                camera.Follow(entities[myPlayerId].Position, 0.1f);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            _spriteBatch.Begin(transformMatrix: camera.GetTransformMatrix());

            // Draw grid
            DrawGrid();

            // Draw all entities
            foreach (var entity in entities.Values)
            {
                if (!entity.Active) continue;

                // Get sprite for entity
                Texture2D sprite = GetSpriteForEntity(entity);

                // Calculate draw position (centered)
                Vector2 drawPos = new Vector2(
                    entity.Position.X - sprite.Width / 2,
                    entity.Position.Y - sprite.Height / 2
                );

                // Draw sprite
                _spriteBatch.Draw(sprite, drawPos, Color.White);

                // Draw health bar
                if (entity.Type != EntityType.Projectile)
                {
                    DrawHealthBar(entity);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        // HELPER METHOD: Get sprite for entity (ADD THIS METHOD)
        private Texture2D GetSpriteForEntity(InterpolatedEntity entity)
        {
            if (entity.Type == EntityType.Player)
            {
                // Different sprite for each player
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
            int barWidth = 32;
            int barHeight = 4;
            int yOffset = -20;

            Rectangle bgRect = new Rectangle(
                (int)entity.Position.X - barWidth / 2,
                (int)entity.Position.Y + yOffset,
                barWidth,
                barHeight
            );
            _spriteBatch.Draw(pixelTexture, bgRect, Color.DarkRed);

            float healthPercent = entity.Health / 100.0f;
            Rectangle fgRect = new Rectangle(
                bgRect.X,
                bgRect.Y,
                (int)(barWidth * healthPercent),
                barHeight
            );
            _spriteBatch.Draw(pixelTexture, fgRect, Color.LimeGreen);
        }
    }
}