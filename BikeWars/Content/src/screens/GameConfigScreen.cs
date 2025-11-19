using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.managers;

namespace BikeWars.Content.screens;

public class GameConfigScreen : IScreen
{
    private Texture2D _backgroundTexture;
    private Texture2D _buttonTexture;
    private SpriteFont _font;
    private List<MenuButton> _buttons;
    private MouseState _previousMouseState;
        
    public ScreenManager ScreenManager { get; set; }
        
    public GameConfigScreen(Texture2D background, SpriteFont font)
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
            int buttonHeight = 60;
            int verticalSpacing = 20;
            int horizontalSpacing = screenWidth / 15;
            
            int leftStartY = screenHeight / 4;
            int rightStartY = screenHeight / 4;

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);

            // Buttons on the left side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.NewProfile,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY, buttonWidth, buttonHeight),
                text: "Neues Profil",
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Back,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Back", 
                font: _font
            ));

            // Buttons on the right side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Singleplayer,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY, buttonWidth, buttonHeight),
                text: "Singleplayer",
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Multiplayer,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Multiplayer",
                font: _font
            ));
            
            // centre start Button
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.StartGame,
                texture: _buttonTexture,
                bounds: new Rectangle(
                    (screenWidth - buttonWidth) / 2,
                    screenHeight / 3 - buttonHeight / 2,
                    buttonWidth,
                    buttonHeight
                ),
                text: "Spiel starten",
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
                case ButtonAction.StartGame:
                    GameScreen gameScreen = new GameScreen();
                    gameScreen.LoadContent(Game1.Instance.Content);
                    ScreenManager.RemoveScreen(this);
                    ScreenManager.AddScreen(gameScreen);
                    break;
            
                case ButtonAction.Back:
                    ScreenManager.RemoveScreen(this);
                    break;
            
                case ButtonAction.NewProfile:
                    // TODO: Profile Creation Logic
                    break;

                case ButtonAction.Singleplayer:
                    // TODO: Choose Singleplayer Gamemode
                    break;

                case ButtonAction.Multiplayer:
                    // TODO: Choose Multiplayer Gamemode
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