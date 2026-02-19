#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifdef _WIN32
    #include <winsock2.h>
    #include <ws2tcpip.h>
    #pragma comment(lib, "ws2_32.lib")
    typedef int socklen_t;
#else
    #include <sys/socket.h>
    #include <netinet/in.h>
    #include <arpa/inet.h>
    #include <unistd.h>
    #define closesocket close
    typedef int SOCKET;
    #define INVALID_SOCKET -1
    #define SOCKET_ERROR -1
#endif

#define PORT 12345
#define BUFFER_SIZE 512

int main() {
    printf("=== UDP SERVER ===\n");
    
#ifdef _WIN32
    // Windows requires WSA initialization
    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) {
        printf("Failed to initialize Winsock. Error: %d\n", WSAGetLastError());
        return 1;
    }
    printf("Winsock initialized.\n");
#endif
    
    // Step 1: Create socket
    SOCKET sock = socket(AF_INET, SOCK_DGRAM, 0);
    if (sock == INVALID_SOCKET) {
        printf("Failed to create socket!\n");
        return 1;
    }
    printf("Socket created.\n");
    
    // Step 2: Bind to port (Server needs to bind so clients can find it, clients don't bind, os auto-assigns them a port when they send)
    struct sockaddr_in server_addr;
    memset(&server_addr, 0, sizeof(server_addr));
    server_addr.sin_family = AF_INET;
    server_addr.sin_addr.s_addr = INADDR_ANY;  // Listen on all interfaces
    server_addr.sin_port = htons(PORT);        // Port 12345
    
    if (bind(sock, (struct sockaddr*)&server_addr, sizeof(server_addr)) == SOCKET_ERROR) {
        printf("Bind failed!\n");
        closesocket(sock);
        return 1;
    }
    printf("Bound to port %d\n", PORT);
    
    // Step 3: Receive messages
    printf("Waiting for messages...\n\n");
    
    char buffer[BUFFER_SIZE];
    struct sockaddr_in client_addr;
    socklen_t client_addr_len = sizeof(client_addr);
    
    while (1) {
        memset(buffer, 0, BUFFER_SIZE);
        
        int recv_len = recvfrom(sock, buffer, BUFFER_SIZE, 0,
                                (struct sockaddr*)&client_addr, &client_addr_len);
        
        if (recv_len == SOCKET_ERROR) {
            printf("recvfrom failed!\n");
            break;
        }
        
        // Print received message
        printf("Received %d bytes from %s:%d\n",
               recv_len,
               inet_ntoa(client_addr.sin_addr),
               ntohs(client_addr.sin_port));
        printf("Message: %s\n\n", buffer);
        
        // Stop if we receive "quit"
        if (strcmp(buffer, "quit") == 0) {
            printf("Received quit command. Shutting down.\n");
            break;
        }
    }
    
    // Cleanup
    closesocket(sock);
    
#ifdef _WIN32
    WSACleanup();
#endif
    
    printf("Server closed.\n");
    return 0;
}