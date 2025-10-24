using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;

namespace uni_logo_game;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _logo;
    private Texture2D _background;

    private SoundEffect _logoHit;

    private SoundEffect _logoMiss;

    private Vector2 logoPosition;
    float angle = 0f;      // controls rotation around the circle
    float radius = 300f;   // circle size
    Vector2 center;        // circle center point




    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set window size (=same as background image)
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 1024;

        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _logo = Content.Load<Texture2D>("images/Unilogo");
        _background = Content.Load<Texture2D>("images/Background");

        center = new Vector2(
            GraphicsDevice.Viewport.Width / 2,
            GraphicsDevice.Viewport.Height / 2
        );

        _logoHit = Content.Load<SoundEffect>("audio/Logo_hit");

        _logoMiss = Content.Load<SoundEffect>("audio/Logo_miss");

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        // Increase the angle of rotation over time
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        angle += delta;

        // Compute logo position along a circle
        logoPosition.X = center.X + (float)Math.Cos(angle) * radius;
        logoPosition.Y = center.Y + (float)Math.Sin(angle) * radius;

        MouseState mouseState = Mouse.GetState();

        // Create hitbox around logo
        // Play different sound if mouse position withon hitbox
        if ((mouseState.LeftButton == ButtonState.Pressed))
        {
            if (logoPosition.X - 100 < mouseState.X && mouseState.X < logoPosition.X + 100)
            {
                if (logoPosition.Y - 100 < mouseState.Y && mouseState.Y < logoPosition.Y + 100)
                {
                    _logoHit.Play();
                }
            }
            else
            {
                _logoMiss.Play();
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();

        // Draw the background texture
        _spriteBatch.Draw(_background, Vector2.Zero, Color.White);

        // Draw the logo texture with updated logoPosition
        _spriteBatch.Draw(
            _logo,
            logoPosition,
            null,
            Color.White,
            0.0f,
            new Vector2(_logo.Width / 2f, _logo.Height / 2f),
            0.2f,
            SpriteEffects.None,
            0.0f
        );

        _spriteBatch.End();


        base.Draw(gameTime);
    }
}
