#include "game_loop.h"
#include "collision.h"
#include "ai.h"
#include <stdio.h>
#include <time.h>
#include <math.h>   

#ifdef _WIN32
#include <windows.h>
#define sleep_ms(ms) Sleep(ms)
#else
#include <unistd.h>
#define sleep_ms(ms) usleep((ms) * 1000)
#endif

#define TICK_RATE 60.0f
#define TICK_TIME (1.0f / TICK_RATE)

// Initialize game
void game_init(GameState* game) {
    printf("=== INITIALIZING GAME ===\n");
    
    entity_manager_init(&game->entity_manager, 100);
    game->running = true;
    game->total_time = 0.0f;
    game->tick_count = 0;
    
    // Create player
    Vector2 player_pos = vector2_create(400.0f, 300.0f);
    entity_create(&game->entity_manager, ENTITY_TYPE_PLAYER, player_pos);
    printf("Player created at (%.0f, %.0f)\n", player_pos.x, player_pos.y);
    
    // Create 3 enemies
    for (int i = 0; i < 3; i++) {
        float angle = (i / 3.0f) * 6.28f;
        float x = 400.0f + cosf(angle) * 200.0f;
        float y = 300.0f + sinf(angle) * 200.0f;
        
        Vector2 enemy_pos = vector2_create(x, y);
        entity_create(&game->entity_manager, ENTITY_TYPE_ENEMY, enemy_pos);
    }
    
    printf("=== GAME INITIALIZED ===\n\n");
}

// Run game loop
void game_run(GameState* game, float duration_seconds) {
    printf("=== STARTING GAME LOOP (%.1fs at %d Hz) ===\n\n", 
           duration_seconds, (int)TICK_RATE);
    
    int total_ticks = (int)(duration_seconds * TICK_RATE);
    
    for (int tick = 0; tick < total_ticks && game->running; tick++) {
        game->tick_count++;
        game->total_time += TICK_TIME;
        
        // Update AI
        ai_update_all(&game->entity_manager, TICK_TIME);
        
        // Update movement
        entity_update_all(&game->entity_manager, TICK_TIME);
        
        // Check collisions
        collision_resolve_all(&game->entity_manager);
        
        // Print state every 60 ticks (once per second)
        if (tick % 60 == 0) {
            printf("=== TICK %d (%.1fs) ===\n", game->tick_count, game->total_time);
            entity_print_all(&game->entity_manager);
        }
        
        // Check win/lose conditions
        bool player_alive = false;
        bool enemies_alive = false;
        
        for (size_t i = 0; i < game->entity_manager.count; i++) {
            Entity* e = &game->entity_manager.entities[i];
            if (e->type == ENTITY_TYPE_PLAYER && e->active) player_alive = true;
            if (e->type == ENTITY_TYPE_ENEMY && e->active) enemies_alive = true;
        }
        
        if (!player_alive) {
            printf("\n=== GAME OVER: Player Died ===\n");
            game->running = false;
            break;
        }
        
        if (!enemies_alive) {
            printf("\n=== VICTORY: All Enemies Defeated! ===\n");
            game->running = false;
            break;
        }
        
        // Sleep to maintain 60 Hz (16.67ms per tick)
        sleep_ms(16);
    }
    
    printf("\n=== GAME LOOP ENDED ===\n");
    printf("Total ticks: %d\n", game->tick_count);
    printf("Total time: %.2fs\n", game->total_time);
}

// Cleanup
void game_cleanup(GameState* game) {
    entity_manager_free(&game->entity_manager);
    printf("=== GAME CLEANUP COMPLETE ===\n");
}