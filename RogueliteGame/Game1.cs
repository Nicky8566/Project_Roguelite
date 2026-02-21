using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RogueliteGame.Networking;
using System;

namespace RogueliteGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Network client
        private NetworkClient networkClient;

        // Simple rendering
        private Texture2D pixelTexture;

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
            // Create network client
            networkClient = new NetworkClient();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create 1x1 white pixel for drawing shapes
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });

            // Connect to server
            networkClient.Connect("127.0.0.1", 12345, "Player1");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Get keyboard input
            KeyboardState keyState = Keyboard.GetState();
            InputKeys keys = InputKeys.None;

            if (keyState.IsKeyDown(Keys.W)) keys |= InputKeys.W;
            if (keyState.IsKeyDown(Keys.A)) keys |= InputKeys.A;
            if (keyState.IsKeyDown(Keys.S)) keys |= InputKeys.S;
            if (keyState.IsKeyDown(Keys.D)) keys |= InputKeys.D;
            if (keyState.IsKeyDown(Keys.Space)) keys |= InputKeys.Space;

            // Send input to server
            networkClient.SendInput(keys, 0, 0);

            // Receive state from server
            networkClient.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // Draw game state if we have one
            if (networkClient.HasNewState)
            {
                StateMessage state = networkClient.LastState;

                // Draw each entity
                foreach (var entity in state.Entities)
                {
                    if (!entity.Active) continue;

                    Color color = entity.Type switch
                    {
                        EntityType.Player => Color.Green,
                        EntityType.Enemy => Color.Red,
                        EntityType.Projectile => Color.Yellow,
                        _ => Color.White
                    };

                    // Draw as rectangle (simple for now)
                    Rectangle rect = new Rectangle(
                        (int)entity.X,
                        (int)entity.Y,
                        entity.Type == EntityType.Projectile ? 8 : 32,
                        entity.Type == EntityType.Projectile ? 8 : 32
                    );

                    _spriteBatch.Draw(pixelTexture, rect, color);
                }

                // Draw tick number
                // (You'll need a SpriteFont for this - for now, skip or add later)
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, Microsoft.Xna.Framework.ExitingEventArgs args)
        {
            networkClient.Disconnect();
            base.OnExiting(sender, args);
        }
    }
}