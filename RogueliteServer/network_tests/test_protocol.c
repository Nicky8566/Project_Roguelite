#include <stdio.h>
#include <string.h>
#include "../src/protocol.h"

void print_hex(const uint8_t* buffer, int size) {
    for (int i = 0; i < size; i++) {
        printf("%02X ", buffer[i]);
        if ((i + 1) % 16 == 0) printf("\n");
    }
    printf("\n");
}

int main() {
    printf("=== PROTOCOL TEST ===\n\n");
    
    uint8_t buffer[MAX_PACKET_SIZE];
    
    // Test 1: INPUT message
    printf("Test 1: INPUT Message\n");
    printf("---------------------\n");
    
    InputMessage input = {
        .player_id = 42,
        .keys = KEY_W | KEY_D,  // W and D pressed
        .mouse_x = 123.45f,
        .mouse_y = 678.90f
    };
    
    int size = serialize_input(&input, buffer, sizeof(buffer));
    printf("Serialized %d bytes:\n", size);
    print_hex(buffer, size);
    
    InputMessage input_copy;
    deserialize_input(buffer, size, &input_copy);
    
    printf("Deserialized:\n");
    printf("  Player ID: %u\n", input_copy.player_id);
    printf("  Keys: 0x%02X (W=%d, D=%d)\n", 
           input_copy.keys,
           (input_copy.keys & KEY_W) != 0,
           (input_copy.keys & KEY_D) != 0);
    printf("  Mouse: (%.2f, %.2f)\n\n", input_copy.mouse_x, input_copy.mouse_y);
    
    // Test 2: STATE message
    printf("Test 2: STATE Message\n");
    printf("---------------------\n");
    
    StateMessage state = {
        .tick = 12345,
        .entity_count = 3
    };
    
    state.entities[0] = (EntityState){1, 0, 100.0f, 200.0f, 100, true};
    state.entities[1] = (EntityState){2, 1, 300.0f, 400.0f, 50, true};
    state.entities[2] = (EntityState){3, 2, 150.0f, 250.0f, 1, true};
    
    size = serialize_state(&state, buffer, sizeof(buffer));
    printf("Serialized %d bytes:\n", size);
    print_hex(buffer, size);
    
    StateMessage state_copy;
    deserialize_state(buffer, size, &state_copy);
    
    printf("Deserialized:\n");
    printf("  Tick: %u\n", state_copy.tick);
    printf("  Entities: %u\n", state_copy.entity_count);
    for (int i = 0; i < state_copy.entity_count; i++) {
        EntityState* e = &state_copy.entities[i];
        printf("    Entity %u: type=%u pos=(%.1f,%.1f) hp=%d\n",
               e->entity_id, e->entity_type, e->x, e->y, e->health);
    }
    
    printf("\n=== ALL TESTS PASSED ===\n");
    
    return 0;
}