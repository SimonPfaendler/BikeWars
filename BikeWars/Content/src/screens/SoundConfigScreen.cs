using System;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
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
            
            UpdateSelection(0);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            MouseState mouse = Mouse.GetState();
            Point mousePos = mouse.Position;
            
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                // music
                if (_musicTrackRect.Contains(mousePos) || _isDraggingMusic)
                {
                    _isDraggingMusic = true;
                    float newValue = MathHelper.Clamp((float)(mousePos.X - _musicTrackRect.X) / _musicTrackRect.Width, 0f, 1f);
                    _audioService.Music.MasterVolume = newValue;
                }
                
                // sounds
                if (_sfxTrackRect.Contains(mousePos) || _isDraggingSfx)
                {
                    _isDraggingSfx = true;
                    float newValue = MathHelper.Clamp((float)(mousePos.X - _sfxTrackRect.X) / _sfxTrackRect.Width, 0f, 1f);
                    _audioService.Sounds.MasterVolume = newValue;
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

            DrawSlider(spriteBatch, _musicTrackRect, _audioService.Music.MasterVolume, "Musik");
            DrawSlider(spriteBatch, _sfxTrackRect, _audioService.Sounds.MasterVolume, "Effekte");

            spriteBatch.End();
        }

        private void DrawSlider(SpriteBatch sb, Rectangle track, float volume, string label)
        {
            sb.Draw(_sliderTexture, track, Color.Gray * 0.5f);
            
            // knob
            int knobX = track.X + (int)(track.Width * volume) - (_knobWidth / 2);
            Rectangle knobRect = new Rectangle(knobX, track.Y + (track.Height / 2) - (_knobHeight / 2), _knobWidth, _knobHeight);
            sb.Draw(_sliderTexture, knobRect, Color.Gold);
            
            // scale text
            string text = $"{label}: {(int)(volume * 100)}%";
            float fontScale = 1.4f * _uiScale;
            Vector2 textSize = _font.MeasureString(text) * fontScale;
            
            sb.DrawString(
                _font, 
                text, 
                new Vector2(track.Center.X - textSize.X / 2, track.Y - (65 * _uiScale)), 
                Color.Black, 
                0f, 
                Vector2.Zero, 
                fontScale, 
                SpriteEffects.None, 
                0f
            );
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