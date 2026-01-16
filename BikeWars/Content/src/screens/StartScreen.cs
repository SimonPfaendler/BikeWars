using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using MonoGame.Extended.Content;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens;
// the first screen that pops up when the Game is started
public class StartScreen : MenuScreenBase, IScreen
{
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;
    public event Action<GraphicsCommand> GraphicsRequested;


    public StartScreen(Texture2D background, SpriteFont font, AudioService audioService)
        : base(background, font)
    {
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
    }


    public override void LoadContent(ContentManager contentManager)
    {
        base.LoadContent(contentManager);
        InitializeButtons();
    }


    protected sealed override void InitializeButtons()
    {
        int buttonWidth = 250;
        int buttonHeight = 80;

        Rectangle buttonBounds = new Rectangle(
            (Content.GetGraphicsDevice().Viewport.Width - buttonWidth) / 2,
            (Content.GetGraphicsDevice().Viewport.Height - buttonHeight) / 3,
            buttonWidth,
            buttonHeight
        );

        _buttonTexture = CreateSimpleTexture(
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
                mainMenu.LoadContent(Content);
                mainMenu.GraphicsRequested += Forward;
                ScreenManager.RemoveScreen(this);
                ScreenManager.AddScreen(mainMenu);
                break;
        }
    }

    private void Forward(GraphicsCommand cmd)
    {
        GraphicsRequested?.Invoke(cmd);
    }
    public override bool DrawLower => false;
    public override bool UpdateLower => false;
}