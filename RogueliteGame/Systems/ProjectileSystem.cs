using DefaultEcs;
using DefaultEcs.System;
using RogueliteGame.Components;
using System.Collections.Generic;
using EcsWorld = DefaultEcs.World;

namespace RogueliteGame.Systems
{
    public class ProjectileSystem : AEntitySetSystem<float>
    {
        private List<Entity> entitiesToDestroy = new List<Entity>();

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
            
            // Mark for destruction if lifetime expired
            if (projectile.Lifetime <= 0f)
            {
                entitiesToDestroy.Add(entity);
            }
        }

        protected override void PostUpdate(float state)
        {
            // Clean up marked entities AFTER iteration
            foreach (var entity in entitiesToDestroy)
            {
                if (entity.IsAlive)
                {
                    entity.Dispose();
                }
            }
            entitiesToDestroy.Clear();
            
            base.PostUpdate(state);
        }
    }
}