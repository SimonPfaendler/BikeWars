using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace App2;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private DisplayMode display;
    private MouseState mouseState;
    
    // Resources and items to add
    private Texture2D background;
    private Texture2D logo;
    private SoundEffect clickHit;
    private SoundEffect clickMiss;
    
    // paths to the resources
    private const String BACKGROUND_PATH = "assets/images/Background";
    private const String LOGO_PATH = "assets/images/Unilogo";
    private const String CLICK_HIT_PATH = "assets/sounds/Logo_hit";
    private const String CLICK_MISS_PATH = "assets/sounds/Logo_miss";
    
    // logic for the rotation around the center
    private Vector2 logoPos;
    private Vector2 centerPoint;
    private Vector2 directionToCenter;
    private float angle;
    private int radius;
    private const int ROTATION_SPEED = 2;
    private const int SCALE_LOGO = 7;
    
    private Rectangle logoRectCollision;
    
    // logic for clicking
    private bool is_pressing;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode; // Maybe we can use next time viewport but for this, it should be sufficient
        
        _graphics.PreferredBackBufferWidth = display.Width;
        _graphics.PreferredBackBufferHeight = display.Height;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        centerPoint = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
        radius = 350;
        angle = 0;
        is_pressing = false;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        background = Content.Load<Texture2D>(BACKGROUND_PATH);
        logo = Content.Load<Texture2D>(LOGO_PATH);
        clickHit = Content.Load<SoundEffect>(CLICK_HIT_PATH);
        clickMiss = Content.Load<SoundEffect>(CLICK_MISS_PATH);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        MouseState mouse = Mouse.GetState();

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        angle += ROTATION_SPEED * deltaTime;
        
        int logoWidth = _graphics.PreferredBackBufferWidth / SCALE_LOGO;
        int logoHeight = _graphics.PreferredBackBufferHeight / SCALE_LOGO;
        
        // start the logo left to center point
        logoPos.X = centerPoint.X + (float)Math.Cos(angle) * radius - logoWidth / 2f;
        logoPos.Y = centerPoint.Y + (float)Math.Sin(angle) * radius - logoHeight / 2f;
        logoRectCollision = new Rectangle((int)logoPos.X, (int)logoPos.Y, logoWidth, logoHeight);

        if (mouse.LeftButton == ButtonState.Pressed)
        {
            is_pressing = true;
        }
        if (is_pressing && mouse.LeftButton == ButtonState.Released)
        {
            if (logoRectCollision.Contains(mouse.Position))
            {
                clickHit.Play();    
            }
            else
            {
                clickMiss.Play();
            }
            is_pressing = false;
        }
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(blendState: BlendState.Opaque);
        _spriteBatch.Draw(background, destinationRectangle: new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
        _spriteBatch.End();
    
        _spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        _spriteBatch.Draw(logo, destinationRectangle: new Rectangle((int)logoPos.X, (int)logoPos.Y, _graphics.PreferredBackBufferWidth / SCALE_LOGO, _graphics.PreferredBackBufferHeight / SCALE_LOGO), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}