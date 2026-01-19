using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.managers;
using BikeWars.Content.screens;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;

namespace BikeWars;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    public SpriteBatch SpriteBatch { get; private set; }
    public ScreenManager ScreenManager;
    private AudioService _audioService;
    public static AudioService Audio => Instance._audioService;
    public static Game1 Instance { get; private set; }
    public static Texture2D background;
    public static GameTime CurrentGameTime { get; private set; }


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Instance = this;

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
        SpriteFont font = Content.Load<SpriteFont>("assets/fonts/Arial");
        MainMenuScreen mainMenu = new MainMenuScreen(background, font, _audioService);
        ScreenManager.AddScreen(mainMenu);
    }

    protected override void LoadContent()
    {
        this.SpriteBatch = new SpriteBatch(GraphicsDevice);

        _audioService = new AudioService();
        _audioService.LoadContent(Content);

        background = Content.Load<Texture2D>("assets/images/Startbildschirm");

        SpriteManager.LoadContent(Content);


        SpriteFont font = Content.Load<SpriteFont>("assets/fonts/Arial");

        StartScreen startScreen = new StartScreen(
            background,
            font,
            _audioService
        );

        ScreenManager.AddScreen(startScreen);

        ScreenManager.SetAudio(_audioService);
    }

    protected override void Update(GameTime gameTime)
    {
        IsFixedTimeStep = false;
        InputHandler.Update();

        ScreenManager.Update(gameTime);
        CurrentGameTime = gameTime;

        _audioService.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        ScreenManager.Draw(gameTime);
        base.Draw(gameTime);
    }
}