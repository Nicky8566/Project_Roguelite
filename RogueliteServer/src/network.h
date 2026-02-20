#ifndef NETWORK_H
#define NETWORK_H

#include "game_loop.h"
#include "protocol.h"

// Initialize socket (create, bind, set non-blocking)
SOCKET network_init(int port);

// Find or create client from address
NetworkClient* network_find_or_create_client(GameState* game, struct sockaddr_in* addr);

// Handle incoming packets
void network_receive_packets(GameState* game);

// Broadcast game state to all clients
void network_broadcast_state(GameState* game);

// Cleanup
void network_cleanup(SOCKET sock);

#endif