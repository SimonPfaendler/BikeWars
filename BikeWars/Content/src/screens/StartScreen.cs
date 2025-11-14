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
    
        // Calculate Size and Position of Start Button
        Game1 game = Game1.Instance;
        int buttonWidth = game.GraphicsDevice.Viewport.Width / 2;
        int buttonHeight = (int)(buttonWidth * ((float)button.Height / button.Width));
    
        // Button is represented as a rectangle even though it looks like an ellipse (for simplicity)
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
    
        // Change to GameScreen (for now might be changed later), if button is clicked
        if (_previousMouseState.LeftButton == ButtonState.Released && 
            currentMouseState.LeftButton == ButtonState.Pressed)
        {
            if (_buttonRectangle.Contains(currentMouseState.Position))
            {
                GameScreen gameScreen = new GameScreen();
                gameScreen.LoadContent(Game1.Instance.Content);
                
                Game1.Instance.ScreenManager.RemoveScreen(this);
                Game1.Instance.ScreenManager.AddScreen(gameScreen);
            }
        }
    
        _previousMouseState = currentMouseState;
    }

    public void Draw(GameTime gameTime)
    {
        // Draw background picture and Start-Button
        Game1 game = Game1.Instance;
        SpriteBatch spriteBatch = game.SpriteBatch;
    
        spriteBatch.Begin();
        
        Rectangle destinationRectangle = new Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
        spriteBatch.Draw(_backgroundTexture, destinationRectangle, Color.White);
        
        spriteBatch.Draw(_buttonTexture, _buttonRectangle, Color.White);
    
        spriteBatch.End();
    }
    
    // set both Drawlower and UpdateLower to false, there simply cannot exist a screen under the Start Screen
    public bool DrawLower => false;
    
    public bool UpdateLower => false;
}