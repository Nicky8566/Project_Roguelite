using DefaultEcs;
using DefaultEcs.System;
using RogueliteGame.Components;

namespace RogueliteGame.Systems
{
    public class MovementSystem : AEntitySetSystem<float>
    {
        public MovementSystem(World world) 
            : base(world.GetEntities()
                .With<Transform>()
                .With<Velocity>()
                .AsSet())
        { }

        protected override void Update(float deltaTime, in Entity entity)
        {
            ref Transform transform = ref entity.Get<Transform>();
            ref Velocity velocity = ref entity.Get<Velocity>();
            
            transform.Position += velocity.Value * deltaTime;
        }
    }
}
