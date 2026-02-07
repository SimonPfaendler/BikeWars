using System;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using Microsoft.Xna.Framework.Content;
using BikeWars.Content.components;

namespace BikeWars.Content.screens
{
    public class GameOverScreen : EndGameScreen
    {
        private Texture2D _crashSheet;
        private List<AnimationFrame> _frames;
        private float _animTotalTime = 0f;
        private const float _frameRate = 1f / 6f;
        private int _currentFrameIndex = 0;
        private bool _isAnimationLoaded = false;

        // public event Action Exit;

        private ConfirmationDialogScreen _confirmDialog {get;set;}

        public GameOverScreen(SpriteFont font, AudioService audioService, Statistic statistic, Viewport vp)
            :base(font, audioService, statistic, vp)
        {
            _confirmDialog = new ConfirmationDialogScreen(
                _font,
                "Bist Du Dir sicher?",
                ButtonAction.None,
                _audioService,
                vp
            );
            // _confirmDialog.Exit += () => Exit();
        }

        public override void LoadContent(ContentManager content, GraphicsDevice gd)
        {
            base.LoadContent(content, gd);
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

            DrawTitle(sb, "GAME OVER", Color.DarkRed, gameTime, 140);
            base.Draw(gameTime, sb);
            sb.End();
        }

        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_isAnimationLoaded)
            {
                _animTotalTime += delta;

                int frameCount = _frames.Count;
                _currentFrameIndex = (int)(_animTotalTime / _frameRate) % frameCount;
            }
            base.Update(gameTime);
        }
    }
}