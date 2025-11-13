using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BikeWars.Content.screens;
// the first screen that gets displayed when the game is started

public class StartScreen : IScreen
{
    private Texture2D _backgroundTexture;
    private Texture2D _buttonTexture;
    private Rectangle _buttonRectangle;
    private MouseState _previousMouseState;
    
    public StartScreen(Texture2D background, Texture2D button)
    {
        _backgroundTexture = background;
        _buttonTexture = button;
    
        // Button-Position SOFORT berechnen
        Game1 game = Game1.Instance;
        int buttonWidth = game.GraphicsDevice.Viewport.Width / 2;
        int buttonHeight = (int)(buttonWidth * ((float)button.Height / button.Width));
    
        _buttonRectangle = new Rectangle(
            (game.GraphicsDevice.Viewport.Width - buttonWidth) / 2,
            (game.GraphicsDevice.Viewport.Height - buttonHeight) / 5,
            buttonWidth,
            buttonHeight
        );
    }
    
    public void Update(GameTime gameTime)
    {
        MouseState currentMouseState = Mouse.GetState();
    
        // Prüfe ob Linke Maustaste gedrückt wurde
        if (_previousMouseState.LeftButton == ButtonState.Released && 
            currentMouseState.LeftButton == ButtonState.Pressed)
        {
            // Prüfe ob Klick innerhalb des Buttons
            if (_buttonRectangle.Contains(currentMouseState.Position))
            {
                // GameScreen erstellen und hinzufügen
                GameScreen gameScreen = new GameScreen();
                gameScreen.LoadContent(Game1.Instance.Content);
            
                // StartScreen entfernen und GameScreen hinzufügen
                Game1.Instance.ScreenManager.RemoveScreen(this);
                Game1.Instance.ScreenManager.AddScreen(gameScreen);
            }
        }
    
        _previousMouseState = currentMouseState;
    }

    public void Draw(GameTime gameTime)
    {
        Game1 game = Game1.Instance;
        SpriteBatch spriteBatch = game.SpriteBatch;
    
        spriteBatch.Begin();
    
        // Hintergrund
        Rectangle destinationRectangle = new Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
        spriteBatch.Draw(_backgroundTexture, destinationRectangle, Color.White);
    
        // Button mit bereits berechnetem Rectangle
        spriteBatch.Draw(_buttonTexture, _buttonRectangle, Color.White);
    
        spriteBatch.End();
    }

    public bool DrawLower => false;
    
    public bool UpdateLower => false;
}