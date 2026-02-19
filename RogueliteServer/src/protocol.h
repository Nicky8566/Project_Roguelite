#ifndef PROTOCOL_H
#define PROTOCOL_H

#include <stdint.h>
#include <stdbool.h>

// Message types
typedef enum {
    MSG_CONNECT = 0,      // Client → Server: Join game
    MSG_DISCONNECT = 1,   // Client → Server: Leave game
    MSG_INPUT = 2,        // Client → Server: Player input
    MSG_STATE = 3,        // Server → Client: Game state
    MSG_PING = 4,         // Bidirectional: Keep-alive
    MSG_PONG = 5          // Response to ping
} MessageType;

// Input keys (bitflags)
#define KEY_W     0x01  // 0000 0001
#define KEY_A     0x02  // 0000 0010
#define KEY_S     0x04  // 0000 0100
#define KEY_D     0x08  // 0000 1000
#define KEY_SPACE 0x10  // 0001 0000

// Maximum packet size
#define MAX_PACKET_SIZE 1024

// Connect message (client → server)
typedef struct {
    char player_name[32];  // Player nickname
} ConnectMessage;

// Input message (client → server)
typedef struct {
    uint32_t player_id;    // Which player
    uint8_t keys;          // Which keys pressed (bitflags)
    float mouse_x;         // Mouse position (for aiming)
    float mouse_y;
} InputMessage;

// Entity state (used in StateMessage)
typedef struct {
    uint32_t entity_id;
    uint8_t entity_type;   // 0=player, 1=enemy, 2=projectile
    float x, y;            // Position
    int16_t health;        // HP
    bool active;
} EntityState;

// State message (server → client)
typedef struct {
    uint32_t tick;         // Server tick number
    uint8_t entity_count;  // How many entities
    EntityState entities[32];  // Max 32 entities
} StateMessage;

// Serialization functions
int serialize_connect(const ConnectMessage* msg, uint8_t* buffer, int buffer_size);
int deserialize_connect(const uint8_t* buffer, int buffer_size, ConnectMessage* msg);

int serialize_input(const InputMessage* msg, uint8_t* buffer, int buffer_size);
int deserialize_input(const uint8_t* buffer, int buffer_size, InputMessage* msg);

int serialize_state(const StateMessage* msg, uint8_t* buffer, int buffer_size);
int deserialize_state(const uint8_t* buffer, int buffer_size, StateMessage* msg);

#endif