CC = gcc
CFLAGS = -Wall -Wextra -std=c11 -O2
LDFLAGS_MATH = -lm
LDFLAGS_WIN = -lws2_32

ifeq ($(OS),Windows_NT)
    LDFLAGS = $(LDFLAGS_MATH) $(LDFLAGS_WIN)
else
    LDFLAGS = $(LDFLAGS_MATH)
endif

SOURCES = $(wildcard src/*.c)
OBJECTS = $(SOURCES:.c=.o)
TARGET = server.exe

all: $(TARGET)

$(TARGET): $(OBJECTS)
	@echo "Linking $(TARGET)..."
	$(CC) $(OBJECTS) -o $(TARGET) $(LDFLAGS)
	@echo "Build complete!"

%.o: %.c
	@echo "Compiling $<..."
	$(CC) $(CFLAGS) -c $< -o $@

clean:
	@echo "Cleaning..."
	rm -f $(OBJECTS) $(TARGET) src/*.o
	@echo "Clean complete!"

run: $(TARGET)
	@echo "Running $(TARGET)..."
	./$(TARGET)

.PHONY: all clean run