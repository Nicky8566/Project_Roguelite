#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifdef _WIN32
    #include <winsock2.h>
    #include <ws2tcpip.h>
    #pragma comment(lib, "ws2_32.lib")
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

#define SERVER_IP "127.0.0.1"  // Localhost (same computer)
#define SERVER_PORT 12345
#define BUFFER_SIZE 512

int main() {
    printf("=== UDP CLIENT ===\n");
    
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
    
    // Step 2: Setup server address
    struct sockaddr_in server_addr;
    // memset = “clear the struct so the OS sees only what I intend.”
    memset(&server_addr, 0, sizeof(server_addr));
    server_addr.sin_family = AF_INET; //ipv4
    // htons = "Re-arrange the bytes of this number so any computer on the network reads it correctly."
    server_addr.sin_port = htons(SERVER_PORT);
    server_addr.sin_addr.s_addr = inet_addr(SERVER_IP); 
    
    printf("Sending to %s:%d\n\n", SERVER_IP, SERVER_PORT);
    
    // Step 3: Send messages
    char buffer[BUFFER_SIZE];
    
    while (1) {
        printf("Enter message (or 'quit' to exit): ");
        fgets(buffer, BUFFER_SIZE, stdin);
        
        // Remove newline
        buffer[strcspn(buffer, "\n")] = 0;
        
        // Send message 
        int send_len = sendto(sock, buffer, strlen(buffer), 0,
                              (struct sockaddr*)&server_addr, sizeof(server_addr));
        
        if (send_len == SOCKET_ERROR) {
            printf("sendto failed!\n");
            break;
        }
        
        printf("Sent %d bytes\n\n", send_len);
        
        // Exit if user typed "quit"
        if (strcmp(buffer, "quit") == 0) {
            break;
        }
    }
    
    // Cleanup
    closesocket(sock);
    
#ifdef _WIN32
    WSACleanup();
#endif
    
    printf("Client closed.\n");
    return 0;
}