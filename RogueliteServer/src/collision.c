#include "collision.h"
#include <stdio.h>

// Entity sizes (in pixels)
#define PLAYER_SIZE 32.0f
#define ENEMY_SIZE 32.0f
#define PROJECTILE_SIZE 8.0f

// Get bounding box for entity
BoundingBox collision_get_bounds(Entity* e) {
    BoundingBox box;
    box.x = e->position.x;
    box.y = e->position.y;
    
    // Set size based on type
    switch (e->type) {
        case ENTITY_TYPE_PLAYER:
            box.width = PLAYER_SIZE;
            box.height = PLAYER_SIZE;
            break;
        case ENTITY_TYPE_ENEMY:
            box.width = ENEMY_SIZE;
            box.height = ENEMY_SIZE;
            break;
        case ENTITY_TYPE_PROJECTILE:
            box.width = PROJECTILE_SIZE;
            box.height = PROJECTILE_SIZE;
            break;
        default:
            box.width = 32.0f;
            box.height = 32.0f;
    }
    
    return box;
}

// AABB collision detection
bool collision_check_aabb(BoundingBox a, BoundingBox b) {
    // Check if boxes overlap on both axes
    bool x_overlap = a.x < b.x + b.width && a.x + a.width > b.x;
    bool y_overlap = a.y < b.y + b.height && a.y + a.height > b.y;
    
    return x_overlap && y_overlap;
}

// Check collision between two entities
bool collision_check_entities(Entity* a, Entity* b) {
    if (!a->active || !b->active) return false;
    
    BoundingBox box_a = collision_get_bounds(a);
    BoundingBox box_b = collision_get_bounds(b);
    
    return collision_check_aabb(box_a, box_b);
}

// Resolve all collisions
void collision_resolve_all(EntityManager* em) {
    // Check projectile vs enemy collisions
    for (size_t i = 0; i < em->count; i++) {
        Entity* projectile = &em->entities[i];
        
        if (projectile->type != ENTITY_TYPE_PROJECTILE || !projectile->active) {
            continue;
        }
        
        for (size_t j = 0; j < em->count; j++) {
            if (i == j) continue;
            
            Entity* enemy = &em->entities[j];
            
            if (enemy->type != ENTITY_TYPE_ENEMY || !enemy->active) {
                continue;
            }
            
            if (collision_check_entities(projectile, enemy)) {
                // Hit!
                enemy->health -= 10;
                projectile->active = false;
                
                printf("Projectile %u hit Enemy %u! HP: %d\n", 
                       projectile->id, enemy->id, enemy->health);
                
                // Kill enemy if health depleted
                if (enemy->health <= 0) {
                    printf("Enemy %u destroyed!\n", enemy->id);
                    enemy->active = false;
                }
                
                break;  // Projectile can only hit one enemy
            }
        }
    }
    
    // Remove inactive entities
    for (size_t i = 0; i < em->count; ) {
        if (!em->entities[i].active) {
            // Swap with last and decrease count
            em->entities[i] = em->entities[em->count - 1];
            em->count--;
        } else {
            i++;
        }
    }
}