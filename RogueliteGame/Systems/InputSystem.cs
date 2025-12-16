using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RogueliteGame.Components;

namespace RogueliteGame.Systems
{
    public class InputSystem : AEntitySetSystem<float>
    {
        private const float PlayerSpeed = 200f;

        public InputSystem(World world) 
            : base(world.GetEntities()
                .With<PlayerTag>()
                .With<Velocity>()
                .AsSet())
        { }

        protected override void Update(float deltaTime, in Entity entity)
        {
            ref Velocity velocity = ref entity.Get<Velocity>();
            
            var keyboard = Keyboard.GetState();
            Vector2 input = Vector2.Zero;

            if (keyboard.IsKeyDown(Keys.W)) input.Y -= 1;
            if (keyboard.IsKeyDown(Keys.S)) input.Y += 1;
            if (keyboard.IsKeyDown(Keys.A)) input.X -= 1;
            if (keyboard.IsKeyDown(Keys.D)) input.X += 1;

            if (input.Length() > 0)
                input.Normalize();

            velocity.Value = input * PlayerSpeed;
        }
    }
}