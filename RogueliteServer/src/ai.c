#include "ai.h"
#include <stdio.h>
#include <stdlib.h>
#include <math.h>

#define CHASE_RANGE 300.0f      // Start chasing if player within 300 pixels
#define ATTACK_RANGE 200.0f     // Attack if player within 200 pixels
#define ATTACK_COOLDOWN 1.0f    // 1 second between attacks
#define WANDER_SPEED 50.0f      // Wander movement speed
#define CHASE_SPEED 80.0f       // Chase movement speed
#define PROJECTILE_SPEED 200.0f // Projectile speed

// Update single enemy AI
void ai_update_enemy(Entity* enemy, Entity* player, float delta_time, EntityManager* em) {
    if (!enemy->active || !player->active) return;
    
    // Calculate distance to player
    float dist = vector2_distance(enemy->position, player->position);
    
    // Update timers
    enemy->ai.state_timer += delta_time;
    if (enemy->ai.attack_cooldown > 0.0f) {
        enemy->ai.attack_cooldown -= delta_time;
    }
    
    // State machine
    switch (enemy->ai.state) {
        case AI_STATE_IDLE:
            // Transition to wander after 1 second
            if (enemy->ai.state_timer > 1.0f) {
                enemy->ai.state = AI_STATE_WANDER;
                enemy->ai.state_timer = 0.0f;
                
                // Pick random wander target
                float angle = (float)rand() / RAND_MAX * 6.28f;  // Random angle
                float distance = 50.0f + (float)rand() / RAND_MAX * 100.0f;
                enemy->ai.wander_target.x = enemy->position.x + cosf(angle) * distance;
                enemy->ai.wander_target.y = enemy->position.y + sinf(angle) * distance;
            }
            break;
            
        case AI_STATE_WANDER:
            // Check if player is nearby
            if (dist < CHASE_RANGE) {
                enemy->ai.state = AI_STATE_CHASE;
                enemy->ai.state_timer = 0.0f;
                printf("Enemy %u: CHASE!\n", enemy->id);
                break;
            }
            
            // Move toward wander target
            float wander_dist = vector2_distance(enemy->position, enemy->ai.wander_target);
            if (wander_dist > 5.0f) {
                Vector2 direction = vector2_subtract(enemy->ai.wander_target, enemy->position);
                float length = sqrtf(direction.x * direction.x + direction.y * direction.y);
                if (length > 0.0f) {
                    direction.x /= length;
                    direction.y /= length;
                    enemy->velocity = vector2_multiply(direction, WANDER_SPEED);
                }
            } else {
                // Reached target, go idle
                enemy->velocity = vector2_create(0.0f, 0.0f);
                enemy->ai.state = AI_STATE_IDLE;
                enemy->ai.state_timer = 0.0f;
            }
            break;
            
        case AI_STATE_CHASE:
            // Check if in attack range
            if (dist < ATTACK_RANGE) {
                enemy->ai.state = AI_STATE_ATTACK;
                enemy->ai.state_timer = 0.0f;
                enemy->velocity = vector2_create(0.0f, 0.0f);  // Stop moving
                printf("Enemy %u: ATTACK!\n", enemy->id);
                break;
            }
            
            // Check if player escaped
            if (dist > CHASE_RANGE + 50.0f) {
                enemy->ai.state = AI_STATE_WANDER;
                enemy->ai.state_timer = 0.0f;
                printf("Enemy %u: Lost player\n", enemy->id);
                break;
            }
            
            // Chase player
            Vector2 direction = vector2_subtract(player->position, enemy->position);
            float length = sqrtf(direction.x * direction.x + direction.y * direction.y);
            if (length > 0.0f) {
                direction.x /= length;
                direction.y /= length;
                enemy->velocity = vector2_multiply(direction, CHASE_SPEED);
            }
            break;
            
        case AI_STATE_ATTACK:
            // Check if player moved away
            if (dist > ATTACK_RANGE + 50.0f) {
                enemy->ai.state = AI_STATE_CHASE;
                enemy->ai.state_timer = 0.0f;
                printf("Enemy %u: Player escaped, chasing\n", enemy->id);
                break;
            }
            
            // Attack (shoot projectile)
            if (enemy->ai.attack_cooldown <= 0.0f) {
                // Calculate direction to player
                Vector2 direction = vector2_subtract(player->position, enemy->position);
                float length = sqrtf(direction.x * direction.x + direction.y * direction.y);
                if (length > 0.0f) {
                    direction.x /= length;
                    direction.y /= length;
                }
                
                // Create projectile
                Vector2 projectile_pos = enemy->position;
                Entity* projectile = entity_create(em, ENTITY_TYPE_PROJECTILE, projectile_pos);
                if (projectile) {
                    projectile->velocity = vector2_multiply(direction, PROJECTILE_SPEED);
                    printf("Enemy %u fired projectile!\n", enemy->id);
                }
                
                // Reset cooldown
                enemy->ai.attack_cooldown = ATTACK_COOLDOWN;
            }
            break;
    }
}

// Update all enemies
void ai_update_all(EntityManager* em, float delta_time) {
    // Find player
    Entity* player = NULL;
    for (size_t i = 0; i < em->count; i++) {
        if (em->entities[i].type == ENTITY_TYPE_PLAYER && em->entities[i].active) {
            player = &em->entities[i];
            break;
        }
    }
    
    if (!player) return;  // No player, no AI
    
    // Update each enemy
    for (size_t i = 0; i < em->count; i++) {
        Entity* enemy = &em->entities[i];
        
        if (enemy->type == ENTITY_TYPE_ENEMY && enemy->active) {
            ai_update_enemy(enemy, player, delta_time, em);
        }
    }
}