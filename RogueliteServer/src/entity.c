#include "entity.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

// Initialize entity manager
void entity_manager_init(EntityManager* em, size_t initial_capacity) {
    // Allocate array on HEAP
    em->entities = malloc(initial_capacity * sizeof(Entity));
    
    if (em->entities == NULL) {
        fprintf(stderr, "Failed to allocate memory for entities!\n");
        exit(1);  // Exit if allocation fails
    }
    
    em->count = 0;
    em->capacity = initial_capacity;
    em->next_id = 1;  // Start IDs at 1 (0 = invalid)
    
    printf("EntityManager initialized with capacity %zu\n", initial_capacity);
}

// Free entity manager
void entity_manager_free(EntityManager* em) {
    free(em->entities);  // Free the array
    em->entities = NULL;
    em->count = 0;
    em->capacity = 0;
    
    printf("EntityManager freed\n");
}

// Create new entity
Entity* entity_create(EntityManager* em, EntityType type, Vector2 position) {
    // Check if we need to grow the array
    if (em->count >= em->capacity) {
        // Double the capacity
        size_t new_capacity = em->capacity * 2;
        
        printf("Growing entity array: %zu -> %zu\n", em->capacity, new_capacity);
        
        // Reallocate (resize) the array
        Entity* new_entities = realloc(em->entities, new_capacity * sizeof(Entity));
        
        if (new_entities == NULL) {
            fprintf(stderr, "Failed to grow entity array!\n");
            return NULL;
        }
        
        em->entities = new_entities;
        em->capacity = new_capacity;
    }
    
    // Get pointer to next available slot
    Entity* e = &em->entities[em->count];
    em->count++;
    
    // Initialize entity
    e->id = em->next_id++;
    e->type = type;
    e->position = position;
    e->velocity = vector2_create(0.0f, 0.0f);
    e->active = true;
    
    // Initialize AI component
    e->ai.state = AI_STATE_IDLE;
    e->ai.state_timer = 0.0f;
    e->ai.attack_cooldown = 0.0f;
    e->ai.wander_target = position;

    // Set health based on type
    if (type == ENTITY_TYPE_PLAYER) {
        e->health = 100;
        e->max_health = 100;
    } else if (type == ENTITY_TYPE_ENEMY) {
        e->health = 50;
        e->max_health = 50;
    } else {
        e->health = 1;
        e->max_health = 1;
    }
    
    printf("Created entity ID %u (type %d) at ", e->id, e->type);
    vector2_print(e->position);
    printf("\n");
    
    return e;
}

// Destroy entity by ID
void entity_destroy(EntityManager* em, uint32_t id) {
    for (size_t i = 0; i < em->count; i++) {
        if (em->entities[i].id == id) {
            printf("Destroying entity ID %u\n", id);
            
            // Swap with last entity (fast removal)
            em->entities[i] = em->entities[em->count - 1];
            em->count--;
            
            return;
        }
    }
    
    printf("Entity ID %u not found\n", id);
}

// Get entity by ID
Entity* entity_get_by_id(EntityManager* em, uint32_t id) {
    for (size_t i = 0; i < em->count; i++) {
        if (em->entities[i].id == id) {
            return &em->entities[i];
        }
    }
    return NULL;  // Not found
}

// Update all entities, added projectile cleanup:
void entity_update_all(EntityManager* em, float delta_time) {
    for (size_t i = 0; i < em->count; i++) {
        Entity* e = &em->entities[i];
        
        if (!e->active) continue;
        
        // Update position
        e->position.x += e->velocity.x * delta_time;
        e->position.y += e->velocity.y * delta_time;
        
        // NEW: Remove projectiles that go off-screen
        if (e->type == ENTITY_TYPE_PROJECTILE) {
            // If projectile is way off screen, remove it
            if (e->position.x < -1000 || e->position.x > 1800 ||
                e->position.y < -1000 || e->position.y > 1200) {
                e->active = false;
                printf("Projectile %u went off-screen, removed\n", e->id);
            }
        }
    }
}
// Print all entities
void entity_print_all(EntityManager* em) {
    printf("\n=== ENTITIES (%zu/%zu) ===\n", em->count, em->capacity);
    
    for (size_t i = 0; i < em->count; i++) {
        Entity* e = &em->entities[i];
        
        printf("ID %u [%s] at ", e->id, 
               e->type == ENTITY_TYPE_PLAYER ? "PLAYER" :
               e->type == ENTITY_TYPE_ENEMY ? "ENEMY" : "PROJECTILE");
        vector2_print(e->position);
        printf(" HP: %d/%d\n", e->health, e->max_health);
    }
    
    printf("========================\n\n");
}