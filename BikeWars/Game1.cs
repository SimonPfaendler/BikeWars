using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BikeWars.Content.entities.items;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using BikeWars.Content.src.utils.SaveLoadExample;
using BikeWars.Utilities;
using Microsoft.Xna.Framework.Audio;
using System;
using InputAction = BikeWars.Content.engine.GameAction;
using BikeWars.Content.src.screens.Overlay;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace BikeWars;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private List<ItemBase> _testItems;
    Player player;
    private Hobo hobo;

    private bool _freeCamera = false;
    

    private SoundHandler soundHandler { get; set; }
    private SpriteFont _debugFont;
    private Debugger _debugger;

    private Camera2D camera;

    //Defines border of visble game world
    private Rectangle worldBounds;

    // Paths
    private const String ARIAL_FONT = "assets/fonts/Arial";

    // counter for SaveLoadExample
    private int _counter = 0;
    private float _counterTimer = 0;

    // overlay
    private Overlay _overlay;
    private TiledMap _tiledMap;
    private TiledMapRenderer _tiledMapRenderer;

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
        _testItems = new List<ItemBase>();
        _testItems.Add(new Item(new Vector2(worldBounds.Width / 2 + 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
        _testItems.Add(new Chest(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
        camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, worldBounds);

        // Spawn player in center of screen
        player = new Player(new Vector2(worldBounds.Width / 2, worldBounds.Height / 2), new Point(32, 32));
        hobo = new Hobo(new Vector2(worldBounds.Width / 2 + 10, worldBounds.Height / 2), new Point(32, 32));

        soundHandler = new SoundHandler();
        // Center camera on player from game start
        camera.Position = player.Transform.Position;

        // Create SaveLoad and load saved data (if there is any)
        _counter = SaveLoad.LoadGame();

        base.Initialize();
    }

    protected override void LoadContent()
    {

        _debugFont = Content.Load<SpriteFont>(ARIAL_FONT);
        _debugger = new Debugger(_debugFont, player);

        _tiledMap = Content.Load<TiledMap>("assets/Map/Bikewars_Tilemap");
        _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load Soundeffects
        player.LoadContent(Content, Content.Load<SoundEffect>(soundHandler.DRIVING_SOUND_PATH));
        hobo.LoadContent(Content, Content.Load<SoundEffect>(soundHandler.WALKING_SOUND_PATH));
        if (_testItems.Count > 1)
        {
            _testItems[1].LoadContent(Content);
        }
        _overlay = new Overlay(_debugFont, GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        InputHandler.Update();
        if (InputHandler.IsPressed(GameAction.ESC))
            Exit();

        if (InputHandler.GamePad.Connected)
        {
            Console.WriteLine("Gamepad connected");
        }
        Console.WriteLine("Gampe not connected");

        if (InputHandler.IsPressed(GameAction.TOGGLE_CAMERA))
        {
            _freeCamera = !_freeCamera;
        }

        // If camera is in FreeLook mode dont update player movement
        if (camera.Mode == CameraMode.FreeLook)
        {
            player.Immobalize(true);
        } else
        {
            player.Immobalize(false);
        }
        player.Update(gameTime);

        if (player.Intersects(_testItems[0].Collider))
        {
            player.SetLastTransform();
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

        HandleCounter(gameTime);
        HandleSaveLoadInput();

        _tiledMapRenderer.Update(gameTime);

        hobo.Update(gameTime);
        base.Update(gameTime);
    }

    // Handles the counter increment logic once per second
    private void HandleCounter(GameTime gameTime)
    {
        _counterTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_counterTimer >= 1)
        {
            _counter++;
            _counterTimer = 0;
        }
    }

    // handels the key inputs for save and load
    private void HandleSaveLoadInput()
    {
        // edge-triggered: pressed this frame, not last frame
        if (InputHandler.IsPressed(GameAction.SAVE))
            SaveLoad.SaveGame(_counter);

        if (InputHandler.IsPressed(GameAction.LOAD))
        {
            _counter = SaveLoad.LoadGame();
            _counterTimer = 0;
        }

        if (InputHandler.IsPressed(GameAction.RESET))
        {
            _counter = 0;
            _counterTimer = 0;
            Console.WriteLine("Counter Reset. Counter=0");
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _tiledMapRenderer.Draw(camera.GetTransform());

        // Everything within the first spriteBatch will be transformed by the camera
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetTransform());
        player.Draw(_spriteBatch);
        hobo.Draw(_spriteBatch);
        foreach (var item in _testItems)
        {
            item.Draw(_spriteBatch);
        }

        // lifelines under the player (world-space)
        _overlay.DrawOnWorld(_spriteBatch, player);
        _spriteBatch.End();

        // Render debugger on top and serparately from camera transformation to stay fixed
        _spriteBatch.Begin();
        _debugger.Draw(_spriteBatch);
        _spriteBatch.End();

        //draw the counter
        _spriteBatch.Begin();
        _spriteBatch.DrawString(_debugFont, $"Counter: {_counter}", new Vector2(20, 100), Color.Black);
        _spriteBatch.DrawString(_debugFont, "T=Save  L=Load  R=Reset counter", new Vector2(20, 125), Color.Black);
        _spriteBatch.End();
        base.Draw(gameTime);

        // draw the invetory and timer
        _spriteBatch.Begin();
        _overlay.DrawOnScreen(_spriteBatch, gameTime);
        _spriteBatch.End();
    }

    private void HandleCameraMode()
    {

    }
}