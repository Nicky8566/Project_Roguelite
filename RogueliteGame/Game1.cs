using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DefaultEcs;
using RogueliteGame.Components;
using RogueliteGame.Systems;
using System;

namespace RogueliteGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private World world;
        private MovementSystem movementSystem;
        private InputSystem inputSystem;
        private RenderSystem renderSystem;
        private BounceSystem bounceSystem;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            world = new World();

            movementSystem = new MovementSystem(world);
            inputSystem = new InputSystem(world);
            bounceSystem = new BounceSystem(world);

            Entity player = world.CreateEntity();
            player.Set(new Transform { Position = new Vector2(400, 300) });
            player.Set(new Velocity { Value = Vector2.Zero });
            player.Set(new PlayerTag());
            player.Set(new Health { Current = 100, Max = 100 });

            Random rng = new Random();
            for (int i = 0; i < 10; i++)
            {
                Entity enemy = world.CreateEntity();
                enemy.Set(new Transform 
                { 
                    Position = new Vector2(rng.Next(100, 700), rng.Next(100, 500)) 
                });
                enemy.Set(new Velocity 
                { 
                    Value = new Vector2(rng.Next(-100, 100), rng.Next(-100, 100)) 
                });
                enemy.Set(new Health { Current = 50, Max = 50 });
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            renderSystem = new RenderSystem(world, GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            inputSystem.Update(deltaTime);
            movementSystem.Update(deltaTime);
            bounceSystem.Update(deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            renderSystem.Update(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                world.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}