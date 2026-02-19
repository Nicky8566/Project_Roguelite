#ifndef GAME_LOOP_H
#define GAME_LOOP_H

#include "entity.h"

// Game state
typedef struct {
    EntityManager entity_manager;
    bool running;
    float total_time;
    int tick_count;
} GameState;

// Initialize game
void game_init(GameState* game);

// Run game loop (60 ticks/sec for N seconds)
void game_run(GameState* game, float duration_seconds);

// Cleanup
void game_cleanup(GameState* game);

#endif