using System;
using System.IO;
using System.Text.Json;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using BikeWars.Content.src.screens.Overlay;

namespace BikeWars.Content.screens
{
    public class GameOverScreen : MenuScreenBase, IScreen
    {
        private Overlay _overlay; 
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

        public GameOverScreen(SpriteFont font, AudioService audioService)
            :base(null, font)
        {
            _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
            InitializeButtons();
            
            LoadAnimationAssets();
        }
        
        private void LoadAnimationAssets()
        {
            try 
            {
                _crashSheet = Game1.Instance.Content.Load<Texture2D>("assets/sprites/videos/BikeCrash");
                
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "sprites", "BikeCrash.json");

                if (File.Exists(jsonPath))
                {
                    string jsonString = File.ReadAllText(jsonPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    
                    var root = JsonSerializer.Deserialize<TexturePackerRoot>(jsonString, options);
                    
                    var rawFrames = root?.frames;
                    
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
            Game1 game = Game1.Instance;
            int screenWidth = game.GraphicsDevice.Viewport.Width;
            int screenHeight = game.GraphicsDevice.Viewport.Height;
    
            int buttonWidth = 380;
            int buttonHeight = 80;
            
            const int horizontalMargin = 10;

            int startY = screenHeight / 2 + 50;

            int verticalSpacing = 20;

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);
            
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

                _buttons.Add(new MenuButton(
                    id: (int)definition.id,
                    texture: _buttonTexture,
                    bounds: new Rectangle(buttonX, startY, buttonWidth, buttonHeight), 
                    text: definition.text,
                    font: _font,
                    audioService: _audioService
                ));
            }
        }
        
        protected override void HandleButtonClick(MenuButton button)
        {
            // Use _currentGameTime from ScreenBase class if GameTime is needed
            switch ((ButtonAction)button.Id)
            {
                case ButtonAction.MainMenu:
                    _audioService.Sounds.StopAll();
                    _audioService.Sounds.Play(AudioAssets.SoftClick);
                    ScreenManager.ReturnToMainMenu();
                    break;
            
                case ButtonAction.Exit:
                    ConfirmationDialogScreen confirmDialog = new ConfirmationDialogScreen(
                        _font, 
                        "Bist Du Dir sicher?", 
                        this,
                        _audioService
                    );
                    ScreenManager.AddScreen(confirmDialog);
                    break;
            }
        }
        
        public override void Draw(GameTime gameTime)
        {
            Game1 game = Game1.Instance;
            SpriteBatch spriteBatch = game.SpriteBatch;
            
            int screenWidth = game.GraphicsDevice.Viewport.Width;
            int screenHeight = game.GraphicsDevice.Viewport.Height;
            
            spriteBatch.Begin();
            
            Texture2D overlay = CreateOverlayTexture(game.GraphicsDevice, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(overlay, Vector2.Zero, Color.White * 0.7f);
            
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

                spriteBatch.Draw(
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
                (game.GraphicsDevice.Viewport.Width - textSize.X) / 2,
                140
            );

            
            spriteBatch.DrawString(
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
            
            spriteBatch.DrawString(
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
                button.Draw(spriteBatch);
            }
            
            spriteBatch.End();
        }
        
        private Texture2D CreateOverlayTexture(GraphicsDevice graphicsDevice, int width, int height)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++) 
                data[i] = Color.Black;
            texture.SetData(data);
            return texture;
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