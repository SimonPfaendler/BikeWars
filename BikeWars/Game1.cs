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
using System;

namespace BikeWars;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _texture;

    private List<TestItem> _testItems;
    
    Player player;

    private bool _freeCamera = false;
    private bool _cKeyPressed = false;

    private SoundHandler soundHandler { get; set; }
    private SpriteFont _debugFont;
    private Debugger _debugger;

    private Camera2D camera;

    //Defines border of visble game world
    private Rectangle worldBounds;

    // Paths
    private const String ARIAL_FONT = "assets/fonts/Arial";

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
        worldBounds = new Rectangle(0, 0, 4000, 2000); // Example values for game world size
        _testItems = new List<TestItem>();
        _testItems.Add(new TestItem(new Vector2(worldBounds.Width / 2 + 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
        _testItems.Add(new TestItem(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 + 50), new Point(32, 32)));

        camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, worldBounds);
        
        // Spawn player in center of screen
        player = new Player(new Vector2(worldBounds.Width / 2, worldBounds.Height / 2), new Point(32, 32));
        
        soundHandler = new SoundHandler();
        // Center camera on player from game start
        camera.Position = player.Transform.Position;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _debugFont = Content.Load<SpriteFont>(ARIAL_FONT);
        _debugger = new Debugger(_debugFont, player);

        // Load Soundeffects
        player.LoadContent(Content.Load<SoundEffect>(soundHandler.WALKING_SOUND_PATH));
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.C))
        {
            _cKeyPressed = true;
        }

        // Switch between camera Playerlock and FreeLook
        if (_cKeyPressed && Keyboard.GetState().IsKeyUp(Keys.C))
        {
            _freeCamera = !_freeCamera;
            _cKeyPressed = false;
        }
        
        
        // If camera is in FreeLook mode dont update player movement
        //player.Update(gameTime, _freeCamera);
        if (camera.Mode == CameraMode.FreeLook)
        {
            player.SetCanMove(false);
        } else
        {
            player.SetCanMove(true);
        }
        player.Update(gameTime);
        
        if (player.Intersects(_testItems[0].Collider))
        {
            player.SetLastTransform();
            //player.Transform = new Transform(new Vector2(player.LastTransform.Position.X, player.LastTransform.Position.Y), player.LastTransform.Size);
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

        // Frist update all objects and last update camera view
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

    private void HandleCameraMode()
    {
        
    }
}