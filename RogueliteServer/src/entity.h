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

// Single entity
typedef struct {
    uint32_t id;           // Unique identifier
    EntityType type;       // What kind of entity?
    Vector2 position;      // Where is it?
    Vector2 velocity;      // How fast/direction is it moving?
    int health;            // HP
    int max_health;        // Max HP
    bool active;           // Is it alive?
} Entity;

// Entity manager (manages all entities)
typedef struct {
    Entity* entities;      // Dynamic array of entities
    size_t count;          // How many entities exist
    size_t capacity;       // Max capacity before resize
    uint32_t next_id;      // ID for next entity
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