#include "game_loop.h"
#include "network.h"
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
#define CLIENT_TIMEOUT 5.0f  // Disconnect after 5 seconds of no packets

void game_init(GameState* game, SOCKET sock) {
    printf("=== INITIALIZING NETWORKED GAME ===\n");
    
    entity_manager_init(&game->entity_manager, 100);
    game->running = true;
    game->total_time = 0.0f;
    game->tick_count = 0;
    game->socket = sock;
    game->client_count = 0;
    
    // Initialize clients
    for (int i = 0; i < MAX_CLIENTS; i++) {
        game->clients[i].connected = false;
        game->clients[i].player_id = 0;
        game->clients[i].last_packet_time = 0.0f;
    }
    
    // Spawn 3 AI enemies
    for (int i = 0; i < 3; i++) {
        float angle = (i / 3.0f) * 6.28f;
        float x = 400.0f + cosf(angle) * 200.0f;
        float y = 300.0f + sinf(angle) * 200.0f;
        
        Vector2 enemy_pos = vector2_create(x, y);
        entity_create(&game->entity_manager, ENTITY_TYPE_ENEMY, enemy_pos);
    }
    
    printf("=== GAME INITIALIZED ===\n");
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
                    entity_destroy(&game->entity_manager, game->clients[i].player_id);
                    game->clients[i].connected = false;
                    game->client_count--;
                }
            }
        }
        
        // 3. Run game logic (same as Week 5)
        ai_update_all(&game->entity_manager, TICK_TIME);
        entity_update_all(&game->entity_manager, TICK_TIME);
        collision_resolve_all(&game->entity_manager);
        
        // 4. Broadcast state to clients
        network_broadcast_state(game);
        
        // 5. Print state every 60 ticks (1 second)
        if (game->tick_count % 60 == 0) {
            printf("=== TICK %d (%.1fs) - Clients: %d ===\n",
                   game->tick_count, game->total_time, game->client_count);
            entity_print_all(&game->entity_manager);
        }
        
        // 6. Sleep to maintain 60 Hz
        sleep_ms(16);
    }
    
    printf("\n=== GAME LOOP ENDED ===\n");
}

void game_cleanup(GameState* game) {
    entity_manager_free(&game->entity_manager);
    printf("=== GAME CLEANUP COMPLETE ===\n");
}