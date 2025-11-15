using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace BikeWars.Content.screens
{
    public class MainMenuScreen : IScreen
    {
        private Texture2D _backgroundTexture;
        private Texture2D _buttonTexture;
        private SpriteFont _font;
        private List<MenuButton> _buttons;
        private MouseState _previousMouseState;
        
        public MainMenuScreen(Texture2D background, SpriteFont font)
        {
            _backgroundTexture = background;
            _font = font;
            _buttons = new List<MenuButton>();
            
            InitializeButtons();
        }
        
        private void InitializeButtons()
        {
            Game1 game = Game1.Instance;
            int screenWidth = game.GraphicsDevice.Viewport.Width;
            int screenHeight = game.GraphicsDevice.Viewport.Height;
            
            int buttonWidth = 250;
            int buttonHeight = 80;
    
            int startY = screenHeight / 6;
            int horizontalSpacing = screenWidth / 15;
            int verticalSpacing = 30;
            
            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);
            
            // Buttons on the left side
            _buttons.Add(new MenuButton(
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, startY, buttonWidth, buttonHeight),
                text: "Neues Spiel",
                font: _font
            ));

            _buttons.Add(new MenuButton(
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, startY + buttonHeight + verticalSpacing, buttonWidth, buttonHeight),
                text: "Spiel laden", 
                font: _font
            ));

            // Buttons on the right side 
            _buttons.Add(new MenuButton(
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, startY, buttonWidth, buttonHeight),
                text: "Profil",
                font: _font
            ));

            _buttons.Add(new MenuButton(
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, startY + buttonHeight + verticalSpacing, buttonWidth, buttonHeight),
                text: "Beenden",
                font: _font
            ));
        }
        
        // CreateSimpleTexture might be changed for a better graphic later
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
                case "Neues Spiel":
                    // Change to GameScreen
                    GameScreen gameScreen = new GameScreen();
                    gameScreen.LoadContent(Game1.Instance.Content);
                    Game1.Instance.ScreenManager.RemoveScreen(this);
                    Game1.Instance.ScreenManager.AddScreen(gameScreen);
                    break;
                    
                case "Spiel laden":
                    // TODO: needs to be done sometime
                    break;
                
                case "Profil":
                    // TODO: needs to be done sometime
                    break;
                
                case "Beenden":
                    Game1.Instance.Exit();
                    break;
            }
        }
        
        public void Draw(GameTime gameTime)
        {
            Game1 game = Game1.Instance;
            SpriteBatch spriteBatch = game.SpriteBatch;
            
            spriteBatch.Begin();
            
            Rectangle destinationRect = new Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(_backgroundTexture, destinationRect, Color.White);
            
            foreach (var button in _buttons)
            {
                button.Draw(spriteBatch);
            }
            
            spriteBatch.End();
        }
        
        public bool DrawLower => false;
        public bool UpdateLower => false;
    }
}