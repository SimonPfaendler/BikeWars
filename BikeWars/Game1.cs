using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BikeWars.Components;
using BikeWars.Content.entities.items;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using BikeWars.Utilities;
using Microsoft.Xna.Framework.Audio;

namespace BikeWars;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _texture;

    private List<TestItem> _testItems;
    
    private int playerPosX = 1;
    private int playerPosY = 30;
    Player player;

    private bool _freeCamera = false;
    private bool _leftShiftPressed = false;
    
    private SpriteFont _debugFont;
    private Debugger _debugger;
    private SoundEffect walkingSound;

    private Camera2D camera;
    private Rectangle worldBounds;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Just for Testing
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
    }

    protected override void Initialize()
    {
        worldBounds = new Rectangle(0, 0, 4000, 2000); // Beispielgröße der Welt
        _testItems = new List<TestItem>();
        _testItems.Add(new TestItem(new Vector2(worldBounds.Width / 2 + 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
        _testItems.Add(new TestItem(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 + 50), new Point(32, 32)));

        
        camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, worldBounds);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _debugFont = Content.Load<SpriteFont>("assets/fonts/Arial");

        // Get screen dimensions
        int width = _graphics.PreferredBackBufferWidth;
        int height = _graphics.PreferredBackBufferHeight;

        // Spawn player in center of screen
        player = new Player(new Vector2(worldBounds.Width / 2, worldBounds.Height / 2), new Point(32, 32));

        // Center camera on player from game start
        camera.Position = player.Transform.Position;
        
        _debugger = new Debugger(_debugFont, player);
        
        // Load Soundeffects
        walkingSound = Content.Load<SoundEffect>("assets/sounds/Walking");
        player.LoadContent(walkingSound);
        
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
        {
            _leftShiftPressed = true;
        }

        if (_leftShiftPressed && Keyboard.GetState().IsKeyUp(Keys.LeftShift))
        {
            _freeCamera = !_freeCamera;
            _leftShiftPressed = false;
        }
        
        
        // freeze player if camera moves free
        player.Update(gameTime, _freeCamera);
        
        if (player.Intersects(_testItems[0].Collider))
        {
            player.Transform = new Transform(new Vector2(player.lastTransform.Position.X, player.lastTransform.Position.Y), player.lastTransform.Size);
            player.UpdateCollider();
        }

        if (_testItems.Count > 1)
        {
            if (player.Intersects(_testItems[1].Collider))
            {
                _testItems.RemoveAt(1);
            }    
        }

        _debugger.Update(gameTime);

        camera.Update(gameTime, player.Transform.Position, _freeCamera);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Everything within the first spriteBatch will be transformed by the camera
        _spriteBatch.Begin(transformMatrix: camera.GetTransform());
        player.Draw(_spriteBatch);
        foreach (var item in _testItems)
        {
            item.Draw(_spriteBatch);
        }
        _spriteBatch.End();

        // Render debugger on top and serparately from camera transformation to stay fixed 
        _spriteBatch.Begin();
        _debugger.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}