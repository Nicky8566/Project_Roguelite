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

all: test_client test_protocol

test_client: test_client.c ../src/protocol.c
	$(CC) $(CFLAGS) test_client.c ../src/protocol.c -o test_client$(EXE_EXT) $(LDFLAGS)
	@echo "Test client compiled!"

test_protocol: test_protocol.c ../src/protocol.c
	$(CC) $(CFLAGS) test_protocol.c ../src/protocol.c -o test_protocol$(EXE_EXT) $(LDFLAGS)
	@echo "Protocol test compiled!"

clean:
	rm -f *.exe *.o test_client test_protocol

.PHONY: all clean