#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include "game_loop.h"

int main() {
    // Seed random number generator
    srand((unsigned int)time(NULL));
    
    printf("\n");
    printf("╔════════════════════════════════════════╗\n");
    printf("║   ROGUELITE SERVER - WEEK 5 COMPLETE  ║\n");
    printf("╚════════════════════════════════════════╝\n");
    printf("\n");
    
    // Create game
    GameState game;
    game_init(&game);
    
    // Run for 10 seconds
    game_run(&game, 10.0f);
    
    // Cleanup
    game_cleanup(&game);
    
    printf("\nPress Enter to exit...\n");
    getchar();
    
    return 0;
}