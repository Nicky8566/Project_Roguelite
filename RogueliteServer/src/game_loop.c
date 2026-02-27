#include "game_loop.h"
#include "network.h"
#include "collision.h"
#include "ai.h"
#include <stdio.h>
#include <stdlib.h>
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
#define CLIENT_TIMEOUT 5.0f  // Disconnect after 5 seconds of no packets

// Count alive enemies
int wave_count_enemies(GameState* game) {
    int count = 0;
    for (size_t i = 0; i < game->entity_manager.count; i++) {
        Entity* e = &game->entity_manager.entities[i];
        if (e->type == ENTITY_TYPE_ENEMY && e->active) {
            count++;
        }
    }
    return count;
}

// Start a new wave
void wave_start(GameState* game) {
    game->current_wave++;
    game->wave_active = true;
    game->wave_countdown = 0.0f;
    
    // Calculate number of enemies for this wave
    int enemy_count = 3 + (game->current_wave - 1) * 2;  // 3, 5, 7, 9...
    
    printf("\n=== WAVE %d STARTING - Spawning %d enemies ===\n", 
           game->current_wave, enemy_count);
    
    // Spawn enemies in a circle, far from center
    for (int i = 0; i < enemy_count; i++) {
        float angle = (i / (float)enemy_count) * 6.28318f;  // 2*PI
        
        // Spawn 250-350 pixels from center
        float distance = 250.0f + (rand() % 100);
        float x = 400.0f + cosf(angle) * distance;
        float y = 300.0f + sinf(angle) * distance;
        
        // Clamp to map boundaries
        if (x < MAP_MIN_X + 50) x = MAP_MIN_X + 50;
        if (x > MAP_MAX_X - 50) x = MAP_MAX_X - 50;
        if (y < MAP_MIN_Y + 50) y = MAP_MIN_Y + 50;
        if (y > MAP_MAX_Y - 50) y = MAP_MAX_Y - 50;
        
        Vector2 enemy_pos = vector2_create(x, y);
        entity_create(&game->entity_manager, ENTITY_TYPE_ENEMY, enemy_pos);
    }
    
    game->enemies_alive = enemy_count;
}

// Update wave system
void wave_update(GameState* game, float delta_time) {
    int current_enemies = wave_count_enemies(game);
    
    if (game->wave_active) {
        // Check if wave is complete (all enemies dead)
        if (current_enemies == 0) {
            game->wave_active = false;
            game->wave_countdown = 5.0f;  // 5 second countdown
            printf("\n=== WAVE %d COMPLETE! Next wave in 5 seconds... ===\n", 
                   game->current_wave);
        }
    } else {
        // Countdown to next wave
        game->wave_countdown -= delta_time;
        
        if (game->wave_countdown <= 0.0f) {
            wave_start(game);
        }
    }
    
    game->enemies_alive = current_enemies;
}

void game_init(GameState* game, SOCKET sock) {
    printf("=== INITIALIZING NETWORKED GAME ===\n");
    
    // Seed random
    srand((unsigned int)time(NULL));
    
    entity_manager_init(&game->entity_manager, 100);
    game->running = true;
    game->total_time = 0.0f;
    game->tick_count = 0;
    game->socket = sock;
    game->client_count = 0;
    
    // Initialize wave system
    game->current_wave = 0;
    game->enemies_alive = 0;
    game->wave_countdown = 3.0f;  // Start first wave after 3 seconds
    game->wave_active = false;
    
    // Initialize clients
    for (int i = 0; i < MAX_CLIENTS; i++) {
        game->clients[i].connected = false;
        game->clients[i].player_id = 0;
        game->clients[i].last_packet_time = 0.0f;
        game->clients[i].kills = 0;
    }
    
    printf("=== GAME INITIALIZED ===\n");
    printf("First wave starts in 3 seconds...\n");
    printf("Waiting for clients to connect...\n\n");
}

void game_run_networked(GameState* game) {
    printf("=== STARTING NETWORKED GAME LOOP ===\n\n");
    
    while (game->running) {
        game->tick_count++;
        game->total_time += TICK_TIME;
        
        // 1. Receive inputs from clients
        network_receive_packets(game, TICK_TIME);
        
        // 2. Check for client timeouts
        for (int i = 0; i < MAX_CLIENTS; i++) {
            if (game->clients[i].connected) {
                float time_since_packet = game->total_time - game->clients[i].last_packet_time;
                if (time_since_packet > CLIENT_TIMEOUT) {
                    printf("Client %d timed out\n", i);
                    Entity* player = entity_get_by_id(&game->entity_manager, 
                                                      game->clients[i].player_id);
                    if (player) {
                        player->active = false;
                    }
                    game->clients[i].connected = false;
                    game->client_count--;
                }
            }
        }
        
        // 3. Update wave system
        wave_update(game, TICK_TIME);
        
        // 4. Run game logic
        ai_update_all(&game->entity_manager, TICK_TIME);
        entity_update_all(&game->entity_manager, TICK_TIME);
        
        // 5. Apply map boundaries to all entities
        for (size_t i = 0; i < game->entity_manager.count; i++) {
            Entity* e = &game->entity_manager.entities[i];
            if (!e->active) continue;
            
            // Clamp position to map bounds
            if (e->position.x < MAP_MIN_X) {
                e->position.x = MAP_MIN_X;
                e->velocity.x = 0;
            }
            if (e->position.x > MAP_MAX_X) {
                e->position.x = MAP_MAX_X;
                e->velocity.x = 0;
            }
            if (e->position.y < MAP_MIN_Y) {
                e->position.y = MAP_MIN_Y;
                e->velocity.y = 0;
            }
            if (e->position.y > MAP_MAX_Y) {
                e->position.y = MAP_MAX_Y;
                e->velocity.y = 0;
            }
        }
        
        // 6. Collision detection
        collision_resolve_all(game);
        
        // 7. Broadcast state to clients
        network_broadcast_state(game);
        
        // 8. Print state every 60 ticks (1 second)
        if (game->tick_count % 60 == 0) {
            printf("=== TICK %d (%.1fs) - Clients: %d - Wave: %d - Enemies: %d ===\n",
                   game->tick_count, game->total_time, game->client_count, 
                   game->current_wave, game->enemies_alive);
            
            // Print kill counts
            for (int i = 0; i < MAX_CLIENTS; i++) {
                if (game->clients[i].connected) {
                    printf("  Client %d: %d kills\n", i, game->clients[i].kills);
                }
            }
        }
        
        // 9. Sleep to maintain 60 Hz
        sleep_ms(16);
    }
    
    printf("\n=== GAME LOOP ENDED ===\n");
}

void game_cleanup(GameState* game) {
    entity_manager_free(&game->entity_manager);
    printf("=== GAME CLEANUP COMPLETE ===\n");
}
