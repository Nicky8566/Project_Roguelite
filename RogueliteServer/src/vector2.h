#ifndef VECTOR2_H
#define VECTOR2_H

// Vector2 represents a 2D position or direction
typedef struct {
    float x;
    float y;
} Vector2;

// Function declarations (signatures)
Vector2 vector2_create(float x, float y);
Vector2 vector2_add(Vector2 a, Vector2 b);
Vector2 vector2_subtract(Vector2 a, Vector2 b);
Vector2 vector2_multiply(Vector2 v, float scalar);
float vector2_distance(Vector2 a, Vector2 b);
void vector2_print(Vector2 v);

#endif