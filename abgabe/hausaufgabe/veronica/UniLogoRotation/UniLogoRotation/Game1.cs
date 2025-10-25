using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UniLogoRotation;

// Main game class that manages graphics, sounds, and the moving logo’s behavior.
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    Texture2D backgroundSprite;
    Texture2D logoSprite;
    SoundEffect hitSound;
    SoundEffect missSound;
    
    
    private Vector2 _logoPosition;
    private Rectangle _backgroundRect;
    private Rectangle _logoRect;
    private Vector2 _logoSpeed;
    Random rand = new Random();

    MouseState mouseState;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        
        // Load game assets
        backgroundSprite = Content.Load<Texture2D>("Background");
        logoSprite = Content.Load<Texture2D>("Unilogo");
        
        hitSound = Content.Load<SoundEffect>("Logo_hit");
        missSound = Content.Load<SoundEffect>("Logo_miss");
        
        _backgroundRect =  new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        
        int logoSize = Math.Min(150, Math.Min(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight) / 4);
        
        //set initial logo position 
        _logoPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2 - logoSize / 2, _graphics.PreferredBackBufferHeight / 2 - logoSize / 2);
        
        _logoRect = new Rectangle((int)_logoPosition.X, (int)_logoPosition.Y, logoSize, logoSize);
        
        //set initial logo speed
        _logoSpeed = new Vector2((float)(rand.NextDouble() * 200 - 100), (float)(rand.NextDouble() * 200 - 100));

        
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        // Move the logo based on its speed and the time elapsed since the last frame.
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds; _logoPosition += _logoSpeed * delta;
        
        // Bounce the logo off the screen edges by reversing its direction when it hits a boundary.
        if (_logoPosition.X < 0 || _logoPosition.X + _logoRect.Width > _graphics.PreferredBackBufferWidth)
            _logoSpeed.X *= -1;

        if (_logoPosition.Y < 0 || _logoPosition.Y + _logoRect.Height > _graphics.PreferredBackBufferHeight)
            _logoSpeed.Y *= -1;
        
        _logoRect.X = (int)_logoPosition.X;
        _logoRect.Y = (int)_logoPosition.Y;
        
        // Play a hit or miss sound depending on whether the player clicks on the logo.
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            if (_logoRect.Contains(mouseState.X, mouseState.Y))
                hitSound?.Play();
            else
                missSound?.Play();
        }
        mouseState = Mouse.GetState();
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteBatch.Begin();
        // drawing background
        _spriteBatch.Draw(backgroundSprite, _backgroundRect, Color.White);
        
        // drawing logo
        _spriteBatch.Draw(logoSprite, _logoRect, Color.White);
        _spriteBatch.End();

        
        base.Draw(gameTime);
    }
}