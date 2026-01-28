using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RogueliteGame.Components;
using EcsWorld = DefaultEcs.World;

namespace RogueliteGame.Systems
{
    public class InputSystem : AEntitySetSystem<float>
    {
        private const float PlayerSpeed = 200f;
        private const float BulletSpeed = 400f;
        private const float ShootCooldown = 0.2f; // 5 shots per second

        private EcsWorld world;
        private MouseState previousMouseState;
        private float shootTimer;

        public InputSystem(EcsWorld world)
            : base(world.GetEntities()
                .With<PlayerTag>()
                .With<Velocity>()
                .AsSet())
        {
            this.world = world;
            previousMouseState = Mouse.GetState();
            shootTimer = 0f;
        }

        protected override void Update(float deltaTime, in Entity entity)
        {
            ref Velocity velocity = ref entity.Get<Velocity>();
            ref Transform transform = ref entity.Get<Transform>();

            // === MOVEMENT (WASD) ===
            var keyboard = Keyboard.GetState();
            Vector2 input = Vector2.Zero;

            if (keyboard.IsKeyDown(Keys.W)) input.Y -= 1;
            if (keyboard.IsKeyDown(Keys.S)) input.Y += 1;
            if (keyboard.IsKeyDown(Keys.A)) input.X -= 1;
            if (keyboard.IsKeyDown(Keys.D)) input.X += 1;

            if (input.Length() > 0)
                input.Normalize();

            velocity.Value = input * PlayerSpeed;

            // === SHOOTING (MOUSE CLICK) ===
            shootTimer -= deltaTime;

            MouseState mouseState = Mouse.GetState();

            // Check if left mouse button is pressed (and cooldown ready)
            // Check if left mouse button is pressed (and cooldown ready)
            if (mouseState.LeftButton == ButtonState.Pressed &&
                previousMouseState.LeftButton == ButtonState.Released &&
                shootTimer <= 0f)
            {
                System.Console.WriteLine("=== MOUSE CLICKED ===");

                // Calculate direction from player to mouse
                Vector2 playerCenter = new Vector2(
                    transform.Position.X + 16,
                    transform.Position.Y + 16
                );

                System.Console.WriteLine($"Player center: {playerCenter}");

                // Get mouse position in world space
                Vector2 shootDirection = new Vector2(1, 0);
                shootDirection.Normalize();

                System.Console.WriteLine($"Shoot direction: {shootDirection}");

                System.Console.WriteLine("Creating bullet entity...");

                // Create bullet entity
                Entity bullet = world.CreateEntity();

                System.Console.WriteLine("Bullet entity created");

                bullet.Set(new Transform
                {
                    Position = playerCenter - new Vector2(4, 4)
                });

                System.Console.WriteLine("Transform set");

                bullet.Set(new Velocity
                {
                    Value = shootDirection * BulletSpeed
                });

                System.Console.WriteLine("Velocity set");

                bullet.Set(new Projectile
                {
                    Lifetime = 2.0f,
                    Damage = 10,
                    OwnerID = 0
                });

                System.Console.WriteLine("Projectile set - BULLET COMPLETE!");

                shootTimer = ShootCooldown;

                System.Console.WriteLine($"BANG! Bullet created at {playerCenter}");
            }

            previousMouseState = mouseState;
        }
    }
}