using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.managers;

namespace BikeWars.Content.screens
{
    public class ConfirmationDialogScreen : IScreen
    {
        private Texture2D _backgroundTexture;
        private Texture2D _buttonTexture;
        private SpriteFont _font;
        private List<MenuButton> _buttons;
        private MouseState _previousMouseState;
        private string _message;
        private IScreen _previousScreen;
        
        public ScreenManager ScreenManager { get; set; }
        
        public ConfirmationDialogScreen(SpriteFont font, string message, IScreen previousScreen)
        {
            _font = font;
            _message = message;
            _previousScreen = previousScreen;
            _buttons = new List<MenuButton>();
            
            InitializeButtons();
        }
        
        private void InitializeButtons()
        {
            Game1 game = Game1.Instance;
            int screenWidth = game.GraphicsDevice.Viewport.Width;
            int screenHeight = game.GraphicsDevice.Viewport.Height;

            int buttonWidth = 150;
            int buttonHeight = 60;
            int buttonSpacing = 30;

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);
            
            int totalWidth = buttonWidth * 2 + buttonSpacing;
            int startX = (screenWidth - totalWidth) / 2;

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.ConfirmYes,
                texture: _buttonTexture,
                bounds: new Rectangle(startX, screenHeight / 2 + 80, buttonWidth, buttonHeight),
                text: "Ja",
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.ConfirmNo,
                texture: _buttonTexture,
                bounds: new Rectangle(startX + buttonWidth + buttonSpacing, screenHeight / 2 + 80, buttonWidth, buttonHeight),
                text: "Nein",
                font: _font
            ));
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
            
            foreach (var button in _buttons)
            {
                button.Update(currentMouseState);
                
                if (button.IsClicked(currentMouseState, _previousMouseState))
                {
                    HandleButtonClick(button);
                }
            }
            
            _previousMouseState = currentMouseState;
        }
        
        private void HandleButtonClick(MenuButton button)
        {
            switch ((ButtonAction)button.Id)
            {
                case ButtonAction.ConfirmYes:
                    Game1.Instance.Exit();
                    break;
            
                case ButtonAction.ConfirmNo:
                    ScreenManager.RemoveScreen(this);
                    break;
            }
        }
        
        public void Draw(GameTime gameTime)
        {
            Game1 game = Game1.Instance;
            SpriteBatch spriteBatch = game.SpriteBatch;
            
            spriteBatch.Begin();
            
            Texture2D overlay = CreateOverlayTexture(game.GraphicsDevice, 
                game.GraphicsDevice.Viewport.Width, 
                game.GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(overlay, Vector2.Zero, Color.Black * 0.7f);
            
            float messageScale = 2.0f;
            Vector2 messageSize = _font.MeasureString(_message) * messageScale;
            Vector2 messagePos = new Vector2(
                (game.GraphicsDevice.Viewport.Width - messageSize.X) / 2,
                game.GraphicsDevice.Viewport.Height / 2 - 40
            );
            
            spriteBatch.DrawString(_font, _message, messagePos, Color.White, 
                0f, Vector2.Zero, messageScale, SpriteEffects.None, 0f);
            
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
        
        public bool DrawLower => true;
        public bool UpdateLower => false;
    }
}