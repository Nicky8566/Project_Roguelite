using Microsoft.Xna.Framework;

namespace RogueliteGame.Rendering
{
    public class Camera
    {
        public Vector2 Position { get; set; }
        public float Zoom { get; set; }
        
        private int screenWidth;
        private int screenHeight;

        public Camera(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            Position = Vector2.Zero;
            Zoom = 1.0f;
        }

        // Convert world position to screen position
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            Vector2 offset = new Vector2(screenWidth / 2, screenHeight / 2);
            return (worldPosition - Position) * Zoom + offset;
        }

        // Convert screen position to world position
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            Vector2 offset = new Vector2(screenWidth / 2, screenHeight / 2);
            return (screenPosition - offset) / Zoom + Position;
        }

        // Smoothly follow a target
        public void Follow(Vector2 target, float smoothness = 0.1f)
        {
            Position = Vector2.Lerp(Position, target, smoothness);
        }

        // Get the transformation matrix for SpriteBatch
        public Matrix GetTransformMatrix()
        {
            return Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
                   Matrix.CreateScale(Zoom, Zoom, 1) *
                   Matrix.CreateTranslation(screenWidth / 2, screenHeight / 2, 0);
        }
    }
}