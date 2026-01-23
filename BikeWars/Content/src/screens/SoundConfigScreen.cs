using BikeWars.Content.engine.Audio;
using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
using BikeWars.Content.engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Content;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens;
public class SoundConfigScreen : MenuScreenBase, IScreen
{
    private readonly AudioService _audioService;

    private Rectangle _musicTrackRect;
    private Rectangle _sfxTrackRect;

    private bool _isDraggingMusic;
    private bool _isDraggingSfx;

    private float _uiScale;
    private int _knobWidth;
    private int _knobHeight;

    private int _selectedItem = 1;
    private float _volumeStep = 0.01f;

    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;
    public override bool DrawLower => false;
    public override bool UpdateLower => false;

    public SoundConfigScreen(Texture2D background, SpriteFont font, AudioService audioService, Viewport vp)
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
        _buttons.Clear();
        _uiScale = ViewPort.Height / 1080f;

        // scale button
        int btnWidth = (int)(250 * _uiScale);
        int btnHeight = (int)(60 * _uiScale);
        int margin = (int)(50 * _uiScale);

        AddButton(new MenuButton(
            id: (int)ButtonAction.Back,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(margin, ViewPort.Height - btnHeight - margin, btnWidth, btnHeight),
            text: "Back",
            font: _font,
            audioService: _audioService
        ));

        // scale slider
        int trackWidth = (int)(ViewPort.Width * 0.45f);
        int trackHeight = (int)(20 * _uiScale);

        _knobWidth = (int)(30 * _uiScale);
        _knobHeight = (int)(50 * _uiScale);

        int centerX = ViewPort.Width / 2 - trackWidth / 2;

        int musicY = (int)(ViewPort.Height * 0.35f);
        _musicTrackRect = new Rectangle(centerX, musicY, trackWidth, trackHeight);

        int sfxY = (int)(ViewPort.Height * 0.55f);
        _sfxTrackRect = new Rectangle(centerX, sfxY, trackWidth, trackHeight);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        HandleControllerInput();
        HandleMouseInput();

        if (_buttons.Count > 0)
        {
            _buttons[0].IsSelected = (_selectedItem == 0);
        }

        if (_selectedIndex == 0 && _usingMouse)
        {
            _selectedItem = 0;
        }
    }

    private void HandleControllerInput()
    {
        // Navigation up/down
        if (InputHandler.IsPressed(GameAction.UI_UP))
        {
            _selectedItem--;
            if (_selectedItem < 0) _selectedItem = 2;
            _usingMouse = false;
        }
        if (InputHandler.IsPressed(GameAction.UI_DOWN))
        {
            _selectedItem++;
            if (_selectedItem > 2) _selectedItem = 0;
            _usingMouse = false;
        }

        if (!_usingMouse)
        {
            _selectedIndex = (_selectedItem == 0) ? 0 : -1;
        }

        // set volume
        if (_selectedItem == 1) // Music
        {
            if (InputHandler.IsHeld(GameAction.UI_LEFT)) _audioService.Music.MasterVolume -= _volumeStep;
            if (InputHandler.IsHeld(GameAction.UI_RIGHT)) _audioService.Music.MasterVolume += _volumeStep;
        }
        else if (_selectedItem == 2) // SFX
        {
            if (InputHandler.IsHeld(GameAction.UI_LEFT)) _audioService.Sounds.MasterVolume -= _volumeStep;
            if (InputHandler.IsHeld(GameAction.UI_RIGHT)) _audioService.Sounds.MasterVolume += _volumeStep;
        }

        _audioService.Music.MasterVolume = MathHelper.Clamp(_audioService.Music.MasterVolume, 0, 1);
        _audioService.Sounds.MasterVolume = MathHelper.Clamp(_audioService.Sounds.MasterVolume, 0, 1);
    }

    private void HandleMouseInput()
    {
        MouseState mouse = Mouse.GetState();
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            _usingMouse = true;
            if (_musicTrackRect.Contains(mouse.Position) || _isDraggingMusic)
            {
                _isDraggingMusic = true;
                _selectedItem = 1;
                _audioService.Music.MasterVolume = MathHelper.Clamp((float)(mouse.X - _musicTrackRect.X) / _musicTrackRect.Width, 0f, 1f);
            }
            if (_sfxTrackRect.Contains(mouse.Position) || _isDraggingSfx)
            {
                _isDraggingSfx = true;
                _selectedItem = 2;
                _audioService.Sounds.MasterVolume = MathHelper.Clamp((float)(mouse.X - _sfxTrackRect.X) / _sfxTrackRect.Width, 0f, 1f);
            }
        }
        else
        {
            _isDraggingMusic = false;
            _isDraggingSfx = false;
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch sb)
    {
        base.Draw(gameTime, sb);
        sb.Begin();

        DrawSlider(sb, _musicTrackRect, _audioService.Music.MasterVolume, "Musik Lautstaerke", _selectedItem == 1);
        DrawSlider(sb, _sfxTrackRect, _audioService.Sounds.MasterVolume, "Effekt Lautstaerke", _selectedItem == 2);

        sb.End();
    }

    private void DrawSlider(SpriteBatch sb, Rectangle track, float volume, string label, bool isSelected)
    {
        string text = $"{label}: {(int)(volume * 100)}%";
        float fontScale = 1.4f * _uiScale;
        Vector2 textSize = _font.MeasureString(text) * fontScale;

        Rectangle box = new Rectangle(track.X - 20, track.Y - 20, track.Width + 40, track.Height * 5);
        sb.Draw(RenderPrimitives.Pixel, box, Color.White);

        Rectangle box2 = new Rectangle((int)(track.Center.X - textSize.X / 2) - 5, (int)(track.Y - (70 * _uiScale)) - 5, 200, 30);
        sb.Draw(RenderPrimitives.Pixel, box2, Color.White);


        // track
        Color trackColor = isSelected ? Color.White * 0.8f : Color.Gray * 0.5f;
        sb.Draw(RenderPrimitives.Pixel, track, trackColor);

        int knobX = track.X + (int)(track.Width * volume) - (_knobWidth / 2);
        Rectangle knobRect = new Rectangle(knobX, track.Y + (track.Height / 2) - (_knobHeight / 2), _knobWidth, _knobHeight);

        // knob
        sb.Draw(RenderPrimitives.Pixel, knobRect, isSelected ? Color.Gold : Color.DarkGoldenrod);

        // text
        Color textColor = isSelected ? Color.Yellow : Color.Black;

        sb.DrawString(_font, text, new Vector2(track.Center.X - textSize.X / 2, track.Y - (70 * _uiScale)),
            textColor, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
    }
}