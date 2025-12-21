using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;

namespace BikeWars.Content.screens;

public class OptionScreen : MenuScreenBase, IScreen
{
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;
    public OptionScreen(Texture2D background, SpriteFont font, AudioService audioService)
        : base(background, font)
    {
        _audioService = audioService;
        InitializeButtons();
    }
    
    protected sealed override void InitializeButtons()
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
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.KeyBindingsPlayer2,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Key-Bindings Spieler 1", 
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Back,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + 3 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Back",
                font: _font,
                audioService: _audioService
            ));

            // Buttons on the right side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.GraphicOptions,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY, buttonWidth, buttonHeight),
                text: "Grafikeinstellungen",
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.SoundOptions,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Soundeinstellungen",
                font: _font,
                audioService: _audioService
            ));
            
            UpdateSelection(0);
        }

        
    protected override void HandleButtonClick(MenuButton button)
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
    public override bool DrawLower => false;
    public override bool UpdateLower => false;
}