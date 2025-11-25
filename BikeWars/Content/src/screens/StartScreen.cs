using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.engine.Audio;

namespace BikeWars.Content.screens;
// the first screen that pops up when the Game is started
public class StartScreen : IScreen
{
    private Texture2D _backgroundTexture;
    private Texture2D _buttonTexture;
    private MenuButton _startButton;
    private MouseState _previousMouseState;
    private SpriteFont _font;
    private readonly AudioService _audioService;
    
    public ScreenManager ScreenManager { get; set; }
    
    public StartScreen(Texture2D background, AudioService audioService)
    {
        _backgroundTexture = background;
        _font = Game1.Instance.Content.Load<SpriteFont>("assets/fonts/Arial");
        _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
        
        InitializeButton();
    }
    
    private void InitializeButton()
    {
        Game1 game = Game1.Instance;
        
        int buttonWidth = 250;
        int buttonHeight = 80;
        
        Rectangle buttonBounds = new Rectangle(
            (game.GraphicsDevice.Viewport.Width - buttonWidth) / 2,
            (game.GraphicsDevice.Viewport.Height - buttonHeight) / 3,
            buttonWidth,
            buttonHeight
        );
        
        _buttonTexture = CreateSimpleTexture(Game1.Instance.GraphicsDevice, buttonWidth, buttonHeight);
        
        _startButton = new MenuButton(
            id: (int)ButtonAction.NewGame,
            texture: _buttonTexture,
            bounds: buttonBounds,
            text: "Start",
            font: _font,
            textColor: Color.Black,
            audioService: _audioService
        );
    }
    
    private Texture2D CreateSimpleTexture(GraphicsDevice graphicsDevice, int width, int height)
    {
        Texture2D texture = new Texture2D(graphicsDevice, width, height);
        Color[] data = new Color[width * height];
        for (int i = 0; i < data.Length; i++) 
            data[i] = Color.White;
        texture.SetData(data);
        return texture;
    }
    
    public void Update(GameTime gameTime)
    {
        MouseState currentMouseState = Mouse.GetState();
        
        _startButton.Update(currentMouseState);
        
        if (_startButton.IsClicked(currentMouseState, _previousMouseState))
        {
            MainMenuScreen mainMenu = new MainMenuScreen(_backgroundTexture, _font, _audioService);
            ScreenManager.RemoveScreen(this);
            ScreenManager.AddScreen(mainMenu);
        }
    
        _previousMouseState = currentMouseState;
    }

    public void Draw(GameTime gameTime)
    {
        Game1 game = Game1.Instance;
        SpriteBatch spriteBatch = game.SpriteBatch;
    
        spriteBatch.Begin();
        
        Rectangle destinationRectangle = new Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
        spriteBatch.Draw(_backgroundTexture, destinationRectangle, Color.White);
        
        _startButton.Draw(spriteBatch);
    
        spriteBatch.End();
    }
    
    public bool DrawLower => false;
    public bool UpdateLower => false;
}