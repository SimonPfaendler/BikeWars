using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BikeWars.Content.screens;
// the first screen that pops up when the Game is started
public class StartScreen : IScreen
{
    private Texture2D _backgroundTexture;
    private Texture2D _buttonTexture;
    private Rectangle _buttonRectangle;
    private MouseState _previousMouseState;
    private SpriteFont _font;
    
    public ScreenManager ScreenManager { get; set; }
    
    public StartScreen(Texture2D background)
    {
        _backgroundTexture = background;
        
        // Calculate Size and Position of Start Button
        Game1 game = Game1.Instance;
        
        int buttonWidth = 300;
        int buttonHeight = 100;
    
        _buttonRectangle = new Rectangle(
            (game.GraphicsDevice.Viewport.Width - buttonWidth) / 2,
            (game.GraphicsDevice.Viewport.Height - buttonHeight) / 3,
            buttonWidth,
            buttonHeight
        );
        
        _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);
        
        _font = Game1.Instance.Content.Load<SpriteFont>("assets/fonts/Arial");
    }
    
    // Button Texture might be changed later
    private Texture2D CreateSimpleTexture(GraphicsDevice graphicsDevice, int width, int height)
    {
        Texture2D texture = new Texture2D(graphicsDevice, width, height);
        Color[] data = new Color[width * height];
        for (int i = 0; i < data.Length; i++) 
            data[i] = Color.LightGray;
        texture.SetData(data);
        return texture;
    }
    
    public void Update(GameTime gameTime)
    {
        MouseState currentMouseState = Mouse.GetState();
    
        // Change to MainMenuScreen, if button is clicked
        if (_previousMouseState.LeftButton == ButtonState.Released && 
            currentMouseState.LeftButton == ButtonState.Pressed)
        {
            if (_buttonRectangle.Contains(currentMouseState.Position))
            {
                
                MainMenuScreen mainMenu = new MainMenuScreen(_backgroundTexture, _font);
                ScreenManager.RemoveScreen(this);
                ScreenManager.AddScreen(mainMenu);
            }
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
        
        spriteBatch.Draw(_buttonTexture, _buttonRectangle, Color.White);
        
        string buttonText = "Start";
        Vector2 textSize = _font.MeasureString(buttonText);
        float scale = 2.0f;
        Vector2 scaledTextSize = textSize * scale;
        
        Vector2 textPosition = new Vector2(
            _buttonRectangle.X + (_buttonRectangle.Width - scaledTextSize.X) / 2,
            _buttonRectangle.Y + (_buttonRectangle.Height - scaledTextSize.Y) / 2
        );
        
        spriteBatch.DrawString(_font, buttonText, textPosition, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    
        spriteBatch.End();
    }
    
    public bool DrawLower => false;
    public bool UpdateLower => false;
}