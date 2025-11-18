using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.managers;

namespace BikeWars.Content.screens;

public class OptionScreen : IScreen
{
    private Texture2D _backgroundTexture;
    private Texture2D _buttonTexture;
    private SpriteFont _font;
    private List<MenuButton> _buttons;
    private MouseState _previousMouseState;
    
    public ScreenManager ScreenManager { get; set; }
    
    public OptionScreen(SpriteFont font)
    {
        _backgroundTexture = Game1.Instance.Content.Load<Texture2D>("assets/images/Startbildschirm");;
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
            
            int leftStartY = screenHeight / 7;
            int rightStartY = screenHeight / 7;

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);

            // Buttons on the left side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.KeyBindingsPlayer1,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY, buttonWidth, buttonHeight),
                text: "Key-Bindings Spieler 1",
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.KeyBindingsPlayer2,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Key-Bindings Spieler 1", 
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Back,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + 3 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Back",
                font: _font
            ));

            // Buttons on the right side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.GraphicOptions,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY, buttonWidth, buttonHeight),
                text: "Grafikeinstellungen",
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.SoundOptions,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Soundeinstellungen",
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
            case ButtonAction.KeyBindingsPlayer1:
                // TODO: Make KeyBindings customizable
                break;
            
            case ButtonAction.KeyBindingsPlayer2:
                // TODO: Make KeyBindings customizable
                break;
            
            case ButtonAction.Back:
                ScreenManager.RemoveScreen(this);
                break;

            case ButtonAction.GraphicOptions:
                // TODO: open graphic option screen
                break;

            case ButtonAction.SoundOptions:
                // TODO: open Sound options screen
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