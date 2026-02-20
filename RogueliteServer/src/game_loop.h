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

// Client info
typedef struct {
    struct sockaddr_in addr;   // Client's IP:Port
    uint32_t player_id;        // Their entity ID
    float last_packet_time;    // Time of last packet (for timeout)
    bool connected;
} NetworkClient;

// Game state (updated)
typedef struct {
    EntityManager entity_manager;
    bool running;
    float total_time;
    int tick_count;
    
    // NEW: Network fields
    SOCKET socket;
    NetworkClient clients[MAX_CLIENTS];
    int client_count;
} GameState;

// Functions
void game_init(GameState* game, SOCKET sock);
void game_run_networked(GameState* game);
void game_cleanup(GameState* game);

#endif