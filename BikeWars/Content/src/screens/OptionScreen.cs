using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens;

public class OptionScreen : MenuScreenBase, IScreen
{
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;

    public OptionScreen(Texture2D background, SpriteFont font, AudioService audioService, Viewport vp)
        : base(background, font, vp)
    {
        _audioService = audioService;
    }
    public override void LoadContent(ContentManager content, GraphicsDevice gd)
    {
        base.LoadContent(content, gd);
        InitializeButtons();
    }

    protected sealed override void InitializeButtons()
    {
        int screenWidth = ViewPort.Width;
        int screenHeight = ViewPort.Height;

        int buttonWidth = 250;
        int buttonHeight = 60;
        int verticalSpacing = 20;
        int horizontalSpacing = screenWidth / 15;

        int leftStartY = screenHeight / 7;
        int rightStartY = screenHeight / 7;

        // Buttons on the left side
        AddButton(new MenuButton(
            id: (int)ButtonAction.KeyBindingsPlayer1,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(horizontalSpacing, leftStartY, buttonWidth, buttonHeight),
            text: "Steuerung Spieler 1",
            font: _font,
            audioService: _audioService
        ));

        AddButton(new MenuButton(
            id: (int)ButtonAction.KeyBindingsPlayer2,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
            text: "Steuerung Spieler 2",
            font: _font,
            audioService: _audioService
        ));

        AddButton(new MenuButton(
            id: (int)ButtonAction.Back,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(horizontalSpacing, leftStartY + 3 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
            text: "Back",
            font: _font,
            audioService: _audioService
        ));

        // Buttons on the right side
        AddButton(new MenuButton(
            id: (int)ButtonAction.GraphicOptions,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY, buttonWidth, buttonHeight),
            text: "Grafikeinstellungen",
            font: _font,
            audioService: _audioService
        ));

        AddButton(new MenuButton(
            id: (int)ButtonAction.SoundOptions,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
            text: "Soundeinstellungen",
            font: _font,
            audioService: _audioService
        ));

        UpdateSelection(0);
    }

    public override bool DrawLower => false;
    public override bool UpdateLower => false;
}