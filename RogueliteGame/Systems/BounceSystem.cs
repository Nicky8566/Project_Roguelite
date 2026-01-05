using DefaultEcs;
using DefaultEcs.System;
using RogueliteGame.Components;
using EcsWorld = DefaultEcs.World;

namespace RogueliteGame.Systems
{
    public class BounceSystem : AEntitySetSystem<float>
    {
        private const int ScreenWidth = 800;
        private const int ScreenHeight = 600;
        private const int EntitySize = 32;

        public BounceSystem(EcsWorld world) 
            : base(world.GetEntities()
                .With<Transform>()
                .With<Velocity>()
                .AsSet())
        { }

        protected override void Update(float deltaTime, in Entity entity)
        {
            ref Transform transform = ref entity.Get<Transform>();
            ref Velocity velocity = ref entity.Get<Velocity>();

            if (transform.Position.X < 0 || transform.Position.X > ScreenWidth - EntitySize)
            {
                velocity.Value.X *= -1;
            }

            if (transform.Position.Y < 0 || transform.Position.Y > ScreenHeight - EntitySize)
            {
                velocity.Value.Y *= -1;
            }

            transform.Position.X = System.Math.Clamp(transform.Position.X, 0, ScreenWidth - EntitySize);
            transform.Position.Y = System.Math.Clamp(transform.Position.Y, 0, ScreenHeight - EntitySize);
        }
    }
}