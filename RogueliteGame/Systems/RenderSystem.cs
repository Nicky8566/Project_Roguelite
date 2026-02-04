using DefaultEcs;
using DefaultEcs.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueliteGame.Components;
using EcsWorld = DefaultEcs.World;

namespace RogueliteGame.Systems
{
    public class RenderSystem : AEntitySetSystem<SpriteBatch>
    {
        private Texture2D pixelTexture;

        public RenderSystem(EcsWorld world, GraphicsDevice graphicsDevice)
            : base(world.GetEntities()
                .With<Transform>()
                .AsSet())
        {
            pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
        }

        protected override void Update(SpriteBatch spriteBatch, in Entity entity)
        {

            if (!entity.IsAlive)
               return;

            ref Transform transform = ref entity.Get<Transform>();

            // Choose color based on entity type
            Color color;
            if (entity.Has<PlayerTag>())
                color = Color.Lime;          // Player = bright green
            else if (entity.Has<Projectile>())
                color = Color.Yellow;        // Bullets = yellow
            else
                color = Color.Red;           // Enemies = red

            // Bullets are smaller (8x8 instead of 32x32)
            int size = entity.Has<Projectile>() ? 8 : 32;

            Rectangle rect = new Rectangle(
                (int)transform.Position.X,
                (int)transform.Position.Y,
                size, size
            );

            spriteBatch.Draw(pixelTexture, rect, color);
        }
    }
}
