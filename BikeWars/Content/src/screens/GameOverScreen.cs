using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
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

        public GameOverScreen(SpriteFont font, AudioService audioService)
            :base(null, font)
        {
            _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
            InitializeButtons();
        }
        
        protected sealed override void InitializeButtons()
        {
            Game1 game = Game1.Instance;
            int screenWidth = game.GraphicsDevice.Viewport.Width;
            int screenHeight = game.GraphicsDevice.Viewport.Height;
    
            int buttonWidth = 380;
            int buttonHeight = 80;

            int startY = screenHeight / 2 + 10;

            int verticalSpacing = 20;

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);
            
            var buttonDefinitions = new[]
            {
                (id: ButtonAction.MainMenu, text: "Hauptmenu"),
                (id: ButtonAction.Exit, text: "Spiel beenden")
            };
    
            for (int i = 0; i < buttonDefinitions.Length; i++)
            {
                _buttons.Add(new MenuButton(
                    id: (int)buttonDefinitions[i].id,
                    texture: _buttonTexture,
                    bounds: new Rectangle((screenWidth - buttonWidth) / 2, startY + i * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                    text: buttonDefinitions[i].text,
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
            
            spriteBatch.Begin();
            
            Texture2D overlay = CreateOverlayTexture(game.GraphicsDevice, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(overlay, Vector2.Zero, Color.White * 0.7f);
            
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

            base.Update(gameTime);
        }

        public override bool DrawLower => true;    // GameScreen gets drawn
        public override bool UpdateLower => false; // GameScreen doesn't get updated
    }
}