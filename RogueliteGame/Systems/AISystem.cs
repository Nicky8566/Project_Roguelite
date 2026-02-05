using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using RogueliteGame.Components;
using System;
using EcsWorld = DefaultEcs.World;

namespace RogueliteGame.Systems
{
    public class AISystem : AEntitySetSystem<float>
    {
        private const float ChaseDistance = 300f;
        private const float AttackDistance = 200f;
        private const float EnemySpeed = 100f;
        private const float BulletSpeed = 300f;
        private const float AttackCooldownTime = 1.5f;
        private const float WanderChangeTime = 2.0f;

        private EcsWorld world;
        private Vector2 playerPosition;
        private Random random = new Random();

        public AISystem(EcsWorld world) 
            : base(world.GetEntities()
                .With<AIState>()
                .With<Transform>()
                .With<Velocity>()
                .Without<PlayerTag>()
                .AsSet())
        {
            this.world = world;
        }

        protected override void PreUpdate(float state)
        {
            // Find player position before updating enemies
            UpdatePlayerPosition();
            base.PreUpdate(state);
        }

        private void UpdatePlayerPosition()
        {
            foreach (var entity in world.GetEntities().With<PlayerTag>().With<Transform>().AsEnumerable())
            {
                playerPosition = entity.Get<Transform>().Position;
                break;
            }
        }

        protected override void Update(float deltaTime, in Entity entity)
        {
            System.Console.WriteLine($"AI Update: State={entity.Get<AIState>().State}"); 

            ref AIState ai = ref entity.Get<AIState>();
            ref Transform transform = ref entity.Get<Transform>();
            ref Velocity velocity = ref entity.Get<Velocity>();

            // Decrease attack cooldown
            if (ai.AttackCooldown > 0)
                ai.AttackCooldown -= deltaTime;

            // Decrease wander timer
            ai.WanderTimer -= deltaTime;

            // Get distance to player
            Vector2 enemyCenter = transform.Position + new Vector2(16, 16);
            float distanceToPlayer = Vector2.Distance(enemyCenter, playerPosition + new Vector2(16, 16));

            // State machine
            switch (ai.State)
            {
                case EnemyState.Wander:
                    HandleWander(ref ai, ref velocity, transform.Position, deltaTime);
                    
                    // Transition to Chase if player is nearby
                    if (distanceToPlayer < ChaseDistance)
                    {
                        ai.State = EnemyState.Chase;
                        System.Console.WriteLine("Enemy spotted player! Chasing...");
                    }
                    break;

                case EnemyState.Chase:
                    HandleChase(ref velocity, enemyCenter);
                    
                    // Transition to Attack if close enough
                    if (distanceToPlayer < AttackDistance)
                    {
                        ai.State = EnemyState.Attack;
                        System.Console.WriteLine("Enemy in attack range!");
                    }
                    // Transition back to Wander if player too far
                    else if (distanceToPlayer > ChaseDistance * 1.5f)
                    {
                        ai.State = EnemyState.Wander;
                        System.Console.WriteLine("Lost sight of player...");
                    }
                    break;

                case EnemyState.Attack:
                    HandleAttack(ref ai, enemyCenter);
                    velocity.Value = Vector2.Zero; // Stop moving while attacking
                    
                    // Transition back to Chase if player moves away
                    if (distanceToPlayer > AttackDistance * 1.2f)
                    {
                        ai.State = EnemyState.Chase;
                    }
                    break;
            }
            System.Console.WriteLine($"AI set velocity to: {velocity.Value}");
        }

        private void HandleWander(ref AIState ai, ref Velocity velocity, Vector2 position, float deltaTime)
        {
            // Pick new wander target periodically
            if (ai.WanderTimer <= 0)
            {
                ai.WanderTarget = position + new Vector2(
                    random.Next(-200, 200),
                    random.Next(-200, 200)
                );
                ai.WanderTimer = WanderChangeTime;
            }

            // Move toward wander target
            Vector2 direction = ai.WanderTarget - position;
            if (direction.Length() > 10f)
            {
                direction.Normalize();
                velocity.Value = direction * (EnemySpeed * 0.5f); // Wander slower
            }
            else
            {
                velocity.Value = Vector2.Zero;
            }
        }

        private void HandleChase(ref Velocity velocity, Vector2 enemyCenter)
        {
            // Move toward player
            Vector2 direction = (playerPosition + new Vector2(16, 16)) - enemyCenter;
            if (direction.Length() > 0.1f)
            {
                direction.Normalize();
                velocity.Value = direction * EnemySpeed;
            }
        }

        private void HandleAttack(ref AIState ai, Vector2 enemyCenter)
        {
            // Shoot at player if cooldown ready
            if (ai.AttackCooldown <= 0f)
            {
                Vector2 direction = (playerPosition + new Vector2(16, 16)) - enemyCenter;
                if (direction.Length() > 0.1f)
                {
                    direction.Normalize();

                    // Create enemy bullet
                    Entity bullet = world.CreateEntity();
                    bullet.Set(new Transform 
                    { 
                        Position = enemyCenter - new Vector2(4, 4) 
                    });
                    bullet.Set(new Velocity 
                    { 
                        Value = direction * BulletSpeed 
                    });
                    bullet.Set(new Projectile 
                    { 
                        Lifetime = 3.0f,
                        Damage = 10,
                        OwnerID = 1 // Enemy-owned
                    });

                    ai.AttackCooldown = AttackCooldownTime;
                    
                    System.Console.WriteLine("Enemy fired!");
                }
            }
        }
    }
}