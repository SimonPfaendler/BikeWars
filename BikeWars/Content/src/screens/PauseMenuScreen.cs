using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.managers;
using BikeWars.Content.src.screens.Overlay;

namespace BikeWars.Content.screens
{
    public class PauseMenuScreen : IScreen
    {
        private Texture2D _backgroundTexture;
        private Texture2D _buttonTexture;
        private SpriteFont _font;
        private List<MenuButton> _buttons;
        private MouseState _previousMouseState;
        private Overlay _overlay; 
        
        public ScreenManager ScreenManager { get; set; }
        
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
    
            int buttonWidth = 300;
            int buttonHeight = 60;
    
            int startY = screenHeight / 4;
            int verticalSpacing = 20;

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);
            
            var buttonDefinitions = new[]
            {
                (id: ButtonAction.Resume, text: "Spiel fortsetzen"),
                (id: ButtonAction.SaveGame, text: "Spiel speichern"),
                (id: ButtonAction.LoadGame, text: "Spiel laden"),
                (id: ButtonAction.MainMenu, text: "Hauptmenu"),
                (id: ButtonAction.Options, text: "Optionen"),
                (id: ButtonAction.Exit, text: "Spiel beenden")
            };
    
            for (int i = 0; i < buttonDefinitions.Length; i++)
            {
                _buttons.Add(new MenuButton(
                    id: (int)buttonDefinitions[i].id,
                    texture: _buttonTexture,
                    bounds: new Rectangle((screenWidth - buttonWidth) / 2, startY + i * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                    text: buttonDefinitions[i].text,
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
                    HandleButtonClick(button, gameTime);
                }
            }
            
            _previousMouseState = currentMouseState;
        }
        
        private void HandleButtonClick(MenuButton button, GameTime gameTime)
        {
            switch ((ButtonAction)button.Id)
            {
                case ButtonAction.Resume:
                    ScreenManager.RemoveScreen(this);
                    break;
            
                case ButtonAction.SaveGame:
                    // TODO: Save game logic
                    break;
            
                case ButtonAction.LoadGame:
                    // TODO: Load game logic  
                    break;
            
                case ButtonAction.MainMenu:
                    ScreenManager.ReturnToMainMenu();
                    break;
            
                case ButtonAction.Options:
                    // TODO: Optionsmenü öffnen
                    break;
            
                case ButtonAction.Exit:
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