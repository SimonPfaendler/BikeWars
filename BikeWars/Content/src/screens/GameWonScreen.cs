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
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens
{
    public enum AnimationState
    {
        Win,
        Beer
    }

    public class GameWonScreen : EndGameScreen
    {
        private Texture2D _winSheet;
        private Texture2D _beerSheet;

        private List<AnimationFrame> _winFrames;
        private List<AnimationFrame> _beerFrames;

        private AnimationState _currentState = AnimationState.Win;

        private float _animTotalTime = 0f;
        private const float _frameRate = 1f / 6f;
        private int _currentFrameIndex = 0;
        private bool _isAnimationLoaded = false;

        private float _animationDurationSeconds = 0f;

        public GameWonScreen(SpriteFont font, AudioService audioService, Statistic statistic, Viewport vp)
            :base(font, audioService, statistic, vp)
        {
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
                // load textures
                _winSheet = content.Load<Texture2D>("assets/sprites/videos/PogacarWinSpriteSheet");
                _beerSheet = content.Load<Texture2D>("assets/sprites/videos/PogacarBeerSpriteSheet");

                // define paths
                string jsonPathWin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "sprites", "PogacarWinSpriteSheet.json");
                string jsonPathBeer = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "sprites", "PogacarBeerSpriteSheet.json");

                // load win animation
                if (File.Exists(jsonPathWin))
                {
                    string jsonString = File.ReadAllText(jsonPathWin);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var root = JsonSerializer.Deserialize<TexturePackerRoot>(jsonString, options);
                    var rawFrames = root?.Frames;

                    if (rawFrames != null && rawFrames.Count > 0)
                    {
                        _winFrames = rawFrames.Select(f => new AnimationFrame
                        {
                            SourceRectangle = new Rectangle(f.Frame.X, f.Frame.Y, f.Frame.W, f.Frame.H),
                            Filename = f.Filename
                        }).ToList();
                        System.Diagnostics.Debug.WriteLine($"Win-Animation geladen: {_winFrames.Count} Frames.");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"WARNUNG: JSON nicht gefunden unter {jsonPathWin}");
                }

                // load beer animation
                if (File.Exists(jsonPathBeer))
                {
                    string jsonString = File.ReadAllText(jsonPathBeer);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var root = JsonSerializer.Deserialize<TexturePackerRoot>(jsonString, options);
                    var rawFrames = root?.Frames;

                    if (rawFrames != null && rawFrames.Count > 0)
                    {
                        _beerFrames = rawFrames.Select(f => new AnimationFrame
                        {
                            SourceRectangle = new Rectangle(f.Frame.X, f.Frame.Y, f.Frame.W, f.Frame.H),
                            Filename = f.Filename
                        }).ToList();
                        System.Diagnostics.Debug.WriteLine($"Beer-Animation geladen: {_beerFrames.Count} Frames.");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"WARNUNG: JSON nicht gefunden unter {jsonPathBeer}");
                }

                if (_winFrames != null && _beerFrames != null && _winFrames.Count > 0 && _beerFrames.Count > 0)
                {
                    _isAnimationLoaded = true;
                    _animationDurationSeconds = _winFrames.Count * _frameRate;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FEHLER beim Laden der Animation: {ex.Message}");
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            int screenWidth = sb.GraphicsDevice.Viewport.Width;
            int screenHeight = sb.GraphicsDevice.Viewport.Height;

            sb.Begin();
            if (_isAnimationLoaded)
            {
                List<AnimationFrame> currentFrames = _currentState == AnimationState.Win ? _winFrames : _beerFrames;
                Texture2D currentSheet = _currentState == AnimationState.Win ? _winSheet : _beerSheet;

                AnimationFrame currentFrame = currentFrames[_currentFrameIndex];
                Rectangle sourceRect = currentFrame.SourceRectangle;

                float scaleAnimation = 1.0f;

                Vector2 videoPos = new Vector2(
                    (screenWidth - (sourceRect.Width * scaleAnimation)) / 2,
                    (screenHeight / 2 - (sourceRect.Height * scaleAnimation) / 2)
                );

                sb.Draw(
                    currentSheet,
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

            DrawTitle(sb, "Gewonnen!", Color.LawnGreen, gameTime, 40);

            base.Draw(gameTime, sb);
            
            sb.End();
        }

        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_isAnimationLoaded)
            {
                List<AnimationFrame> currentFrames = _currentState == AnimationState.Win ? _winFrames : _beerFrames;
                int frameCount = currentFrames.Count;

                _animTotalTime += delta;

                _currentFrameIndex = (int)(_animTotalTime / _frameRate);

                if (_currentFrameIndex >= frameCount)
                {
                    _animTotalTime = 0f;
                    _currentFrameIndex = 0;

                    if (_currentState == AnimationState.Win)
                    {
                        _currentState = AnimationState.Beer;
                        _animationDurationSeconds = _beerFrames.Count * _frameRate;
                    }
                    else // State == AnimationState.Beer
                    {
                        _currentState = AnimationState.Win;
                        _animationDurationSeconds = _winFrames.Count * _frameRate;
                    }
                }
            }

            base.Update(gameTime);
        }
    }
}