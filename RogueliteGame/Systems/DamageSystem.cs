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

            // Get player
            Entity? playerEntity = null;
            Transform playerTransform = default;

            foreach (var p in world.GetEntities().With<PlayerTag>().With<Health>().With<Transform>().AsEnumerable())
            {
                playerEntity = p;
                playerTransform = p.Get<Transform>();
                break;
            }

            // Check each bullet
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

                // PLAYER BULLETS (OwnerID = 0) hit ENEMIES
                if (projectile.OwnerID == 0)
                {
                    foreach (var enemy in enemies)
                    {
                        if (!enemy.IsAlive) continue;

                        ref Transform enemyTransform = ref enemy.Get<Transform>();
                        ref Health enemyHealth = ref enemy.Get<Health>();

                        Rectangle enemyRect = new Rectangle(
                            (int)enemyTransform.Position.X,
                            (int)enemyTransform.Position.Y,
                            32, 32
                        );

                        if (bulletRect.Intersects(enemyRect))
                        {
                            enemyHealth.Current -= projectile.Damage;
                            System.Console.WriteLine($"Player bullet HIT enemy! Health: {enemyHealth.Current}/{enemyHealth.Max}");

                            entitiesToDestroy.Add(bullet);

                            if (enemyHealth.Current <= 0)
                            {
                                System.Console.WriteLine("Enemy destroyed!");
                                entitiesToDestroy.Add(enemy);
                            }

                            break;
                        }
                    }
                }
                // ENEMY BULLETS (OwnerID = 1) hit PLAYER
                else if (projectile.OwnerID == 1)
                {
                    if (playerEntity.HasValue && playerEntity.Value.IsAlive)
                    {
                        Rectangle playerRect = new Rectangle(
                            (int)playerTransform.Position.X,
                            (int)playerTransform.Position.Y,
                            32, 32
                        );

                        if (bulletRect.Intersects(playerRect))
                        {
                            ref Health playerHealth = ref playerEntity.Value.Get<Health>();
                            playerHealth.Current -= projectile.Damage;

                            System.Console.WriteLine($"Enemy bullet HIT player! Health: {playerHealth.Current}/{playerHealth.Max}");

                            entitiesToDestroy.Add(bullet);

                            if (playerHealth.Current <= 0)
                            {
                                System.Console.WriteLine("=== PLAYER DEAD! GAME OVER! ===");
                            }
                        }
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