# Compiler and flags
CC = gcc
CFLAGS = -Wall -Wextra -std=c11 -O2
LDFLAGS = -lm

# Directories
SRC_DIR = src
BUILD_DIR = build

# Source files
SOURCES = $(wildcard $(SRC_DIR)/*.c) # Find all .c files in the src directory
OBJECTS = $(SOURCES:.c=.o) # Replace .c with .o for object files

# Output
TARGET = server.exe

# Default target
all: $(TARGET) # Build the target executable

# Link
$(TARGET): $(OBJECTS)
	$(CC) $(OBJECTS) -o $(TARGET) $(LDFLAGS)
	@echo "Build complete: $(TARGET)"

# Compile
%.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

# Clean
clean:
	rm -f $(SRC_DIR)/*.o $(TARGET)
	@echo "Cleaned build files"

# Run
run: $(TARGET)
	./$(TARGET)

.PHONY: all clean run