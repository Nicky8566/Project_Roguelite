#include "protocol.h"
#include <string.h>

#ifdef _WIN32
    #include <winsock2.h>
#else
    #include <arpa/inet.h>
#endif

// Helper: Write uint32_t to buffer (network byte order)
static void write_uint32(uint8_t* buffer, uint32_t value) {
    uint32_t net_value = htonl(value);
    memcpy(buffer, &net_value, 4);
}

// Helper: Read uint32_t from buffer (network byte order)
static uint32_t read_uint32(const uint8_t* buffer) {
    uint32_t net_value;
    memcpy(&net_value, buffer, 4);
    return ntohl(net_value);
}

// Helper: Write float to buffer (as uint32_t, network byte order)
static void write_float(uint8_t* buffer, float value) {
    uint32_t* value_as_int = (uint32_t*)&value;
    write_uint32(buffer, *value_as_int);
}

// Helper: Read float from buffer (as uint32_t, network byte order)
static float read_float(const uint8_t* buffer) {
    uint32_t value_as_int = read_uint32(buffer);
    return *(float*)&value_as_int;
}

// Helper: Write int16_t to buffer (network byte order)
static void write_int16(uint8_t* buffer, int16_t value) {
    uint16_t net_value = htons((uint16_t)value);
    memcpy(buffer, &net_value, 2);
}

// Helper: Read int16_t from buffer (network byte order)
static int16_t read_int16(const uint8_t* buffer) {
    uint16_t net_value;
    memcpy(&net_value, buffer, 2);
    return (int16_t)ntohs(net_value);
}

// Serialize CONNECT message
int serialize_connect(const ConnectMessage* msg, uint8_t* buffer, int buffer_size) {
    if (buffer_size < 1 + 32) return -1;  // Need 33 bytes
    
    int offset = 0;
    
    // Message type
    buffer[offset++] = MSG_CONNECT;
    
    // Player name (32 bytes)
    memcpy(&buffer[offset], msg->player_name, 32);
    offset += 32;
    
    return offset;  // Total bytes written
}

// Deserialize CONNECT message
int deserialize_connect(const uint8_t* buffer, int buffer_size, ConnectMessage* msg) {
    if (buffer_size < 1 + 32) return -1;
    
    int offset = 0;
    
    // Skip message type (already read)
    offset++;
    
    // Player name
    memcpy(msg->player_name, &buffer[offset], 32);
    msg->player_name[31] = '\0';  // Ensure null-terminated
    offset += 32;
    
    return offset;
}

// Serialize INPUT message
int serialize_input(const InputMessage* msg, uint8_t* buffer, int buffer_size) {
    if (buffer_size < 1 + 4 + 1 + 4 + 4) return -1;  // Need 14 bytes
    
    int offset = 0;
    
    // Message type
    buffer[offset++] = MSG_INPUT;
    
    // Player ID (4 bytes)
    write_uint32(&buffer[offset], msg->player_id);
    offset += 4;
    
    // Keys (1 byte)
    buffer[offset++] = msg->keys;
    
    // Mouse X (4 bytes)
    write_float(&buffer[offset], msg->mouse_x);
    offset += 4;
    
    // Mouse Y (4 bytes)
    write_float(&buffer[offset], msg->mouse_y);
    offset += 4;
    
    return offset;
}

// Deserialize INPUT message
int deserialize_input(const uint8_t* buffer, int buffer_size, InputMessage* msg) {
    if (buffer_size < 1 + 4 + 1 + 4 + 4) return -1;
    
    int offset = 0;
    
    // Skip message type
    offset++;
    
    // Player ID
    msg->player_id = read_uint32(&buffer[offset]);
    offset += 4;
    
    // Keys
    msg->keys = buffer[offset++];
    
    // Mouse X
    msg->mouse_x = read_float(&buffer[offset]);
    offset += 4;
    
    // Mouse Y
    msg->mouse_y = read_float(&buffer[offset]);
    offset += 4;
    
    return offset;
}

// Serialize STATE message
int serialize_state(const StateMessage* msg, uint8_t* buffer, int buffer_size) {
    // Calculate required size
    int required = 1 + 4 + 1 + (msg->entity_count * (4 + 1 + 4 + 4 + 2 + 1));
    if (buffer_size < required) return -1;
    
    int offset = 0;
    
    // Message type
    buffer[offset++] = MSG_STATE;
    
    // Tick number (4 bytes)
    write_uint32(&buffer[offset], msg->tick);
    offset += 4;
    
    // Entity count (1 byte)
    buffer[offset++] = msg->entity_count;
    
    // Each entity
    for (int i = 0; i < msg->entity_count; i++) {
        const EntityState* e = &msg->entities[i];
        
        // Entity ID (4 bytes)
        write_uint32(&buffer[offset], e->entity_id);
        offset += 4;
        
        // Entity type (1 byte)
        buffer[offset++] = e->entity_type;
        
        // Position (8 bytes)
        write_float(&buffer[offset], e->x);
        offset += 4;
        write_float(&buffer[offset], e->y);
        offset += 4;
        
        // Health (2 bytes)
        write_int16(&buffer[offset], e->health);
        offset += 2;
        
        // Active (1 byte)
        buffer[offset++] = e->active ? 1 : 0;
    }
    
    return offset;
}

// Deserialize STATE message
int deserialize_state(const uint8_t* buffer, int buffer_size, StateMessage* msg) {
    if (buffer_size < 1 + 4 + 1) return -1;
    
    int offset = 0;
    
    // Skip message type
    offset++;
    
    // Tick number
    msg->tick = read_uint32(&buffer[offset]);
    offset += 4;
    
    // Entity count
    msg->entity_count = buffer[offset++];
    
    if (msg->entity_count > 32) return -1;  // Sanity check
    
    // Each entity
    for (int i = 0; i < msg->entity_count; i++) {
        if (offset + 16 > buffer_size) return -1;
        
        EntityState* e = &msg->entities[i];
        
        // Entity ID
        e->entity_id = read_uint32(&buffer[offset]);
        offset += 4;
        
        // Entity type
        e->entity_type = buffer[offset++];
        
        // Position
        e->x = read_float(&buffer[offset]);
        offset += 4;
        e->y = read_float(&buffer[offset]);
        offset += 4;
        
        // Health
        e->health = read_int16(&buffer[offset]);
        offset += 2;
        
        // Active
        e->active = buffer[offset++] != 0;
    }
    
    return offset;
}