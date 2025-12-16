using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RogueliteGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Player variables
    private Vector2 playerPosition;
    private float playerSpeed = 200f; // pixels per second
    private Texture2D pixelTexture; // For drawing rectangles

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Set starting position (center of 800x600 window)
        playerPosition = new Vector2(400, 300);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white pixel for drawing rectangles
        pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // DELTA TIME - makes movement frame-independent
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Keyboard input
        var keyboardState = Keyboard.GetState();
        
        if (keyboardState.IsKeyDown(Keys.W))
            playerPosition.Y -= playerSpeed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.S))
            playerPosition.Y += playerSpeed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.A))
            playerPosition.X -= playerSpeed * deltaTime;
        if (keyboardState.IsKeyDown(Keys.D))
            playerPosition.X += playerSpeed * deltaTime;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        
        // Draw player as a white square
        DrawRectangle(playerPosition, 32, 32, Color.White);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    // Helper method to draw colored rectangles
    private void DrawRectangle(Vector2 position, int width, int height, Color color)
    {
        var rect = new Rectangle((int)position.X, (int)position.Y, width, height);
        _spriteBatch.Draw(pixelTexture, rect, color);
    }
}