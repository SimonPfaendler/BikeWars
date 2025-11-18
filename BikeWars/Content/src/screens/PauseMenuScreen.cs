using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace BikeWars.Content.screens
{
    public class PauseMenuScreen : IScreen
    {
        private Texture2D _backgroundTexture;
        private Texture2D _buttonTexture;
        private SpriteFont _font;
        private List<MenuButton> _buttons;
        private MouseState _previousMouseState;
        
        public PauseMenuScreen(SpriteFont font)
        {
            _font = font;
            _buttons = new List<MenuButton>();
            
            InitializeButtons();
        }
        
        private void InitializeButtons()
        {
            Game1 game = Game1.Instance;
            int screenWidth = game.GraphicsDevice.Viewport.Width;
            int screenHeight = game.GraphicsDevice.Viewport.Height;
            
            // Positioning logic for buttons
            int buttonWidth = 300;
            int buttonHeight = 60;
            
            int startY = screenHeight / 4;
            int verticalSpacing = 20;
            
            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);
            
            string[] buttonTexts = {
                "Resume Game",
                "Save Game", 
                "Load Game",
                "Main Menu",
                "Options",
                "Quit Game"
            };
            
            for (int i = 0; i < buttonTexts.Length; i++)
            {
                _buttons.Add(new MenuButton(
                    texture: _buttonTexture,
                    bounds: new Rectangle((screenWidth - buttonWidth) / 2, startY + i * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                    text: buttonTexts[i],
                    font: _font
                ));
            }
        }
        
        private Texture2D CreateSimpleTexture(GraphicsDevice graphicsDevice, int width, int height)
        {
            // screen gets darker when game is paused
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++) 
                data[i] = Color.DarkGray;
            texture.SetData(data);
            return texture;
        }
        
        public void Update(GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();
            
            foreach (var button in _buttons)
            {
                if (button.IsClicked(currentMouseState, _previousMouseState))
                {
                    HandleButtonClick(button);
                }
            }
            
            _previousMouseState = currentMouseState;
        }
        
        private void HandleButtonClick(MenuButton button)
        {
            switch (button.Text)
            {
                case "Resume Game":
                    Game1.Instance.ScreenManager.RemoveScreen(this);
                    break;
                    
                case "Save Game":
                    // TODO: Save game logic
                    break;
                    
                case "Load Game":
                    // TODO: Load game logic  
                    break;
                    
                case "Main Menu":
                    // TODO: Go to Main Menu
                    break;
                    
                case "Options":
                    // TODO: Open Options Menu
                    break;
                    
                case "Quit Game":
                    Game1.Instance.Exit();
                    break;
            }
        }
        
        public void Draw(GameTime gameTime)
        {
            Game1 game = Game1.Instance;
            SpriteBatch spriteBatch = game.SpriteBatch;
            
            spriteBatch.Begin();
            
            Texture2D overlay = CreateOverlayTexture(game.GraphicsDevice, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(overlay, Vector2.Zero, Color.White * 0.7f);
            
            foreach (var button in _buttons)
            {
                button.Draw(spriteBatch);
            }
            
            spriteBatch.End();
        }
        
        private Texture2D CreateOverlayTexture(GraphicsDevice graphicsDevice, int width, int height)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++) 
                data[i] = Color.Black;
            texture.SetData(data);
            return texture;
        }
        
        public bool DrawLower => true;    // GameScreen gets drawn
        public bool UpdateLower => false; // GameScreen doesn't get updated
    }
}