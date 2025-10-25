using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace App1;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _BackgroundImage;
    private Texture2D _Unilogo;
    private Vector2 _center; 
    private float _angle;
    private Vector2 _origin;
    private float _scale = 0.5f;  // makes sure the logo fits in the screen
    private SoundEffect _Logo_hit; // 👈 Ton über Logo
    private SoundEffect _Logo_miss;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += (_, __) =>
        {
            _graphics.PreferredBackBufferWidth  = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            _graphics.ApplyChanges();
        };
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _BackgroundImage = Content.Load<Texture2D>("BackgroundImage"); 
        _Unilogo = Content.Load<Texture2D>("Unilogo");
        // calculating center of the screen
        var vp   = GraphicsDevice.Viewport;
        _center  = new Vector2(vp.Width * 0.5f, vp.Height * 0.5f);
        _origin = new Vector2(_Unilogo.Width / 2f, _Unilogo.Height / 2f);
        _Logo_hit = Content.Load<SoundEffect>("Logo_hit");
        _Logo_miss = Content.Load<SoundEffect>("Logo_miss");
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit(); // breaks if esc is pressed

        // makes the logo turn 
        _angle += 0.5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        var mouse = Mouse.GetState();

        if (mouse.LeftButton == ButtonState.Pressed)
        {
            
            var Position_mouse = new Vector2(mouse.X, mouse.Y);
            // calculating space of logo
            var logo_space = new Rectangle(
                (int)(_center.X - _Unilogo.Width * _scale / 2f),
                (int)(_center.Y - _Unilogo.Height * _scale / 2f),
                (int)(_Unilogo.Width * _scale),
                (int)(_Unilogo.Height * _scale)
            );
            
            if (logo_space.Contains(Position_mouse))
                _Logo_hit.Play();
            else
                _Logo_miss.Play();
        }


    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Blue);
        // shows the background image on the full scale
        var full = GraphicsDevice.Viewport.Bounds;
        
        _spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        _spriteBatch.Draw(texture:_BackgroundImage, destinationRectangle: full, color:Color.White);
        _spriteBatch.Draw(
            texture: _Unilogo,
            position: _center,
            sourceRectangle: null,
            color: Color.White,
            rotation: _angle,
            origin: _origin,
            scale: _scale,
            effects: SpriteEffects.None,
            layerDepth: 0f);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}