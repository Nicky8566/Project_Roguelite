#include <stdio.h>
#include "entity.h"

int main() {
    printf("=== Entity System Test ===\n\n");
    
    // Create entity manager
    EntityManager em;
    entity_manager_init(&em, 4);  // Start with capacity 4
    
    // Create some entities
    Vector2 pos1 = vector2_create(100.0f, 200.0f);
    Entity* player = entity_create(&em, ENTITY_TYPE_PLAYER, pos1);
    
    Vector2 pos2 = vector2_create(500.0f, 300.0f);
    Entity* enemy1 = entity_create(&em, ENTITY_TYPE_ENEMY, pos2);
    
    Vector2 pos3 = vector2_create(550.0f, 350.0f);
    Entity* enemy2 = entity_create(&em, ENTITY_TYPE_ENEMY, pos3);
    
    // Print all entities
    entity_print_all(&em);
    
    // Give player velocity
    player->velocity = vector2_create(5.0f, 0.0f);  // Move right
    enemy1->velocity = vector2_create(-2.0f, 1.0f); // Move left-down
    
    // Simulate 3 frames (60 FPS = 0.016s per frame)
    printf("Simulating movement...\n\n");
    for (int frame = 1; frame <= 3; frame++) {
        printf("=== FRAME %d ===\n", frame);
        entity_update_all(&em, 0.016f);  // 16ms = 60 FPS
        entity_print_all(&em);
    }
    
    // Test growing array (capacity is 4, add 5th entity)
    printf("Adding 2 more entities (should trigger resize)...\n");
    Vector2 pos4 = vector2_create(200.0f, 200.0f);
    entity_create(&em, ENTITY_TYPE_PROJECTILE, pos4);
    
    Vector2 pos5 = vector2_create(300.0f, 300.0f);
    entity_create(&em, ENTITY_TYPE_ENEMY, pos5);
    
    entity_print_all(&em);
    
    // Test entity deletion
    printf("Destroying enemy ID 2...\n");
    entity_destroy(&em, 2);
    entity_print_all(&em);
    
    // Clean up
    entity_manager_free(&em);
    
    printf("=== Test Complete ===\n");
    
    return 0;
}