using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BikeWars.Content.managers;
using BikeWars.Content.screens;

namespace BikeWars;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    public SpriteBatch SpriteBatch { get; private set; }
    public ScreenManager ScreenManager;
    public static Game1 Instance { get; private set; }

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Instance = this;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
    }

    protected override void Initialize()
    {
        ScreenManager = new ScreenManager();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        this.SpriteBatch = new SpriteBatch(GraphicsDevice);
        
        Texture2D background = Content.Load<Texture2D>("assets/images/Startbildschirm");
        Texture2D button = Content.Load<Texture2D>("assets/images/StartButton");
        StartScreen startScreen = new StartScreen(background, button);
        ScreenManager.AddScreen(startScreen);
    }

    protected override void Update(GameTime gameTime)
    {
        ScreenManager.Update(gameTime);
        
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        ScreenManager.Draw(gameTime);
        base.Draw(gameTime);
    }
}