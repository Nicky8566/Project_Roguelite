#ifndef GAME_LOOP_H
#define GAME_LOOP_H

#include "entity.h"

#ifdef _WIN32
    #include <winsock2.h>
    typedef int socklen_t;
#else
    #include <sys/socket.h>
    #include <netinet/in.h>
    typedef int SOCKET;
    #define INVALID_SOCKET -1
#endif

// Maximum clients
#define MAX_CLIENTS 4

// Map boundaries (match client grid)
#define MAP_MIN_X -400.0f
#define MAP_MIN_Y -300.0f
#define MAP_MAX_X 1200.0f
#define MAP_MAX_Y 900.0f

// Client info
typedef struct {
    struct sockaddr_in addr;   // Client's IP:Port
    uint32_t player_id;        // Their entity ID
    float last_packet_time;    // Time of last packet (for timeout)
    bool connected;
    int kills;                 // NEW: Kill counter
} NetworkClient;

// Game state (updated)
typedef struct {
    EntityManager entity_manager;
    bool running;
    float total_time;
    int tick_count;
    
    // Network fields
    SOCKET socket;
    NetworkClient clients[MAX_CLIENTS];
    int client_count;
    
    // NEW: Wave system
    int current_wave;
    int enemies_alive;
    float wave_countdown;
    bool wave_active;
} GameState;

// Functions
void game_init(GameState* game, SOCKET sock);
void game_run_networked(GameState* game);
void game_cleanup(GameState* game);

// NEW: Wave functions
void wave_start(GameState* game);
void wave_update(GameState* game, float delta_time);
int wave_count_enemies(GameState* game);

#endif
