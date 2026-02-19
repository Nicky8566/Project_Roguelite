CC = gcc
CFLAGS = -Wall -Wextra -std=c11
LDFLAGS_WIN = -lws2_32

ifeq ($(OS),Windows_NT)
    LDFLAGS = $(LDFLAGS_WIN)
    EXE_EXT = .exe
else
    LDFLAGS =
    EXE_EXT =
endif

all: server client

server: udp_server.c
	$(CC) $(CFLAGS) udp_server.c -o udp_server$(EXE_EXT) $(LDFLAGS)
	@echo "Server compiled!"

client: udp_client.c
	$(CC) $(CFLAGS) udp_client.c -o udp_client$(EXE_EXT) $(LDFLAGS)
	@echo "Client compiled!"

clean:
	rm -f udp_server$(EXE_EXT) udp_client$(EXE_EXT)

.PHONY: all clean