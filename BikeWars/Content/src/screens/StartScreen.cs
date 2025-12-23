using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.engine.Audio;

namespace BikeWars.Content.screens;
// the first screen that pops up when the Game is started
public class StartScreen : MenuScreenBase, IScreen
{
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;
    
    public StartScreen(Texture2D background, SpriteFont font, AudioService audioService)
        : base(background, font)
    {
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        InitializeButtons();
    }

    
    protected sealed override void InitializeButtons()
    {
        Game1 game = Game1.Instance;

        int buttonWidth = 250;
        int buttonHeight = 80;

        Rectangle buttonBounds = new Rectangle(
            (game.GraphicsDevice.Viewport.Width - buttonWidth) / 2,
            (game.GraphicsDevice.Viewport.Height - buttonHeight) / 3,
            buttonWidth,
            buttonHeight
        );

        _buttonTexture = CreateSimpleTexture(
            game.GraphicsDevice,
            buttonWidth,
            buttonHeight
        );

        _buttons.Add(new MenuButton(
            id: (int)ButtonAction.NewGame,
            texture: _buttonTexture,
            bounds: buttonBounds,
            text: "Start",
            font: _font,
            audioService: _audioService
        ));

        UpdateSelection(0);
    }
    
    protected override void HandleButtonClick(MenuButton button)
    {
        switch ((ButtonAction)button.Id)
        {
            case ButtonAction.NewGame:
                MainMenuScreen mainMenu = new MainMenuScreen(
                    _backgroundTexture,
                    _font,
                    _audioService
                );
                ScreenManager.RemoveScreen(this);
                ScreenManager.AddScreen(mainMenu);
                break;
        }
    }

    
    public override bool DrawLower => false;
    public override bool UpdateLower => false;
}