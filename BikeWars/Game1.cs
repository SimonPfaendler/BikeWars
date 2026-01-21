using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.managers;
using BikeWars.Content.screens;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.src.utils.SaveLoadExample;
using System;
using BikeWars.Content.events;
using System.Collections.Generic;

namespace BikeWars;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    public SpriteBatch SpriteBatch { get; private set; }
    public ScreenManager ScreenManager;
    private AudioService _audioService;
    private GameMode _selectedGameMode = GameMode.SinglePlayer;
    public static Texture2D background;
    public static GameTime CurrentGameTime { get; private set; }


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = false;

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
        MainMenuScreen mainMenu = new MainMenuScreen(background, UIAssets.DefaultFont, _audioService, GraphicsDevice.Viewport);
        mainMenu.LoadContent(Content, GraphicsDevice);
        mainMenu.BtnClicked += OnBtnClicked;
        mainMenu.GraphicsRequested += OnGraphicsRequested;
        ScreenManager.AddScreen(mainMenu);
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        RenderPrimitives.Init(GraphicsDevice);
        UIAssets.Load(Content);
        SpriteManager.LoadContent(Content);


        _audioService = new AudioService(Content);
        _audioService.LoadContent();

        background = Content.Load<Texture2D>("assets/images/Startbildschirm");
        StartScreen startScreen = new StartScreen(background, UIAssets.DefaultFont, _audioService, GraphicsDevice.Viewport);
        startScreen.LoadContent(Content, GraphicsDevice);
        startScreen.GraphicsRequested += OnGraphicsRequested;
        startScreen.BtnClicked += OnBtnClicked;
        ScreenManager.AddScreen(startScreen);
        ScreenManager.SetAudio(_audioService);
        ScreenManager.Content = Content;
        ScreenManager.GraphicsDevice = GraphicsDevice;
    }

    private void OnGraphicsRequested(GraphicsCommand gc)
    {
        SetResolution(gc.Width, gc.Height, _graphics.IsFullScreen = !_graphics.IsFullScreen);

        // Need to update all the Screens
        foreach (var screen in ScreenManager.Screens)
        {
            if (screen is MenuScreenBase menu)
            {
                menu.ViewPort = GraphicsDevice.Viewport;
                menu.OnActivated();
            }
        }
    }

    private void OnPauseBtnPressed()
    {
        PauseMenuScreen pauseMenu = new PauseMenuScreen(UIAssets.DefaultFont, _audioService, GraphicsDevice.Viewport);
        pauseMenu.LoadContent(Content, GraphicsDevice);
        pauseMenu.BtnClicked += OnBtnClicked;
        ScreenManager.AddScreen(pauseMenu);
    }

    private void OnBtnClicked(int id, IScreen screen)
    {
        switch (screen) {
            case StartScreen:
                switch ((ButtonAction)id)
                {
                    case ButtonAction.NewGame:
                        MainMenuScreen mainMenu = new MainMenuScreen(
                            background,
                            UIAssets.DefaultFont,
                            _audioService,
                            GraphicsDevice.Viewport
                        );
                        mainMenu.LoadContent(Content, GraphicsDevice);
                        mainMenu.BtnClicked += OnBtnClicked;
                        ScreenManager.RemoveScreen(screen);
                        ScreenManager.AddScreen(mainMenu);
                        break;
                }
                break;
            case MainMenuScreen:
                switch ((ButtonAction)id)
                {
                    case ButtonAction.NewGame:
                        GameConfigScreen gameConfigScreen = new GameConfigScreen(background, UIAssets.DefaultFont, _audioService, GraphicsDevice.Viewport);
                        gameConfigScreen.LoadContent(Content, GraphicsDevice);
                        gameConfigScreen.BtnClicked += OnBtnClicked;
                        ScreenManager.AddScreen(gameConfigScreen);
                        break;

                    case ButtonAction.LoadGame:
                        try
                        {
                            var loadedState = SaveLoad.LoadGame();
                            GameMode savedMode = (GameMode)loadedState.GameMode;

                            GameScreen gameScreen = new GameScreen(_audioService, savedMode);
                            gameScreen.ViewPort = GraphicsDevice.Viewport;
                            gameScreen.LoadContent(Content, GraphicsDevice);
                            gameScreen.HandleLoadGame();
                            gameScreen.BtnClicked += OnBtnClicked;
                            gameScreen.PauseBtnPressed += OnPauseBtnPressed;
                            gameScreen.GameOver += OnGameOver;
                            gameScreen.GameWon += OnGameWon;
                            gameScreen.StartTechDemo += OnStartTech;
                            ScreenManager.RemoveScreen(screen);
                            ScreenManager.AddScreen(gameScreen);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Fehler beim Laden: " + ex.Message);
                        }
                        break;

                    case ButtonAction.Profile:
                        ProfileScreen profileScreen = new ProfileScreen(background, UIAssets.DefaultFont, _audioService, GraphicsDevice.Viewport);
                        profileScreen.LoadContent(Content, GraphicsDevice);
                        profileScreen.BtnClicked += OnBtnClicked;
                        ScreenManager.AddScreen(profileScreen);
                        break;

                    case ButtonAction.Statistics:
                        StatisticsScreen ss = new StatisticsScreen(background, UIAssets.DefaultFont, _audioService, GraphicsDevice.Viewport);
                        ss.LoadContent(Content, GraphicsDevice);
                        ss.BtnClicked += OnBtnClicked;
                        ScreenManager.AddScreen(ss);
                        break;

                    case ButtonAction.TechDemo:
                        TechDemoScreen tds = new TechDemoScreen(_audioService);
                        tds.ViewPort = GraphicsDevice.Viewport;
                        tds.LoadContent(Content, GraphicsDevice);
                        tds.PauseBtnPressed += OnPauseBtnPressed;
                        tds.BtnClicked += OnBtnClicked;
                        ScreenManager.AddScreen(tds);
                        break;

                    case ButtonAction.Options:
                        _audioService.Sounds.StopAll();
                        OptionScreen optionScreen = new OptionScreen(background, UIAssets.DefaultFont, _audioService, GraphicsDevice.Viewport);
                        optionScreen.LoadContent(Content, GraphicsDevice);
                        optionScreen.BtnClicked += OnBtnClicked;
                        ScreenManager.AddScreen(optionScreen);
                        break;

                    case ButtonAction.Exit:
                        ConfirmationDialogScreen confirmDialog = new ConfirmationDialogScreen(
                            UIAssets.DefaultFont,
                            "Bist Du Dir sicher?",
                            screen,
                            _audioService,
                            GraphicsDevice.Viewport
                        );
                        confirmDialog.LoadContent(Content, GraphicsDevice);
                        confirmDialog.BtnClicked += OnBtnClicked;
                        ScreenManager.AddScreen(confirmDialog);
                        break;
                }
                break;

            case OptionScreen:
                switch ((ButtonAction)id)
                {
                    case ButtonAction.KeyBindingsPlayer1:
                        // TODO: Make KeyBindings customizable
                        break;

                    case ButtonAction.KeyBindingsPlayer2:
                        // TODO: Make KeyBindings customizable
                        break;

                    case ButtonAction.Back:
                        ScreenManager.RemoveScreen(screen);
                        break;

                    case ButtonAction.GraphicOptions:
                    {
                        GraphicsConfigScreen gcs = new GraphicsConfigScreen(background, UIAssets.DefaultFont, _audioService, GraphicsDevice.Viewport);
                        gcs.LoadContent(Content, GraphicsDevice);
                        gcs.BtnClicked += OnBtnClicked;
                        ScreenManager.AddScreen(gcs);
                        break;
                    }

                    case ButtonAction.SoundOptions:
                        SoundConfigScreen scs = new SoundConfigScreen(background, UIAssets.DefaultFont, _audioService, GraphicsDevice.Viewport);
                        scs.LoadContent(Content, GraphicsDevice);
                        scs.BtnClicked += OnBtnClicked;
                        ScreenManager.AddScreen(scs);
                        break;
                }
                break;
            case GraphicsConfigScreen:
                switch ((ButtonAction)id)
                {
                    case ButtonAction.Resolution1920x1080:
                        OnGraphicsRequested(new GraphicsCommand(1920, 1080, false));
                        break;
                    case ButtonAction.Resolution1536x864:
                        OnGraphicsRequested(new GraphicsCommand(1536, 864, false));
                        break;
                    case ButtonAction.Resolution1280x720:
                        OnGraphicsRequested(new GraphicsCommand(1280, 720, false));
                        break;
                    case ButtonAction.Resolution800x600:
                        OnGraphicsRequested(new GraphicsCommand(800, 600, false));
                        break;

                    case ButtonAction.ResolutionPortrait1080x1920:
                        OnGraphicsRequested(new GraphicsCommand(1080, 1920, false));
                        break;
                    case ButtonAction.ResolutionPortrait864x1536:
                        OnGraphicsRequested(new GraphicsCommand(864, 1536, false));
                        break;
                    case ButtonAction.ResolutionPortrait720x1280:
                        OnGraphicsRequested(new GraphicsCommand(720, 1280, false));
                        break;
                    case ButtonAction.ResolutionPortrait600x800:
                        OnGraphicsRequested(new GraphicsCommand(600, 800, false));
                        break;
                    case ButtonAction.ToggleFullscreen:
                        OnGraphicsRequested(new GraphicsCommand(0, 0, true));
                        break;
                    case ButtonAction.Back:
                        ScreenManager.RemoveScreen(screen);
                        break;
                }
                break;
            case SoundConfigScreen:
                switch ((ButtonAction)id)
                {
                    case ButtonAction.Back:
                        ScreenManager.RemoveScreen(screen);
                        break;
                }
                break;
            case StatisticsScreen:
                switch ((ButtonAction)id)
                {
                    case ButtonAction.Back:
                        ScreenManager.RemoveScreen(screen);
                        break;
                }
                break;
            case ConfirmationDialogScreen:
                switch ((ButtonAction)id)
                {
                    case ButtonAction.ConfirmYes:
                        ScreenManager.RemoveScreen(screen);
                        Exit();
                        break;
                    case ButtonAction.ConfirmNo:
                        ScreenManager.RemoveScreen(screen);
                        break;
                }
                break;
            case GameConfigScreen:
                switch ((ButtonAction)id)
                {
                    case ButtonAction.StartGame:
                        GameScreen gameScreen = new GameScreen(_audioService, _selectedGameMode);
                        gameScreen.ViewPort = GraphicsDevice.Viewport;
                        gameScreen.LoadContent(Content, GraphicsDevice);
                        gameScreen.BtnClicked += OnBtnClicked;
                        gameScreen.PauseBtnPressed += OnPauseBtnPressed;
                        gameScreen.StartTechDemo += OnStartTech;
                        gameScreen.GameWon += OnGameWon;
                        gameScreen.GameOver += OnGameOver;
                        ScreenManager.RemoveScreen(screen);
                        ScreenManager.AddScreen(gameScreen);
                        break;

                    case ButtonAction.Back:
                        ScreenManager.RemoveScreen(screen);
                        break;

                    case ButtonAction.NewProfile:
                        // TODO: Profile Creation Logic
                        break;

                    case ButtonAction.Singleplayer:
                        _selectedGameMode = GameMode.SinglePlayer;
                        break;

                    case ButtonAction.Multiplayer:
                        _selectedGameMode = GameMode.MultiPlayer;
                        break;
                }
                break;
            case PauseMenuScreen:
                switch((ButtonAction) id)
                {
                    case ButtonAction.Resume:
                        _audioService.Sounds.ResumeAll();
                        GameEvents.RaiseResumeTimer();
                        ScreenManager.RemoveScreen(screen);
                        break;

                    case ButtonAction.SaveGame:
                        // TODO: Save game logic
                        break;

                    case ButtonAction.LoadGame:
                        // TODO: Load game logic
                        break;

                    case ButtonAction.MainMenu:
                        _audioService.Sounds.StopAll();
                        _audioService.Sounds.Play(AudioAssets.SoftClick);
                        ScreenManager.ReturnToMainMenu();
                        break;

                    case ButtonAction.Options:
                        OptionScreen optionScreen = new OptionScreen(background, UIAssets.DefaultFont, _audioService, GraphicsDevice.Viewport);
                        optionScreen.LoadContent(Content, GraphicsDevice);
                        optionScreen.BtnClicked += OnBtnClicked;
                        ScreenManager.AddScreen(optionScreen);
                        break;

                    case ButtonAction.Exit:
                        ConfirmationDialogScreen confirmDialog = new ConfirmationDialogScreen(
                            UIAssets.DefaultFont,
                            "Bist Du Dir sicher?",
                            screen,
                            _audioService,
                            GraphicsDevice.Viewport
                        );
                        confirmDialog.LoadContent(Content, GraphicsDevice);
                        confirmDialog.BtnClicked += OnBtnClicked;
                        ScreenManager.AddScreen(confirmDialog);
                        break;
                }
                break;
            case GameOverScreen:
                switch ((ButtonAction)id)
                {
                    case ButtonAction.MainMenu:
                        _audioService.Sounds.StopAll();
                        _audioService.Sounds.Play(AudioAssets.SoftClick);
                        ScreenManager.ReturnToMainMenu();
                        break;

                    case ButtonAction.Exit:
                        ConfirmationDialogScreen confirmDialog = new ConfirmationDialogScreen(
                            UIAssets.DefaultFont,
                            "Bist Du Dir sicher?",
                            screen,
                            _audioService,
                            GraphicsDevice.Viewport
                        );
                        confirmDialog.LoadContent(Content, GraphicsDevice);
                        confirmDialog.BtnClicked += OnBtnClicked;
                        ScreenManager.AddScreen(confirmDialog);
                        break;
                }
                break;
            case GameWonScreen:
                switch ((ButtonAction)id)
                {
                    case ButtonAction.MainMenu:
                        _audioService.Sounds.StopAll();
                        _audioService.Sounds.Play(AudioAssets.SoftClick);
                        ScreenManager.ReturnToMainMenu();
                        break;

                    case ButtonAction.Exit:
                        ConfirmationDialogScreen confirmDialog = new ConfirmationDialogScreen(
                            UIAssets.DefaultFont,
                            "Bist Du Dir sicher?",
                            screen,
                            _audioService,
                            GraphicsDevice.Viewport
                        );
                        confirmDialog.LoadContent(Content, GraphicsDevice);
                        confirmDialog.BtnClicked += OnBtnClicked;
                        ScreenManager.AddScreen(confirmDialog);
                        break;
                }
                break;
        }
    }

    private void OnStartTech(IScreen screen)
    {
        TechDemoScreen tds = new TechDemoScreen(_audioService);
        tds.ViewPort = GraphicsDevice.Viewport;
        tds.LoadContent(Content, GraphicsDevice);
        tds.PauseBtnPressed += OnPauseBtnPressed;
        tds.BtnClicked += OnBtnClicked;
        ScreenManager.RemoveScreen(screen);
        ScreenManager.AddScreen(tds);
    }

    private void OnGameOver(Statistic statistic)
    {
        GameOverScreen gos = new GameOverScreen(UIAssets.DefaultFont, _audioService, statistic, GraphicsDevice.Viewport);
        gos.LoadContent(Content, GraphicsDevice);
        gos.BtnClicked += OnBtnClicked;
        ScreenManager.AddScreen(gos);
    }

    private void OnGameWon(Statistic statistic)
    {
        GameWonScreen gws = new GameWonScreen(UIAssets.DefaultFont, _audioService, statistic, GraphicsDevice.Viewport);
        gws.LoadContent(Content, GraphicsDevice);
        gws.BtnClicked += OnBtnClicked;
        ScreenManager.AddScreen(gws);
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
        ScreenManager.Draw(gameTime, SpriteBatch);
    }
}