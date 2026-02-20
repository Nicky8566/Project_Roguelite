#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include "game_loop.h"
#include "network.h"

#define SERVER_PORT 12345

int main() {
    srand((unsigned int)time(NULL));
    
    printf("\n");
    printf("╔════════════════════════════════════════╗\n");
    printf("║   ROGUELITE NETWORKED SERVER           ║\n");
    printf("║   Week 6 Complete                      ║\n");
    printf("╚════════════════════════════════════════╝\n");
    printf("\n");
    
    // Initialize network
    SOCKET sock = network_init(SERVER_PORT);
    if (sock == INVALID_SOCKET) {
        printf("Failed to initialize network\n");
        return 1;
    }
    
    // Initialize game
    GameState game;
    game_init(&game, sock);
    
    // Run game loop (infinite)
    game_run_networked(&game);
    
    // Cleanup
    game_cleanup(&game);
    network_cleanup(sock);
    
    return 0;
}