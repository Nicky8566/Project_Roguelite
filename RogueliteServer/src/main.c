#include <stdio.h>
#include <stdlib.h>

typedef struct {
    float x, y;
    int health;
} Entity;

int main() {
    printf("=== C Server Test ===\n\n");
    
    // Stack entity
    Entity player;
    player.x = 100.0f;
    player.y = 200.0f;
    player.health = 100;
    
    printf("Stack Entity:\n");
    printf("  Player at (%.1f, %.1f) HP: %d\n", player.x, player.y, player.health);
    
    // Heap entity
    Entity* enemy = malloc(sizeof(Entity));
    enemy->x = 500.0f;
    enemy->y = 300.0f;
    enemy->health = 50;
    
    printf("\nHeap Entity:\n");
    printf("  Enemy at (%.1f, %.1f) HP: %d\n", enemy->x, enemy->y, enemy->health);
    
    // Simulate movement
    player.x += 5.0f;
    enemy->x -= 3.0f;
    
    printf("\nAfter movement:\n");
    printf("  Player at (%.1f, %.1f) HP: %d\n", player.x, player.y, player.health);
    printf("  Enemy at (%.1f, %.1f) HP: %d\n", enemy->x, enemy->y, enemy->health);
    
    // Clean up heap memory
    free(enemy);
    printf("\n=== Memory freed successfully ===\n");
    
    return 0;
}
