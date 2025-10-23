using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Uni_Logo_Project_simonge;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _background;
    private Texture2D _logo;
    private SoundEffect _hitSound;
    private SoundEffect _missSound;
    private double _changingAngle;
    private Vector2 _rotatingPosition;
    private float _logoScale = 0.25f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {

        base.Initialize();
        // Setting the size of the background (= size of the window)
        _graphics.PreferredBackBufferWidth = _background.Width;
        _graphics.PreferredBackBufferHeight = _background.Height;
        _graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _background = Content.Load<Texture2D>("Grafiken/Background");
        _logo = Content.Load<Texture2D>("Grafiken/Unilogo");
        
        _hitSound = Content.Load<SoundEffect>("Soundeffekte/Logo_hit");
        _missSound =  Content.Load<SoundEffect>("Soundeffekte/Logo_miss");
        
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        // Increase angle for rotation over time
        _changingAngle += 2f * gameTime.ElapsedGameTime.TotalSeconds;
        _changingAngle %= 2f * Math.PI;
        
        Vector2 originBackground = new Vector2(_background.Width / 2f, _background.Height / 2f);
        
        // rotation of the logo
        float distanceToCenter = 200f;
        _rotatingPosition.X = originBackground.X + distanceToCenter * (float)Math.Cos(_changingAngle);
        _rotatingPosition.Y =  originBackground.Y + distanceToCenter * (float)Math.Sin(_changingAngle);
        
        // Logic for hit/miss sound effects
        float maximumAcceptableDistance = _logo.Width * _logoScale / 2f;
        MouseState mousePosition = Mouse.GetState();
        if (mousePosition.LeftButton == ButtonState.Pressed)
        {
            float distanceX = mousePosition.X - _rotatingPosition.X;
            float distanceY = mousePosition.Y - _rotatingPosition.Y;

            if (distanceX * distanceX + distanceY * distanceY <= maximumAcceptableDistance * maximumAcceptableDistance)
            {
                _hitSound.Play();
            }
            else
            {
                _missSound.Play();
            }
            
        }
        
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        // Drawing Code for the Background
        _spriteBatch.Begin();
        _spriteBatch.Draw(texture:_background, position:Vector2.Zero, color:Color.White);
        // Drawing Code for the Logo
        
        Vector2 originLogo = new Vector2(_logo.Width / 2f, _logo.Height / 2f);
        
        _spriteBatch.Draw(texture:_logo, position: _rotatingPosition, sourceRectangle:null, color:Color.White, rotation:0f, origin:originLogo, scale:_logoScale, effects: SpriteEffects.None, layerDepth:0);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}