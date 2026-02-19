# CC = gcc
# CFLAGS = -Wall -Wextra -std=c11
# LDFLAGS_WIN = -lws2_32

# ifeq ($(OS),Windows_NT)
#     LDFLAGS = $(LDFLAGS_WIN)
#     EXE_EXT = .exe
# else
#     LDFLAGS =
#     EXE_EXT =
# endif

# all: server client

# server: udp_server.c
# 	$(CC) $(CFLAGS) udp_server.c -o udp_server$(EXE_EXT) $(LDFLAGS)
# 	@echo "Server compiled!"

# client: udp_client.c
# 	$(CC) $(CFLAGS) udp_client.c -o udp_client$(EXE_EXT) $(LDFLAGS)
# 	@echo "Client compiled!"

# clean:
# 	rm -f udp_server$(EXE_EXT) udp_client$(EXE_EXT)

# .PHONY: all clean

CC = gcc
CFLAGS = -Wall -Wextra -std=c11 -I../src
LDFLAGS_WIN = -lws2_32

ifeq ($(OS),Windows_NT)
    LDFLAGS = $(LDFLAGS_WIN)
    EXE_EXT = .exe
else
    LDFLAGS =
    EXE_EXT =
endif

all: server client test_protocol

server: udp_server.c
	$(CC) $(CFLAGS) udp_server.c -o udp_server$(EXE_EXT) $(LDFLAGS)

client: udp_client.c
	$(CC) $(CFLAGS) udp_client.c -o udp_client$(EXE_EXT) $(LDFLAGS)

test_protocol: test_protocol.c ../src/protocol.c
	$(CC) $(CFLAGS) test_protocol.c ../src/protocol.c -o test_protocol$(EXE_EXT) $(LDFLAGS)

clean:
	rm -f *.exe *.o udp_server udp_client test_protocol

.PHONY: all clean