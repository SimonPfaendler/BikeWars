using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.managers;

namespace BikeWars.Content.screens
{
    public class ConfirmationDialogScreen : MenuScreenBase, IScreen
    {
        private string _message;
        private IScreen _previousScreen;
        
        public ConfirmationDialogScreen(SpriteFont font, string message, IScreen previousScreen)
            :base(null, font)
        {
            _message = message;
            _previousScreen = previousScreen;
        }
        
        protected override void InitializeButtons()
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
        
        protected override void HandleButtonClick(MenuButton button)
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
        
        public override void Draw(GameTime gameTime)
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
        public override bool DrawLower => true;
        public override bool UpdateLower => false;
    }
}