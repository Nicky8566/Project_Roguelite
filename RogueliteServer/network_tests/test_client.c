#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#ifdef _WIN32
    #include <winsock2.h>
    #include <conio.h>  // For _kbhit()
    #pragma comment(lib, "ws2_32.lib")
     typedef int socklen_t;  
    #define sleep_ms(ms) Sleep(ms)
#else
    #include <sys/socket.h>
    #include <netinet/in.h>
    #include <arpa/inet.h>
    #include <unistd.h>
    #define closesocket close
    #define sleep_ms(ms) usleep((ms) * 1000)
    typedef int SOCKET;
    #define INVALID_SOCKET -1
#endif

#include "../src/protocol.h"

#define SERVER_IP "127.0.0.1"
#define SERVER_PORT 12345

int main() {
    printf("=== TEST CLIENT ===\n");
    
#ifdef _WIN32
    WSADATA wsa;
    WSAStartup(MAKEWORD(2, 2), &wsa);
#endif
    
    // Create socket
    SOCKET sock = socket(AF_INET, SOCK_DGRAM, 0);
    
    // Server address
    struct sockaddr_in server_addr;
    memset(&server_addr, 0, sizeof(server_addr));
    server_addr.sin_family = AF_INET;
    server_addr.sin_port = htons(SERVER_PORT);
    server_addr.sin_addr.s_addr = inet_addr(SERVER_IP);
    
    printf("Connected to server at %s:%d\n", SERVER_IP, SERVER_PORT);
    printf("Controls: W/A/S/D to move, Q to quit\n\n");
    
    // Send CONNECT message
    ConnectMessage connect_msg;
    strcpy(connect_msg.player_name, "TestPlayer");
    
    uint8_t buffer[MAX_PACKET_SIZE];
    int size = serialize_connect(&connect_msg, buffer, sizeof(buffer));
    sendto(sock, (char*)buffer, size, 0, (struct sockaddr*)&server_addr, sizeof(server_addr));
    
    printf("Sent CONNECT message\n");
    
    // Main loop
    uint8_t keys = 0;
    int tick = 0;
    
    while (1) {
        tick++;
        
        // Check for keyboard input (Windows only in this simple version)
#ifdef _WIN32
        if (_kbhit()) {
            char ch = _getch();
            if (ch == 'q' || ch == 'Q') break;
            
            keys = 0;
            if (ch == 'w' || ch == 'W') keys |= KEY_W;
            if (ch == 'a' || ch == 'A') keys |= KEY_A;
            if (ch == 's' || ch == 'S') keys |= KEY_S;
            if (ch == 'd' || ch == 'D') keys |= KEY_D;
        }
#endif
        
        // Send INPUT message
        InputMessage input;
        input.player_id = 1;
        input.keys = keys;
        input.mouse_x = 400.0f;
        input.mouse_y = 300.0f;
        
        size = serialize_input(&input, buffer, sizeof(buffer));
        sendto(sock, (char*)buffer, size, 0, (struct sockaddr*)&server_addr, sizeof(server_addr));
        
        // Receive STATE message (non-blocking would be better, but this is simple)
        struct sockaddr_in from_addr;
        socklen_t from_len = sizeof(from_addr);
        int recv_len = recvfrom(sock, (char*)buffer, sizeof(buffer), 0,
                               (struct sockaddr*)&from_addr, &from_len);
        
        if (recv_len > 0 && buffer[0] == MSG_STATE) {
            StateMessage state;
            deserialize_state(buffer, recv_len, &state);
            
            if (tick % 60 == 0) {  // Print every second
                printf("Tick %u - Entities: %u\n", state.tick, state.entity_count);
                for (int i = 0; i < state.entity_count; i++) {
                    EntityState* e = &state.entities[i];
                    printf("  ID %u: type=%u pos=(%.1f, %.1f) hp=%d\n",
                           e->entity_id, e->entity_type, e->x, e->y, e->health);
                }
            }
        }
        
        sleep_ms(16);  // 60 Hz
    }
    
    // Send DISCONNECT
    buffer[0] = MSG_DISCONNECT;
    sendto(sock, (char*)buffer, 1, 0, (struct sockaddr*)&server_addr, sizeof(server_addr));
    
    closesocket(sock);
#ifdef _WIN32
    WSACleanup();
#endif
    
    printf("Disconnected\n");
    return 0;
}