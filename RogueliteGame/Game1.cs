using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DefaultEcs;
using RogueliteGame.Components;
using RogueliteGame.Systems;
using RogueliteGame.World;
using System;
using EcsWorld = DefaultEcs.World;

namespace RogueliteGame
{
    public class Game1 : Game
    {
        private const int TEST_SEED = 23451;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private EcsWorld world;
        private MovementSystem movementSystem;
        private InputSystem inputSystem;
        private RenderSystem renderSystem;
        // Week 4: Shooting
        private ProjectileSystem projectileSystem;

        // Week 3: Dungeon
        private Dungeon dungeon;
        private DungeonRenderSystem dungeonRenderSystem;

        // Camera
        private Vector2 cameraPosition;
        private Matrix cameraTransform;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Normal laptop screen size
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }

        protected override void Initialize()
        {
            // Redirect console output to a file
            var logFile = System.IO.File.CreateText("spawn_debug.txt");
            logFile.AutoFlush = true;
            System.Console.SetOut(logFile);

            // Generate dungeon
            DungeonGenerator generator = new DungeonGenerator(seed: 12345);
            dungeon = generator.Generate(50, 50);

            Console.WriteLine("=== DUNGEON GENERATION COMPLETE ===");
            Console.WriteLine($"Seed: {TEST_SEED}");

            // Create ECS world
            world = new EcsWorld();

            // Create systems
            movementSystem = new MovementSystem(world);
            // NEW - pass GraphicsDevice
            inputSystem = new InputSystem(world, GraphicsDevice);
            projectileSystem = new ProjectileSystem(world); 
            Console.WriteLine("\n=== SPAWNING PLAYER ===");

            // Create player at a random floor position
            Random rng = new Random(TEST_SEED);
            Entity player = world.CreateEntity();
            Vector2 playerPos = dungeon.GetRandomFloorPosition(rng);
            player.Set(new Transform { Position = playerPos });
            player.Set(new Velocity { Value = Vector2.Zero });
            player.Set(new PlayerTag());
            player.Set(new Health { Current = 100, Max = 100 });

            // PRINTF: Check player spawn position
            int playerTileX = (int)(playerPos.X / Dungeon.TileSize);
            int playerTileY = (int)(playerPos.Y / Dungeon.TileSize);
            TileType playerTile = dungeon.GetTile(playerTileX, playerTileY);
            bool playerWalkable = IsPositionWalkable(playerPos);

            Console.WriteLine($"  Position: ({playerPos.X:F0}, {playerPos.Y:F0})");
            Console.WriteLine($"  Tile: ({playerTileX}, {playerTileY})");
            Console.WriteLine($"  Tile Type: {playerTile}");
            Console.WriteLine($"  Can spawn safely? {playerWalkable}");

            if (!playerWalkable)
            {
                Console.WriteLine("  ❌ PROBLEM: Player spawned in wall!");
            }
            else
            {
                Console.WriteLine("  ✓ Player spawn OK");
            }

            Console.WriteLine("\n=== SPAWNING ENEMIES ===");

            // Create 10 enemies at random floor positions
            for (int i = 0; i < 10; i++)
            {
                Entity enemy = world.CreateEntity();
                Vector2 enemyPos = dungeon.GetRandomFloorPosition(rng);
                enemy.Set(new Transform { Position = enemyPos });
                enemy.Set(new Velocity
                {
                    Value = new Vector2(rng.Next(-50, 50), rng.Next(-50, 50))
                });
                enemy.Set(new Health { Current = 50, Max = 50 });

                // PRINTF: Check enemy spawn position
                int enemyTileX = (int)(enemyPos.X / Dungeon.TileSize);
                int enemyTileY = (int)(enemyPos.Y / Dungeon.TileSize);
                TileType enemyTile = dungeon.GetTile(enemyTileX, enemyTileY);
                bool enemyWalkable = IsPositionWalkable(enemyPos);

                Console.Write($"  Enemy {i + 1}: ({enemyPos.X:F0}, {enemyPos.Y:F0}) -> ");

                if (!enemyWalkable)
                {
                    Console.WriteLine($"❌ SPAWNED IN WALL!");
                }
                else
                {
                    Console.WriteLine($"✓ OK");
                }
            }

            Console.WriteLine("\n=== SPAWN COMPLETE ===\n");

            Console.WriteLine("Press ENTER to start the game...");
            Console.ReadLine();  // ← ADD THIS - waits for you to press Enter

            base.Initialize();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            renderSystem = new RenderSystem(world, GraphicsDevice);
            dungeonRenderSystem = new DungeonRenderSystem(dungeon, GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update input
            inputSystem.Update(deltaTime);
            
            // Update projectiles (lifetime, despawn)
            projectileSystem.Update(deltaTime); 

            // Movement with PROPER collision detection for ALL entities
            foreach (var entity in world.GetEntities().With<Transform>().With<Velocity>().AsEnumerable())
            {
                ref Transform transform = ref entity.Get<Transform>();
                ref Velocity velocity = ref entity.Get<Velocity>();

                Vector2 newPosition = transform.Position + velocity.Value * deltaTime;

                // Check if new position is valid (all corners of the 32x32 entity)
                bool canMove = IsPositionWalkable(newPosition);

                if (canMove)
                {
                    transform.Position = newPosition;
                }
                else
                {
                    // Hit a wall - bounce enemies, stop player
                    if (!entity.Has<PlayerTag>())
                    {
                        // Enemy: reverse direction (bounce)
                        velocity.Value *= -1;
                    }
                    else
                    {
                        // Player: stop moving
                        velocity.Value = Vector2.Zero;
                    }
                }
            }

            // Update camera to follow player
            UpdateCamera();
             
            // NEW - Tell InputSystem about the camera position
            inputSystem.SetCameraTransform(cameraTransform);

            base.Update(gameTime);
        }

        private bool IsPositionWalkable(Vector2 position)
        {
            const int entitySize = 32;
            const int margin = 2; // Small margin to prevent getting stuck in corners

            // Check all four corners of the entity
            bool topLeft = dungeon.IsWalkable(new Vector2(position.X + margin, position.Y + margin));
            bool topRight = dungeon.IsWalkable(new Vector2(position.X + entitySize - margin, position.Y + margin));
            bool bottomLeft = dungeon.IsWalkable(new Vector2(position.X + margin, position.Y + entitySize - margin));
            bool bottomRight = dungeon.IsWalkable(new Vector2(position.X + entitySize - margin, position.Y + entitySize - margin));

            return topLeft && topRight && bottomLeft && bottomRight;
        }

        private void UpdateCamera()
        {
            // Find player position
            foreach (var entity in world.GetEntities().With<PlayerTag>().With<Transform>().AsEnumerable())
            {
                ref Transform transform = ref entity.Get<Transform>();

                // Center camera on player
                cameraPosition.X = transform.Position.X + 16; // +16 to center on 32x32 entity
                cameraPosition.Y = transform.Position.Y + 16;

                // Create camera transform matrix
                cameraTransform = Matrix.CreateTranslation(
                    -cameraPosition.X + _graphics.PreferredBackBufferWidth / 2,
                    -cameraPosition.Y + _graphics.PreferredBackBufferHeight / 2,
                    0
                );

                break; // Only one player
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Draw with camera transform
            _spriteBatch.Begin(transformMatrix: cameraTransform);

            // Draw dungeon first (background)
            dungeonRenderSystem.Draw(_spriteBatch);

            // Draw entities on top
            renderSystem.Update(_spriteBatch);

            _spriteBatch.End();

            // Draw UI without camera (screen-space)
            _spriteBatch.Begin();
            // Add UI here later (health bars, score, etc.)
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