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
        private const int GAME_SEED = 12345;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        private EcsWorld world;
        private MovementSystem movementSystem;
        private InputSystem inputSystem;
        private RenderSystem renderSystem;
        private ProjectileSystem projectileSystem;
        private DamageSystem damageSystem;
        private AISystem aiSystem;
        private UISystem uiSystem;
        private MenuSystem menuSystem;
        private WaveSystem waveSystem;

        private Dungeon dungeon;
        private DungeonRenderSystem dungeonRenderSystem;

        // Camera
        private Vector2 cameraPosition;
        private Matrix cameraTransform;

        // Game state
        private GameState currentState = GameState.MainMenu;
        private WaveComponent waveComponent;

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
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Load font
            _font = Content.Load<SpriteFont>("Font");
            
            menuSystem = new MenuSystem(GraphicsDevice, _font);
        }

        private void StartGame()
        {
            Console.WriteLine("=== STARTING NEW GAME ===");
            
            // Generate dungeon
            DungeonGenerator generator = new DungeonGenerator(seed: GAME_SEED);
            dungeon = generator.Generate(50, 50);

            // Create/reset ECS world
            if (world != null)
                world.Dispose();
            
            world = new EcsWorld();

            // Create systems
            movementSystem = new MovementSystem(world);
            inputSystem = new InputSystem(world, GraphicsDevice);
            projectileSystem = new ProjectileSystem(world);
            damageSystem = new DamageSystem(world);
            aiSystem = new AISystem(world);
            waveSystem = new WaveSystem(world, dungeon, GAME_SEED + 100);
            
            renderSystem = new RenderSystem(world, GraphicsDevice);
            dungeonRenderSystem = new DungeonRenderSystem(dungeon, GraphicsDevice);
            uiSystem = new UISystem(world, GraphicsDevice, _font);

            // Create player
            Random rng = new Random(GAME_SEED);
            Entity player = world.CreateEntity();
            Vector2 playerPos = dungeon.GetRandomFloorPosition(rng);
            player.Set(new Transform { Position = playerPos });
            player.Set(new Velocity { Value = Vector2.Zero });
            player.Set(new PlayerTag());
            player.Set(new Health { Current = 100, Max = 100 });

            Console.WriteLine($"Player spawned at ({playerPos.X:F0}, {playerPos.Y:F0})");

            // Initialize wave system
            waveComponent = new WaveComponent
            {
                CurrentWave = 0,
                EnemiesRemaining = 0,
                WaveDelay = 1f,
                TotalKills = 0
            };

            currentState = GameState.Playing;
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (currentState)
            {
                case GameState.MainMenu:
                    break;

                case GameState.Playing:
                    UpdatePlaying(deltaTime);
                    break;

                case GameState.GameOver:
                    break;
            }

            base.Update(gameTime);
        }

        private void UpdatePlaying(float deltaTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                currentState = GameState.MainMenu;
                return;
            }

            inputSystem.Update(deltaTime);
            aiSystem.Update(deltaTime);
            projectileSystem.Update(deltaTime);

            // Track kills
            int enemiesBefore = 0;
            foreach (var _ in world.GetEntities().With<AIState>().AsEnumerable())
                enemiesBefore++;

            damageSystem.Update(deltaTime);

            int enemiesAfter = 0;
            foreach (var _ in world.GetEntities().With<AIState>().AsEnumerable())
                enemiesAfter++;

            int killsThisFrame = enemiesBefore - enemiesAfter;
            if (killsThisFrame > 0)
            {
                waveComponent.TotalKills += killsThisFrame;
            }

            waveSystem.Update(deltaTime, ref waveComponent);

            // Movement with collision detection
            foreach (var entity in world.GetEntities().With<Transform>().With<Velocity>().AsEnumerable())
            {
                ref Transform transform = ref entity.Get<Transform>();
                ref Velocity velocity = ref entity.Get<Velocity>();

                if (entity.Has<Projectile>())
                {
                    Vector2 newPos = transform.Position + velocity.Value * deltaTime;

                    if (!dungeon.IsWalkable(newPos) || !dungeon.IsWalkable(newPos + new Vector2(7, 7)))
                    {
                        entity.Dispose();
                    }
                    else
                    {
                        transform.Position = newPos;
                    }
                    continue;
                }

                Vector2 newPosition = transform.Position + velocity.Value * deltaTime;
                bool canMove = IsPositionWalkable(newPosition);

                if (canMove)
                {
                    transform.Position = newPosition;
                }
                else
                {
                    if (!entity.Has<PlayerTag>())
                    {
                        velocity.Value *= -1;
                    }
                    else
                    {
                        velocity.Value = Vector2.Zero;
                    }
                }
            }

            // Check player death
            foreach (var player in world.GetEntities().With<PlayerTag>().With<Health>().AsEnumerable())
            {
                ref Health health = ref player.Get<Health>();
                if (health.Current <= 0)
                {
                    currentState = GameState.GameOver;
                }
                break;
            }

            UpdateCamera();
            inputSystem.SetCameraTransform(cameraTransform);
        }

        private bool IsPositionWalkable(Vector2 position)
        {
            const int entitySize = 32;
            const int margin = 2;

            bool topLeft = dungeon.IsWalkable(new Vector2(position.X + margin, position.Y + margin));
            bool topRight = dungeon.IsWalkable(new Vector2(position.X + entitySize - margin, position.Y + margin));
            bool bottomLeft = dungeon.IsWalkable(new Vector2(position.X + margin, position.Y + entitySize - margin));
            bool bottomRight = dungeon.IsWalkable(new Vector2(position.X + entitySize - margin, position.Y + entitySize - margin));

            return topLeft && topRight && bottomLeft && bottomRight;
        }

        private void UpdateCamera()
        {
            foreach (var entity in world.GetEntities().With<PlayerTag>().With<Transform>().AsEnumerable())
            {
                ref Transform transform = ref entity.Get<Transform>();

                cameraPosition.X = transform.Position.X + 16;
                cameraPosition.Y = transform.Position.Y + 16;

                cameraTransform = Matrix.CreateTranslation(
                    -cameraPosition.X + _graphics.PreferredBackBufferWidth / 2,
                    -cameraPosition.Y + _graphics.PreferredBackBufferHeight / 2,
                    0
                );

                break;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (currentState)
            {
                case GameState.MainMenu:
                    DrawMainMenu();
                    break;

                case GameState.Playing:
                    DrawPlaying();
                    break;

                case GameState.GameOver:
                    DrawGameOver();
                    break;
            }

            base.Draw(gameTime);
        }

        private void DrawMainMenu()
        {
            _spriteBatch.Begin();
            
            MenuAction action = menuSystem.DrawMainMenu(_spriteBatch);
            
            if (action == MenuAction.PlaySolo)
            {
                StartGame();
            }
            else if (action == MenuAction.Exit)
            {
                Exit();
            }
            
            _spriteBatch.End();
        }

        private void DrawPlaying()
        {
            _spriteBatch.Begin(transformMatrix: cameraTransform);
            dungeonRenderSystem.Draw(_spriteBatch);
            renderSystem.Update(_spriteBatch);
            _spriteBatch.End();

            _spriteBatch.Begin();
            uiSystem.Draw(_spriteBatch, waveComponent);
            _spriteBatch.End();
        }

        private void DrawGameOver()
        {
            // Draw game world faded
            _spriteBatch.Begin(transformMatrix: cameraTransform);
            dungeonRenderSystem.Draw(_spriteBatch);
            renderSystem.Update(_spriteBatch);
            _spriteBatch.End();

            // Draw death menu (with dark overlay)
            _spriteBatch.Begin();
            
            MenuAction action = menuSystem.DrawDeathMenu(_spriteBatch);
            
            if (action == MenuAction.Respawn)
            {
                StartGame();
            }
            else if (action == MenuAction.ExitToMenu)
            {
                currentState = GameState.MainMenu;
            }
            
            _spriteBatch.End();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                world?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}