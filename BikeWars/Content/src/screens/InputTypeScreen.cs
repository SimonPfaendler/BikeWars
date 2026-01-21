using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.input;
using BikeWars.Content.managers;

namespace BikeWars.Content.screens;

public class InputTypeScreen: MenuScreenBase, IScreen
{
    private readonly bool _isPlayer1;
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;
    private float _uiScale;
    private float _imageScale;
    private int _imageOffsetX;
    private readonly Color _selectedColor = new Color(100, 149, 237);
    private readonly Color _defaultColor = Color.White;
    
    private Texture2D _keyboardTexture;
    private Texture2D _controllerTexture;
    
    private MenuButton _keyboardButton;
    private MenuButton _controllerButton;
    public InputTypeScreen(Texture2D background, SpriteFont font, AudioService audioService, bool isPlayer1)
        : base(background, font)
    {
        _isPlayer1 = isPlayer1;
        _audioService = audioService;
        _keyboardTexture = Game1.Instance.Content.Load<Texture2D>("assets/images/TastaturBelegung");
        _controllerTexture = Game1.Instance.Content.Load<Texture2D>("assets/images/ControllerBelegung");
        InitializeButtons();
    }
    
    protected sealed override void InitializeButtons()
    {
        // sizes of the screens components change when the screen size changes
        _buttons.Clear();

        Game1 game = Game1.Instance;
        var viewport = game.GraphicsDevice.Viewport;
        
        _uiScale = viewport.Height / 1080f;
        
        int buttonWidth = (int)(250 * _uiScale);
        int buttonHeight = (int)(60 * _uiScale);
        int verticalSpacing = (int)(20 * _uiScale);
        int horizontalSpacing = (int)(viewport.Width * 0.05f);

        int leftStartY = (int)(viewport.Height * 0.25f);

        _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, 1, 1);
        
        _keyboardButton = new MenuButton(
            id: (int)ButtonAction.Keyboard,
            texture: _buttonTexture,
            bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
            text: "Tastatur",
            font: _font,
            audioService: _audioService
        );
        
        _controllerButton = new MenuButton(
            id: (int)ButtonAction.Controller,
            texture: _buttonTexture,
            bounds: new Rectangle(horizontalSpacing, leftStartY + 2 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
            text: "Controller",
            font: _font,
            audioService: _audioService
        );
        
        _buttons.Add(_keyboardButton);
        _buttons.Add(_controllerButton);
        
        _buttons.Add(new MenuButton(
            id: (int)ButtonAction.Back,
            texture: _buttonTexture,
            bounds: new Rectangle(
                horizontalSpacing,
                leftStartY + 3 * (buttonHeight + verticalSpacing),
                buttonWidth,
                buttonHeight),
            text: "Back",
            font: _font,
            audioService: _audioService
        ));
        
        _imageScale = 0.75f * _uiScale;
        _imageOffsetX = (int)(viewport.Width * 0.07f);
        UpdateUIState();
        UpdateSelection(0);
    }
    
    private void UpdateUIState()
    {
        // show the UI_Type chosen by the Player
        ControlType current = _isPlayer1 ? InputSettings.Player1Control : InputSettings.Player2Control;
        
        // mark the selected gamemode blue
        _keyboardButton.BackgroundColor = (current == ControlType.Keyboard) ? _selectedColor : _defaultColor;
        _controllerButton.BackgroundColor = (current == ControlType.Controller) ? _selectedColor : _defaultColor;
    }
    
    public override void Draw(GameTime gameTime)
    {
        Game1 game = Game1.Instance;
        SpriteBatch spriteBatch = game.SpriteBatch;

        spriteBatch.Begin();
        
        Rectangle destinationRect = new Rectangle(
            0, 0,
            game.GraphicsDevice.Viewport.Width,
            game.GraphicsDevice.Viewport.Height);

        spriteBatch.Draw(_backgroundTexture, destinationRect, Color.White);
        
        // show the configurations of the chosen UI-Type
        ControlType current = _isPlayer1 ? InputSettings.Player1Control : InputSettings.Player2Control;
        Texture2D textureToShow = (current == ControlType.Keyboard) ? _keyboardTexture : _controllerTexture;

        // draw keyboard image
        if (textureToShow != null)
        {
            var viewport = game.GraphicsDevice.Viewport;
            
            int scaledWidth = (int)(textureToShow.Width * _imageScale);
            int scaledHeight = (int)(textureToShow.Height * _imageScale);
            

            int x = (viewport.Width - scaledWidth) / 2 + _imageOffsetX;
            int y = (viewport.Height - scaledHeight) / 2;
            
            Rectangle destRect = new Rectangle(x, y, scaledWidth, scaledHeight);

            spriteBatch.Draw(
                textureToShow,
                destRect,
                Color.White
            );
        }
        
        // draw buttons
        foreach (var button in _buttons)
        {
            button.Draw(spriteBatch);
        }

        spriteBatch.End();
    }

        
    protected override void HandleButtonClick(MenuButton button)
    {
        switch ((ButtonAction)button.Id)
        {
            case ButtonAction.Back:
                ScreenManager.RemoveScreen(this);
                break;
            
            case  ButtonAction.Controller:
                InputSettings.SetControlType(_isPlayer1, ControlType.Controller);
                UpdateUIState();
                break;
            
            case ButtonAction.Keyboard:
                InputSettings.SetControlType(_isPlayer1, ControlType.Keyboard);
                UpdateUIState();
                break;
        }
    }
    public override bool DrawLower => false;
    public override bool UpdateLower => false;
}