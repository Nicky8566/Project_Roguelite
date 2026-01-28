using DefaultEcs;
using DefaultEcs.System;
using RogueliteGame.Components;
using EcsWorld = DefaultEcs.World;

namespace RogueliteGame.Systems
{
    public class ProjectileSystem : AEntitySetSystem<float>
    {
        public ProjectileSystem(EcsWorld world) 
            : base(world.GetEntities()
                .With<Projectile>()
                .With<Velocity>()
                .AsSet())
        { }

        protected override void Update(float deltaTime, in Entity entity)
        {
            ref Projectile projectile = ref entity.Get<Projectile>();
            
            // Decrease lifetime
            projectile.Lifetime -= deltaTime;
            
            // Destroy bullet if lifetime expired
            if (projectile.Lifetime <= 0f)
            {
                entity.Dispose(); // Remove from world
            }
        }
    }
}