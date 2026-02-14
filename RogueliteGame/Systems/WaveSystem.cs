using DefaultEcs;
using Microsoft.Xna.Framework;
using RogueliteGame.Components;
using RogueliteGame.World;
using System;

namespace RogueliteGame.Systems
{
    public class WaveSystem
    {
        private DefaultEcs.World world;
        private Dungeon dungeon;
        private Random rng;

        public WaveSystem(DefaultEcs.World world, Dungeon dungeon, int seed)
        {
            this.world = world;
            this.dungeon = dungeon;
            this.rng = new Random(seed);
        }

        public void StartWave(ref WaveComponent wave)
        {
            wave.CurrentWave++;
            
            // Each wave spawns more enemies
            int enemiesToSpawn = 5 + (wave.CurrentWave * 3);
            
            Console.WriteLine($"\n=== WAVE {wave.CurrentWave} STARTING ===");
            Console.WriteLine($"Spawning {enemiesToSpawn} enemies");

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                Entity enemy = world.CreateEntity();
                Vector2 enemyPos = dungeon.GetRandomFloorPosition(rng);
                
                enemy.Set(new Transform { Position = enemyPos });
                enemy.Set(new Velocity { Value = Vector2.Zero });
                
                // Enemies get tougher each wave
                int enemyHealth = 30 + (wave.CurrentWave * 10);
                enemy.Set(new Health { Current = enemyHealth, Max = enemyHealth });
                
                enemy.Set(new AIState
                {
                    State = EnemyState.Wander,
                    AttackCooldown = 0f,
                    WanderTarget = enemyPos,
                    WanderTimer = 2f
                });
            }

            wave.EnemiesRemaining = enemiesToSpawn;
            wave.WaveDelay = 0f;
        }

        public void Update(float deltaTime, ref WaveComponent wave)
        {
            // Count living enemies
            int livingEnemies = 0;
            foreach (var entity in world.GetEntities().With<AIState>().AsEnumerable())
            {
                livingEnemies++;
            }

            wave.EnemiesRemaining = livingEnemies;

            // If all enemies dead, start countdown to next wave
            if (livingEnemies == 0 && wave.WaveDelay <= 0f)
            {
                wave.WaveDelay = 3f; // 3 second delay between waves
            }

            // Countdown wave delay
            if (wave.WaveDelay > 0f)
            {
                wave.WaveDelay -= deltaTime;
                
                if (wave.WaveDelay <= 0f)
                {
                    StartWave(ref wave);
                }
            }
        }
    }
}