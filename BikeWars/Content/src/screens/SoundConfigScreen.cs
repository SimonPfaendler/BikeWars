using System;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
using BikeWars.Content.engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BikeWars.Content.screens
{
    public class SoundConfigScreen : MenuScreenBase, IScreen
    {
        private readonly AudioService _audioService;
        
        private Rectangle _musicTrackRect;
        private Rectangle _sfxTrackRect;
        private Texture2D _sliderTexture;
        
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

        public SoundConfigScreen(Texture2D background, SpriteFont font, AudioService audioService)
            : base(background, font)
        {
            _audioService = audioService;
            
            _buttonTexture = CreateSimpleTexture(Game1.Instance.GraphicsDevice, 1, 1);
            _sliderTexture = CreateSimpleTexture(Game1.Instance.GraphicsDevice, 1, 1);
            
            InitializeButtons();
        }

        protected sealed override void InitializeButtons()
        {
            _buttons.Clear();
            var viewport = Game1.Instance.GraphicsDevice.Viewport;
            
            _uiScale = viewport.Height / 1080f;

            // scale button
            int btnWidth = (int)(250 * _uiScale);
            int btnHeight = (int)(60 * _uiScale);
            int margin = (int)(50 * _uiScale);
            
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Back,
                texture: _buttonTexture,
                bounds: new Rectangle(margin, viewport.Height - btnHeight - margin, btnWidth, btnHeight),
                text: "Back",
                font: _font,
                audioService: _audioService
            ));

            // scale slider
            int trackWidth = (int)(viewport.Width * 0.45f);
            int trackHeight = (int)(20 * _uiScale);
            
            _knobWidth = (int)(30 * _uiScale);
            _knobHeight = (int)(50 * _uiScale);
            
            int centerX = viewport.Width / 2 - trackWidth / 2;
            
            int musicY = (int)(viewport.Height * 0.35f);
            _musicTrackRect = new Rectangle(centerX, musicY, trackWidth, trackHeight);
            
            int sfxY = (int)(viewport.Height * 0.55f);
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

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            
            var spriteBatch = Game1.Instance.SpriteBatch;
            spriteBatch.Begin();

            DrawSlider(spriteBatch, _musicTrackRect, _audioService.Music.MasterVolume, "Musik Lautstaerke", _selectedItem == 1);
            DrawSlider(spriteBatch, _sfxTrackRect, _audioService.Sounds.MasterVolume, "Effekt Lautstaerke", _selectedItem == 2);

            spriteBatch.End();
        }

        private void DrawSlider(SpriteBatch sb, Rectangle track, float volume, string label, bool isSelected)
        {
            // track
            Color trackColor = isSelected ? Color.White * 0.8f : Color.Gray * 0.5f;
            sb.Draw(_sliderTexture, track, trackColor);
            
            int knobX = track.X + (int)(track.Width * volume) - (_knobWidth / 2);
            Rectangle knobRect = new Rectangle(knobX, track.Y + (track.Height / 2) - (_knobHeight / 2), _knobWidth, _knobHeight);
            
            // knob
            sb.Draw(_sliderTexture, knobRect, isSelected ? Color.Gold : Color.DarkGoldenrod);

            string text = $"{label}: {(int)(volume * 100)}%";
            float fontScale = 1.4f * _uiScale;
            Vector2 textSize = _font.MeasureString(text) * fontScale;
            
            // text
            Color textColor = isSelected ? Color.Yellow : Color.Black;

            sb.DrawString(_font, text, new Vector2(track.Center.X - textSize.X / 2, track.Y - (70 * _uiScale)), 
                textColor, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
        }
        
        protected override void HandleButtonClick(MenuButton button)
        {
            if (button.Id == (int)ButtonAction.Back)
            {
                ScreenManager.RemoveScreen(this);
            }
        }
    }
}