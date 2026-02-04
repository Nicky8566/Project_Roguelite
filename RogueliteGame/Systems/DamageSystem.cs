using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using RogueliteGame.Components;
using System.Collections.Generic;
using EcsWorld = DefaultEcs.World;

namespace RogueliteGame.Systems
{
    public class DamageSystem : ISystem<float>
    {
        private EcsWorld world;
        private List<Entity> entitiesToDestroy = new List<Entity>();

        public DamageSystem(EcsWorld world)
        {
            this.world = world;
        }

        public bool IsEnabled { get; set; } = true;

        public void Update(float deltaTime)
        {
            if (!IsEnabled) return;

            entitiesToDestroy.Clear();

            // Get all projectiles
            var projectiles = world.GetEntities()
                .With<Projectile>()
                .With<Transform>()
                .AsEnumerable();

            // Get all enemies (entities with Health but NOT PlayerTag)
            var enemies = world.GetEntities()
                .With<Health>()
                .With<Transform>()
                .Without<PlayerTag>()
                .AsEnumerable();

            // Check each bullet against each enemy
            foreach (var bullet in projectiles)
            {
                if (!bullet.IsAlive) continue;

                ref Transform bulletTransform = ref bullet.Get<Transform>();
                ref Projectile projectile = ref bullet.Get<Projectile>();

                Rectangle bulletRect = new Rectangle(
                    (int)bulletTransform.Position.X,
                    (int)bulletTransform.Position.Y,
                    8, 8  // Bullet is 8x8
                );

                foreach (var enemy in enemies)
                {
                    if (!enemy.IsAlive) continue;

                    ref Transform enemyTransform = ref enemy.Get<Transform>();
                    ref Health enemyHealth = ref enemy.Get<Health>();

                    Rectangle enemyRect = new Rectangle(
                        (int)enemyTransform.Position.X,
                        (int)enemyTransform.Position.Y,
                        32, 32  // Enemy is 32x32
                    );

                    // Check collision
                    if (bulletRect.Intersects(enemyRect))
                    {
                        // Deal damage
                        enemyHealth.Current -= projectile.Damage;

                        System.Console.WriteLine($"HIT! Enemy health: {enemyHealth.Current}/{enemyHealth.Max}");

                        // Mark bullet for destruction
                        entitiesToDestroy.Add(bullet);

                        // If enemy is dead, mark it too
                        if (enemyHealth.Current <= 0)
                        {
                            System.Console.WriteLine("Enemy destroyed!");
                            entitiesToDestroy.Add(enemy);
                        }

                        break; // Bullet can only hit one enemy
                    }
                }
            }

            // Clean up destroyed entities
            foreach (var entity in entitiesToDestroy)
            {
                if (entity.IsAlive)
                {
                    entity.Dispose();
                }
            }
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}