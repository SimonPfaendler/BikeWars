using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens;
// the first screen that pops up when the Game is started
public class StartScreen : MenuScreenBase, IScreen
{
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;
    public event Action<GraphicsCommand> GraphicsRequested;
    public StartScreen(Texture2D background, SpriteFont font, AudioService audioService, Viewport vp)
        : base(background, font, vp)
    {
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
    }

    public override void LoadContent(ContentManager content, GraphicsDevice gd)
    {
        base.LoadContent(content, gd);
        InitializeButtons();
    }

    protected sealed override void InitializeButtons()
    {
        int buttonWidth = 250;
        int buttonHeight = 80;

        Rectangle buttonBounds = new Rectangle(
            (ViewPort.Width - buttonWidth) / 2,
            (ViewPort.Height - buttonHeight) / 3,
            buttonWidth,
            buttonHeight
        );

        AddButton(new MenuButton(
            id: (int)ButtonAction.NewGame,
            texture: RenderPrimitives.Pixel,
            bounds: buttonBounds,
            text: "Start",
            font: _font,
            audioService: _audioService
        ));
        UpdateSelection(0);
    }
    public override bool DrawLower => false;
    public override bool UpdateLower => false;
}