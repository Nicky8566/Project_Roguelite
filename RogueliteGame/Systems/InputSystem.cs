using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RogueliteGame.Components;
using EcsWorld = DefaultEcs.World;

namespace RogueliteGame.Systems
{
    public class InputSystem : AEntitySetSystem<float>
    {
        private const float PlayerSpeed = 200f;
        private const float BulletSpeed = 400f;
        private const float ShootCooldown = 0.2f;
        
        private EcsWorld world;
        private MouseState previousMouseState;
        private float shootTimer;
        private GraphicsDevice graphicsDevice;
        private Matrix cameraTransform;

        public InputSystem(EcsWorld world, GraphicsDevice graphicsDevice) 
            : base(world.GetEntities()
                .With<PlayerTag>()
                .With<Velocity>()
                .AsSet())
        {
            this.world = world;
            this.graphicsDevice = graphicsDevice;
            previousMouseState = Mouse.GetState();
            shootTimer = 0f;
        }

        public void SetCameraTransform(Matrix transform)
        {
            cameraTransform = transform;
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
            
            if (mouseState.LeftButton == ButtonState.Pressed && 
                previousMouseState.LeftButton == ButtonState.Released &&
                shootTimer <= 0f)
            {
                System.Console.WriteLine("=== MOUSE CLICKED ===");
                
                Vector2 playerCenter = new Vector2(
                    transform.Position.X + 16,
                    transform.Position.Y + 16
                );
                
                // Convert mouse screen position to world position
                Vector2 mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);
                Vector2 mouseWorldPos = ScreenToWorld(mouseScreenPos);
                
                System.Console.WriteLine($"Mouse screen: {mouseScreenPos}, world: {mouseWorldPos}");
                
                // Calculate direction from player to mouse
                Vector2 shootDirection = mouseWorldPos - playerCenter;
                
                // Handle case where mouse is exactly on player
                if (shootDirection.Length() < 0.1f)
                {
                    shootDirection = new Vector2(1, 0); // Default to right
                }
                else
                {
                    shootDirection.Normalize();
                }
                
                System.Console.WriteLine($"Shoot direction: {shootDirection}");
                
                // Create bullet entity
                Entity bullet = world.CreateEntity();
                bullet.Set(new Transform 
                { 
                    Position = playerCenter - new Vector2(4, 4)
                });
                bullet.Set(new Velocity 
                { 
                    Value = shootDirection * BulletSpeed 
                });
                bullet.Set(new Projectile 
                { 
                    Lifetime = 2.0f,
                    Damage = 10,
                    OwnerID = 0
                });
                
                shootTimer = ShootCooldown;
                
                System.Console.WriteLine($"BANG! Bullet created, aiming at mouse");
            }
            
            previousMouseState = mouseState;
        }

        private Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            // Invert the camera transformation
            Matrix invertedMatrix = Matrix.Invert(cameraTransform);
            
            // Transform screen position to world position
            Vector3 screenPos3D = new Vector3(screenPosition, 0);
            Vector3 worldPos3D = Vector3.Transform(screenPos3D, invertedMatrix);
            
            return new Vector2(worldPos3D.X, worldPos3D.Y);
        }
    }
}