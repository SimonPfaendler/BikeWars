using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.managers;

namespace BikeWars.Content.screens
{
    public abstract class ScreenBase : IScreen
    {
        protected Texture2D _backgroundTexture;
        protected Texture2D _buttonTexture;
        protected SpriteFont _font;
        protected List<MenuButton> _buttons;
        protected MouseState _previousMouseState;
        protected GameTime _currentGameTime;
        public ScreenManager ScreenManager { get; set; }
        
        protected ScreenBase(Texture2D background, SpriteFont font)
        {
            _backgroundTexture = background;
            _font = font;
            _buttons = new List<MenuButton>();
            
            InitializeButtons();
        }
        
        protected abstract void InitializeButtons();

        public virtual void Update(GameTime gameTime)
        {
            _currentGameTime = gameTime;

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
        
        public virtual void Draw(GameTime gameTime)
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
        
        // Every screen has to handle their own button clicks
        protected abstract void HandleButtonClick(MenuButton button);

        public virtual bool DrawLower => false;
        public virtual bool UpdateLower => false;
    }
}