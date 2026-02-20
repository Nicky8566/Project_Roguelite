#include "network.h"
#include <stdio.h>
#include <string.h>

#ifdef _WIN32
    #include <winsock2.h>
    #define close closesocket
#else
    #include <fcntl.h>
    #include <unistd.h>
    #define closesocket close
#endif

// Initialize network socket
SOCKET network_init(int port) {
#ifdef _WIN32
    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) {
        printf("WSAStartup failed\n");
        return INVALID_SOCKET;
    }
#endif
    
    // Create UDP socket
    SOCKET sock = socket(AF_INET, SOCK_DGRAM, 0);
    if (sock == INVALID_SOCKET) {
        printf("Failed to create socket\n");
        return INVALID_SOCKET;
    }
    
    // Bind to port
    struct sockaddr_in server_addr;
    memset(&server_addr, 0, sizeof(server_addr));
    server_addr.sin_family = AF_INET;
    server_addr.sin_addr.s_addr = INADDR_ANY;
    server_addr.sin_port = htons(port);
    
    if (bind(sock, (struct sockaddr*)&server_addr, sizeof(server_addr)) < 0) {
        printf("Bind failed\n");
        closesocket(sock);
        return INVALID_SOCKET;
    }
    
    // Set non-blocking mode
#ifdef _WIN32
    u_long mode = 1;
    ioctlsocket(sock, FIONBIO, &mode);
#else
    int flags = fcntl(sock, F_GETFL, 0);
    fcntl(sock, F_SETFL, flags | O_NONBLOCK);
#endif
    
    printf("Network initialized on port %d\n", port);
    return sock;
}

// Compare addresses (helper function)
static bool addr_equal(struct sockaddr_in* a, struct sockaddr_in* b) {
    return a->sin_addr.s_addr == b->sin_addr.s_addr &&
           a->sin_port == b->sin_port;
}

// Find or create client
NetworkClient* network_find_or_create_client(GameState* game, struct sockaddr_in* addr) {
    // Check if client already exists
    for (int i = 0; i < MAX_CLIENTS; i++) {
        if (game->clients[i].connected && addr_equal(&game->clients[i].addr, addr)) {
            return &game->clients[i];
        }
    }
    
    // Find empty slot
    for (int i = 0; i < MAX_CLIENTS; i++) {
        if (!game->clients[i].connected) {
            game->clients[i].addr = *addr;
            game->clients[i].connected = true;
            game->clients[i].last_packet_time = game->total_time;
            game->client_count++;
            
            // Spawn player entity
            Vector2 spawn_pos = vector2_create(400.0f + i * 50.0f, 300.0f);
            Entity* player = entity_create(&game->entity_manager, ENTITY_TYPE_PLAYER, spawn_pos);
            game->clients[i].player_id = player->id;
            
            printf("New client connected: %s:%d (Player ID: %u)\n",
                   inet_ntoa(addr->sin_addr),
                   ntohs(addr->sin_port),
                   player->id);
            
            return &game->clients[i];
        }
    }
    
    printf("Server full! Cannot accept more clients.\n");
    return NULL;
}

// Receive and process packets
void network_receive_packets(GameState* game) {
    uint8_t buffer[MAX_PACKET_SIZE];
    struct sockaddr_in client_addr;
    socklen_t addr_len = sizeof(client_addr);
    
    // Non-blocking receive (returns immediately if no data)
    while (1) {
        // recvfrom reads data from the socket into the buffer. 
        // It also fills in client_addr with the sender's address and addr_len with the size of that address structure. 
        // If there are no packets to read, it returns -1 (or 0 on some platforms), which we check for to break out of the loop.
        int recv_len = recvfrom(game->socket, (char*)buffer, sizeof(buffer), 0,
                               (struct sockaddr*)&client_addr, &addr_len);
        
        if (recv_len <= 0) break;  // No more packets
        
        // Get message type
        uint8_t msg_type = buffer[0];
        
        // Find or create client
        NetworkClient* client = network_find_or_create_client(game, &client_addr);
        if (!client) continue;
        
        client->last_packet_time = game->total_time;
        
        // Handle message
        if (msg_type == MSG_CONNECT) {
            ConnectMessage msg;
            deserialize_connect(buffer, recv_len, &msg);
            printf("Player '%s' connected\n", msg.player_name);
        }
        else if (msg_type == MSG_INPUT) {
            InputMessage msg;
            deserialize_input(buffer, recv_len, &msg);
            
            // Find player entity
            Entity* player = entity_get_by_id(&game->entity_manager, client->player_id);
            if (player) {
                // Apply input (simple movement)
                Vector2 velocity = vector2_create(0, 0);
                
                if (msg.keys & KEY_W) velocity.y -= 100.0f;
                if (msg.keys & KEY_S) velocity.y += 100.0f;
                if (msg.keys & KEY_A) velocity.x -= 100.0f;
                if (msg.keys & KEY_D) velocity.x += 100.0f;
                
                player->velocity = velocity;
            }
        }
        else if (msg_type == MSG_DISCONNECT) {
            printf("Client disconnected: %s:%d\n",
                   inet_ntoa(client_addr.sin_addr),
                   ntohs(client_addr.sin_port));
            
            // Remove player entity
            entity_destroy(&game->entity_manager, client->player_id);
            
            client->connected = false;
            game->client_count--;
        }
    }
}

// Broadcast game state to all clients
void network_broadcast_state(GameState* game) {
    StateMessage state;
    state.tick = game->tick_count;
    state.entity_count = 0;
    
    // Pack entities into state message
    for (size_t i = 0; i < game->entity_manager.count && state.entity_count < 32; i++) {
        Entity* e = &game->entity_manager.entities[i];
        if (!e->active) continue;
        
        EntityState* es = &state.entities[state.entity_count++];
        es->entity_id = e->id;
        es->entity_type = e->type;
        es->x = e->position.x;
        es->y = e->position.y;
        es->health = e->health;
        es->active = e->active;
    }
    
    // Serialize
    uint8_t buffer[MAX_PACKET_SIZE];
    int size = serialize_state(&state, buffer, sizeof(buffer));
    
    if (size < 0) {
        printf("Failed to serialize state\n");
        return;
    }
    
    // Send to all connected clients
    for (int i = 0; i < MAX_CLIENTS; i++) {
        if (game->clients[i].connected) {
            // The sendto function is used to send the serialized state message to each connected client.
            // It takes the socket, the buffer containing the serialized message, the size of that message,
            // and the address of the client to send it to. The sizeof(game->clients[i].addr) specifies the size of the address structure.
            sendto(game->socket, (char*)buffer, size, 0,
                   (struct sockaddr*)&game->clients[i].addr,
                   sizeof(game->clients[i].addr));
        }
    }
}

// Cleanup network
void network_cleanup(SOCKET sock) {
    if (sock != INVALID_SOCKET) {
        closesocket(sock);
    }
    
#ifdef _WIN32
    WSACleanup();
#endif
}