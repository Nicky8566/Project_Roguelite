#ifndef ENTITY_H
#define ENTITY_H

#include "vector2.h"
#include <stdbool.h>
#include <stdint.h>

// Entity types
typedef enum {
    ENTITY_TYPE_PLAYER,
    ENTITY_TYPE_ENEMY,
    ENTITY_TYPE_PROJECTILE
} EntityType;

// AI states
typedef enum {
    AI_STATE_IDLE,
    AI_STATE_WANDER,
    AI_STATE_CHASE,
    AI_STATE_ATTACK
} AIStateType;

// AI component
typedef struct {
    AIStateType state;
    float state_timer;
    float attack_cooldown;
    Vector2 wander_target;
} AIComponent;

// Single entity (UPDATED - added ai field)
typedef struct {
    uint32_t id;
    EntityType type;
    Vector2 position;
    Vector2 velocity;
    int health;
    int max_health;
    bool active;
    AIComponent ai;          // ← NEW: AI data
} Entity;

// Entity manager
typedef struct {
    Entity* entities;
    size_t count;
    size_t capacity;
    uint32_t next_id;
} EntityManager;

// Function declarations
void entity_manager_init(EntityManager* em, size_t initial_capacity);
void entity_manager_free(EntityManager* em);
Entity* entity_create(EntityManager* em, EntityType type, Vector2 position);
void entity_destroy(EntityManager* em, uint32_t id);
Entity* entity_get_by_id(EntityManager* em, uint32_t id);
void entity_update_all(EntityManager* em, float delta_time);
void entity_print_all(EntityManager* em);

#endif