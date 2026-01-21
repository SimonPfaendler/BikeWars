using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.input;
using Microsoft.Xna.Framework.Content;

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
    public InputTypeScreen(Texture2D background, SpriteFont font, AudioService audioService, bool isPlayer1, Viewport vp)
        : base(background, font, vp)
    {
        _isPlayer1 = isPlayer1;
        _audioService = audioService;

        InitializeButtons();
    }

    public bool IsPlayer1()
    {
        return _isPlayer1;
    }

    public override void LoadContent(ContentManager content, GraphicsDevice gd)
    {
        base.LoadContent(content, gd);
        _keyboardTexture = content.Load<Texture2D>("assets/images/TastaturBelegung");
        _controllerTexture = content.Load<Texture2D>("assets/images/ControllerBelegung");
        InitializeButtons();
    }

    protected sealed override void InitializeButtons()
    {
        // sizes of the screens components change when the screen size changes
        _buttons.Clear();
        _uiScale = ViewPort.Height / 1080f;

        int buttonWidth = (int)(250 * _uiScale);
        int buttonHeight = (int)(60 * _uiScale);
        int verticalSpacing = (int)(20 * _uiScale);
        int horizontalSpacing = (int)(ViewPort.Width * 0.05f);

        int leftStartY = (int)(ViewPort.Height * 0.25f);

        _keyboardButton = new MenuButton(
            id: (int)ButtonAction.Keyboard,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
            text: "Tastatur",
            font: _font,
            audioService: _audioService
        );

        _controllerButton = new MenuButton(
            id: (int)ButtonAction.Controller,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(horizontalSpacing, leftStartY + 2 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
            text: "Controller",
            font: _font,
            audioService: _audioService
        );

        AddButton(_keyboardButton);
        AddButton(_controllerButton);

        AddButton(new MenuButton(
            id: (int)ButtonAction.Back,
            texture: RenderPrimitives.Pixel,
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
        _imageOffsetX = (int)(ViewPort.Width * 0.07f);
        UpdateUIState();
        UpdateSelection(0);
    }

    public void UpdateUIState()
    {
        // show the UI_Type chosen by the Player
        ControlType current = _isPlayer1 ? InputSettings.Player1Control : InputSettings.Player2Control;

        // mark the selected gamemode blue
        _keyboardButton.BackgroundColor = (current == ControlType.Keyboard) ? _selectedColor : _defaultColor;
        _controllerButton.BackgroundColor = (current == ControlType.Controller) ? _selectedColor : _defaultColor;
    }

    public override void Draw(GameTime gameTime, SpriteBatch sb)
    {
        sb.Begin();

        Rectangle destinationRect = new Rectangle(
            0, 0,
            ViewPort.Width,
            ViewPort.Height);

        sb.Draw(_backgroundTexture, destinationRect, Color.White);

        // show the configurations of the chosen UI-Type
        ControlType current = _isPlayer1 ? InputSettings.Player1Control : InputSettings.Player2Control;
        Texture2D textureToShow = (current == ControlType.Keyboard) ? _keyboardTexture : _controllerTexture;

        // draw keyboard image
        if (textureToShow != null)
        {
            int scaledWidth = (int)(textureToShow.Width * _imageScale);
            int scaledHeight = (int)(textureToShow.Height * _imageScale);


            int x = (ViewPort.Width - scaledWidth) / 2 + _imageOffsetX;
            int y = (ViewPort.Height - scaledHeight) / 2;

            Rectangle destRect = new Rectangle(x, y, scaledWidth, scaledHeight);

            sb.Draw(
                textureToShow,
                destRect,
                Color.White
            );
        }

        // draw buttons
        foreach (var button in _buttons)
        {
            button.Draw(sb);
        }
        sb.End();
    }

    public override bool DrawLower => false;
    public override bool UpdateLower => false;
}