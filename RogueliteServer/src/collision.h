#ifndef COLLISION_H
#define COLLISION_H

#include "entity.h"
#include <stdbool.h>

// AABB (Axis-Aligned Bounding Box) collision
typedef struct {
    float x, y;      // Top-left corner
    float width;     // Width
    float height;    // Height
} BoundingBox;

// Get bounding box for entity
BoundingBox collision_get_bounds(Entity* e);

// Check if two boxes overlap
bool collision_check_aabb(BoundingBox a, BoundingBox b);

// Check collision between two entities
bool collision_check_entities(Entity* a, Entity* b);

// Check and resolve all collisions in manager
void collision_resolve_all(EntityManager* em);

#endif