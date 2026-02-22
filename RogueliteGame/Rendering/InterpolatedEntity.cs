using Microsoft.Xna.Framework;
using RogueliteGame.Networking;
using System;

namespace RogueliteGame
{
    public class InterpolatedEntity
    {
        
        public uint EntityId { get; set; }
        public EntityType Type { get; set; }
        
        public Vector2 Position { get; private set; }
        public Vector2 TargetPosition { get; private set; }
        private Vector2 previousPosition;
        
        public short Health { get; set; }
        public bool Active { get; set; }
        
        // NEW: Rotation (in radians)
        public float Rotation { get; set; }

        // New
        public float TargetRotation;
        
        private float interpolationProgress;
        private const float InterpolationSpeed = 0.3f;

        public InterpolatedEntity(uint id, EntityType type, Vector2 position)
        {
            EntityId = id;
            Type = type;
            Position = position;
            TargetPosition = position;
            previousPosition = position;
            interpolationProgress = 1.0f;
            Rotation = 0f;  // Start facing right
        }

        public void SetTargetPosition(Vector2 newTarget)
        {
            previousPosition = Position;
            TargetPosition = newTarget;
            interpolationProgress = 0.0f;
            
            // Calculate rotation based on movement direction
            Vector2 direction = newTarget - Position;
            if (direction.LengthSquared() > 0.1f)  // Only rotate if actually moving
            {
                Rotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (interpolationProgress < 1.0f)
            {
                interpolationProgress += InterpolationSpeed;
                if (interpolationProgress > 1.0f)
                    interpolationProgress = 1.0f;
                
                Position = Vector2.Lerp(previousPosition, TargetPosition, interpolationProgress);
            }
        }
    }
}