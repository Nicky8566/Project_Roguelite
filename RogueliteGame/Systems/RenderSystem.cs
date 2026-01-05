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
            ref Transform transform = ref entity.Get<Transform>();
            
            Color color = entity.Has<PlayerTag>() ? Color.Lime : Color.Red;
            
            Rectangle rect = new Rectangle(
                (int)transform.Position.X, 
                (int)transform.Position.Y, 
                32, 32
            );
            
            spriteBatch.Draw(pixelTexture, rect, color);
        }
    }
}
