#include "vector2.h"
#include <stdio.h>
#include <math.h>

// Function IMPLEMENTATIONS (actual code)

Vector2 vector2_create(float x, float y) {
    Vector2 v;
    v.x = x;
    v.y = y;
    return v;
}

Vector2 vector2_add(Vector2 a, Vector2 b) {
    Vector2 result;
    result.x = a.x + b.x;
    result.y = a.y + b.y;
    return result;
}

Vector2 vector2_subtract(Vector2 a, Vector2 b) {
    Vector2 result;
    result.x = a.x - b.x;
    result.y = a.y - b.y;
    return result;
}

Vector2 vector2_multiply(Vector2 v, float scalar) {
    Vector2 result;
    result.x = v.x * scalar;
    result.y = v.y * scalar;
    return result;
}

float vector2_distance(Vector2 a, Vector2 b) {
    float dx = b.x - a.x;
    float dy = b.y - a.y;
    return sqrtf(dx * dx + dy * dy);  // Pythagorean theorem
}

void vector2_print(Vector2 v) {
    printf("(%.2f, %.2f)", v.x, v.y);
}