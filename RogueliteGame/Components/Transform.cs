using Microsoft.Xna.Framework;

// struct = Lightweight data container (better performance than class for small data)
// Transform = Holds position data
// No methods, no logic—just pure data!
namespace RogueliteGame.Components
{
    public struct Transform
    {
        public Vector2 Position;
    }
}