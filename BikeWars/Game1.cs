using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.managers;
using BikeWars.Content.screens;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using System;
using BikeWars.Content.components;

namespace BikeWars;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    public SpriteBatch SpriteBatch { get; private set; }
    public ScreenManager ScreenManager;
    private AudioService _audioService;
    // public static AudioService Audio => _audioService;
    public static Texture2D background;
    public static GameTime CurrentGameTime { get; private set; }


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.HardwareModeSwitch = false; // Use borderless full screen for smoother transitions
    }

    public void SetResolution(int width, int height, bool fullscreen)
    {
        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.IsFullScreen = fullscreen;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        ScreenManager = new ScreenManager();
        ScreenManager.OnReturnToMainMenu += CreateMainMenu;
        base.Initialize();
    }

    private void CreateMainMenu()
    {
        MainMenuScreen mainMenu = new MainMenuScreen(background, UIAssets.DefaultFont, _audioService);
        mainMenu.LoadContent(Content);
        mainMenu.GraphicsRequested += OnGraphicsRequested;
        ScreenManager.AddScreen(mainMenu);
    }

    protected override void LoadContent()
    {
        this.SpriteBatch = new SpriteBatch(GraphicsDevice);
        RenderPrimitives.Init(GraphicsDevice);
        UIAssets.Load(Content);

        _audioService = new AudioService(Content);
        _audioService.LoadContent();

        background = Content.Load<Texture2D>("assets/images/Startbildschirm");
        StartScreen startScreen = new StartScreen(background, UIAssets.DefaultFont, _audioService);
        startScreen.LoadContent(Content);
        startScreen.GraphicsRequested += OnGraphicsRequested;
        ScreenManager.AddScreen(startScreen);
        ScreenManager.SetAudio(_audioService);
    }

    private void OnGraphicsRequested(GraphicsCommand gc)
    {
        SetResolution(gc.Width, gc.Height, _graphics.IsFullScreen = !_graphics.IsFullScreen);
    }

    protected override void Update(GameTime gameTime)
    {
        InputHandler.Update();

        ScreenManager.Update(gameTime);
        CurrentGameTime = gameTime;

        _audioService.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        ScreenManager.Draw(gameTime, SpriteBatch); // SpriteBatch wird durchgereicht
    }
}