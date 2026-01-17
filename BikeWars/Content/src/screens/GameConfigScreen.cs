using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens;

public class GameConfigScreen : MenuScreenBase, IScreen
{
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;
    private GameMode _selectedGameMode = GameMode.SinglePlayer;
    private MenuButton _singleplayerButton;
    private MenuButton _multiplayerButton;

    private readonly Color _selectedColor = new Color(100, 149, 237);
    private readonly Color _defaultColor = Color.White;


    public GameConfigScreen(Texture2D background, SpriteFont font, AudioService audioService, Viewport vp)
        :base(background, font, vp)
    {
        _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
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

        int leftStartY = screenHeight / 4;
        int rightStartY = screenHeight / 4;

        // _buttonTexture = CreateSimpleTexture(buttonWidth, buttonHeight);

        // Buttons on the left side
        AddButton(new MenuButton(
            id: (int)ButtonAction.NewProfile,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(horizontalSpacing, leftStartY, buttonWidth, buttonHeight),
            text: "Neues Profil",
            font: _font,
            audioService: _audioService
        ));

        AddButton(new MenuButton(
            id: (int)ButtonAction.Back,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
            text: "Back",
            font: _font,
            audioService: _audioService
        ));

        // Buttons on the right side
        _multiplayerButton = new MenuButton(
            id: (int)ButtonAction.Multiplayer,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
            text: "Multiplayer",
            font: _font,
            audioService: _audioService
        );

        _multiplayerButton.Clicked += id =>
        {
            _selectedGameMode = GameMode.MultiPlayer;
            UpdateModeButtonColors();
            RaiseBtnClicked(id);
        };
        AddButton(_multiplayerButton);
        // Singleplayer Button
        _singleplayerButton = new MenuButton(
            id: (int)ButtonAction.Singleplayer,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY, buttonWidth, buttonHeight),
            text: "Singleplayer",
            font: _font,
            audioService: _audioService
        );

        _singleplayerButton.Clicked += id =>
        {
            _selectedGameMode = GameMode.SinglePlayer;
            UpdateModeButtonColors();
            RaiseBtnClicked(id);
        };
        AddButton(_singleplayerButton);
        // centre start Button
        AddButton(new MenuButton(
            id: (int)ButtonAction.StartGame,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(
                (screenWidth - buttonWidth) / 2,
                screenHeight / 3 - buttonHeight / 2,
                buttonWidth,
                buttonHeight
            ),
            text: "Spiel starten",
            font: _font,
            audioService: _audioService
        ));

        UpdateSelection(3);
        UpdateModeButtonColors();
    }
    private void UpdateModeButtonColors()
    {
        bool isMultiplayer = _selectedGameMode == GameMode.MultiPlayer;

        // Multiplayer Button
        _multiplayerButton.BackgroundColor = isMultiplayer ? _selectedColor : _defaultColor;
        _multiplayerButton.IsSelected = isMultiplayer;

        // Singleplayer Button
        _singleplayerButton.BackgroundColor = !isMultiplayer ? _selectedColor : _defaultColor;
        _singleplayerButton.IsSelected = !isMultiplayer;
    }

    public override bool DrawLower => false;
    public override bool UpdateLower => false;
}