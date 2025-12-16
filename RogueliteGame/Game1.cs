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

        // ECS World and Systems
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
            // Create the ECS world (holds all entities)
            world = new World();

            // Create systems
            movementSystem = new MovementSystem(world);
            inputSystem = new InputSystem(world);
            bounceSystem = new BounceSystem(world);

            // ===== CREATE PLAYER =====
            Entity player = world.CreateEntity();
            player.Set(new Transform { Position = new Vector2(400, 300) });
            player.Set(new Velocity { Value = Vector2.Zero });
            player.Set(new PlayerTag()); // Mark it as the player
            player.Set(new Health { Current = 100, Max = 100 });

            // ===== CREATE 10 ENEMIES =====
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
                // Notice: NO PlayerTag, so InputSystem won't control them
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create render system (needs GraphicsDevice for texture)
            renderSystem = new RenderSystem(world, GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update all systems
            // Order matters! Input first, then movement
            inputSystem.Update(deltaTime);
            movementSystem.Update(deltaTime);
            bounceSystem.Update(deltaTime); // Add this line
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // RenderSystem draws all entities with Transform
            renderSystem.Update(_spriteBatch);

            _spriteBatch.End();

            // Count entities
            int entityCount = 0;
            foreach (var entity in world.GetEntities().AsEnumerable())
            {
                entityCount++;
            }
            // Draw count in window title
            Window.Title = $"Roguelite Game - Entities: {entityCount}";
            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up the ECS world
                world.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}