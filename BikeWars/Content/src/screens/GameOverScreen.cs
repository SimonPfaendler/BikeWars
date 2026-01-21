using System;
using System.IO;
using System.Text.Json;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens
{
    public class GameOverScreen : MenuScreenBase, IScreen
    {
        private readonly AudioService _audioService;
        public string DesiredMusic => null;
        public float MusicVolume => 1.0f;

        private float _musicDelayTimer = 2.5f;
        private bool _musicStarted = false;

        private Texture2D _crashSheet;
        private List<AnimationFrame> _frames;
        private float _animTotalTime = 0f;
        private const float _frameRate = 1f / 6f;
        private int _currentFrameIndex = 0;
        private bool _isAnimationLoaded = false;

        public event Action Exit;

        public Statistic Statistic {get; set;}

        private ConfirmationDialogScreen _confirmDialog {get;set;}

        public GameOverScreen(SpriteFont font, AudioService audioService, Statistic statistic, Viewport vp)
            :base(null, font, vp)
        {
            _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
            Statistic = statistic;
            _confirmDialog = new ConfirmationDialogScreen(
                _font,
                "Bist Du Dir sicher?",
                this,
                _audioService,
                vp
            );
            _confirmDialog.Exit += () => Exit();
        }

        public override void LoadContent(ContentManager content, GraphicsDevice gd)
        {
            base.LoadContent(content, gd);
            InitializeButtons();
            LoadAnimationAssets(content);
        }

        private void LoadAnimationAssets(ContentManager content)
        {
            try
            {
                _crashSheet = content.Load<Texture2D>("assets/sprites/videos/BikeCrash");
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "sprites", "BikeCrash.json");

                if (File.Exists(jsonPath))
                {
                    string jsonString = File.ReadAllText(jsonPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var root = JsonSerializer.Deserialize<TexturePackerRoot>(jsonString, options);

                    var rawFrames = root?.Frames;

                    if (rawFrames != null && rawFrames.Count > 0)
                    {
                        _frames = rawFrames.Select(f => new AnimationFrame
                        {
                            SourceRectangle = new Rectangle(f.Frame.X, f.Frame.Y, f.Frame.W, f.Frame.H),
                            Filename = f.Filename
                        }).ToList();

                        _isAnimationLoaded = true;
                        System.Diagnostics.Debug.WriteLine($"Animation geladen: {_frames.Count} Frames.");
                    }

                    if (_frames != null && _frames.Count > 0)
                    {
                        _isAnimationLoaded = true;
                        System.Diagnostics.Debug.WriteLine($"Animation geladen: {_frames.Count} Frames.");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"WARNUNG: JSON nicht gefunden unter {jsonPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FEHLER beim Laden der Animation: {ex.Message}");
            }
        }

        protected sealed override void InitializeButtons()
        {
            int screenWidth = ViewPort.Width;
            int screenHeight = ViewPort.Height;

            int buttonWidth = 380;
            int buttonHeight = 80;

            const int horizontalMargin = 10;

            int startY = screenHeight / 2 + 50;

            var buttonDefinitions = new[]
            {
                (id: ButtonAction.MainMenu, text: "Hauptmenu", isLeft: true),
                (id: ButtonAction.Exit, text: "Spiel beenden", isLeft: false)
            };

            foreach (var definition in buttonDefinitions)
            {
                int buttonX;

                if (definition.isLeft)
                {
                    buttonX = horizontalMargin;
                }
                else
                {
                    buttonX = screenWidth - buttonWidth - horizontalMargin;
                }

                AddButton(new MenuButton(
                    id: (int)definition.id,
                    texture: RenderPrimitives.Pixel,
                    bounds: new Rectangle(buttonX, startY, buttonWidth, buttonHeight),
                    text: definition.text,
                    font: _font,
                    audioService: _audioService
                ));
            }
            UpdateSelection(0);
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            int screenWidth = ViewPort.Width;
            int screenHeight = ViewPort.Height;

            sb.Begin();
            if (_isAnimationLoaded)
            {
                if (_currentFrameIndex >= _frames.Count) _currentFrameIndex = 0;

                AnimationFrame currentFrame = _frames[_currentFrameIndex];
                Rectangle sourceRect = currentFrame.SourceRectangle;

                float scaleAnimation = 1.0f;

                Vector2 videoPos = new Vector2(
                    (screenWidth - (sourceRect.Width * scaleAnimation)) / 2,
                    (screenHeight - (sourceRect.Height * scaleAnimation)) / 2
                );

                sb.Draw(
                    _crashSheet,
                    videoPos,
                    sourceRect,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    scaleAnimation,
                    SpriteEffects.None,
                    0f
                );
            }

            // GAME OVER Text
            string title = "GAME OVER";

            float pulse = MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * 2.2f);
            float scale = 5.0f + pulse * 0.5f;

            Vector2 textSize = _font.MeasureString(title) * scale;

            Vector2 position = new Vector2(
                (screenWidth - textSize.X) / 2,
                140
            );

            sb.DrawString(
                _font,
                title,
                position + new Vector2(4, 4),
                Color.Black,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f
            );

            sb.DrawString(
                _font,
                title,
                position,
                Color.DarkRed,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f
            );

            foreach (var button in _buttons)
            {
                button.Draw(sb);
            }
            StatisticsComponent sc = new StatisticsComponent(Statistic);
            sc.Draw(sb, RenderPrimitives.Pixel, Color.DarkSlateGray, new Vector2(400, 600), UIAssets.DefaultFont);
            // shown text
            sb.End();
        }

        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!_musicStarted)
            {
                _musicDelayTimer -= delta;

                if (_musicDelayTimer <= 0)
                {
                    _audioService.Music.Play(AudioAssets.MenuMusic);
                    _musicStarted = true;
                }
            }

            if (_isAnimationLoaded)
            {
                _animTotalTime += delta;

                int frameCount = _frames.Count;
                _currentFrameIndex = (int)(_animTotalTime / _frameRate) % frameCount;
            }

            base.Update(gameTime);
        }

        public override bool DrawLower => true;    // GameScreen gets drawn
        public override bool UpdateLower => false; // GameScreen doesn't get updated
    }
}