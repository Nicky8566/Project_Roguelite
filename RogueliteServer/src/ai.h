#ifndef AI_H
#define AI_H

#include "entity.h"

// // AI states
// typedef enum {
//     AI_STATE_IDLE,
//     AI_STATE_WANDER,
//     AI_STATE_CHASE,
//     AI_STATE_ATTACK
// } AIStateType;

// // AI component (add to entity later)
// typedef struct {
//     AIStateType state;
//     float state_timer;        // Time in current state
//     float attack_cooldown;    // Time until can attack again
//     Vector2 wander_target;    // Random wander destination
// } AIComponent;

// Update AI for a single enemy
void ai_update_enemy(Entity* enemy, Entity* player, float delta_time, EntityManager* em);

// Update AI for all enemies
void ai_update_all(EntityManager* em, float delta_time);

#endif