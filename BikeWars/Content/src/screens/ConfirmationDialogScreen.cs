using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;

namespace BikeWars.Content.screens
{
    public class ConfirmationDialogScreen : MenuScreenBase, IScreen
    {
        private readonly string _message;
        private readonly IScreen _previousScreen;
        private readonly AudioService _audioService;
        public float MusicVolume => 0.5f;
        private Texture2D _overlayTexture;

        public ConfirmationDialogScreen(SpriteFont font, string message, IScreen previousScreen, AudioService audioService)
            : base(null, font)
        {
            _message = message;
            _previousScreen = previousScreen;
            _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
            InitializeButtons();
        }

        protected sealed override void InitializeButtons()
        {
            Game1 game = Game1.Instance;
            int screenWidth = game.GraphicsDevice.Viewport.Width;
            int screenHeight = game.GraphicsDevice.Viewport.Height;

            int buttonWidth = 150;
            int buttonHeight = 60;
            int buttonSpacing = 30;

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);

            int totalWidth = buttonWidth * 2 + buttonSpacing;
            int startX = (screenWidth - totalWidth) / 2;

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.ConfirmYes,
                texture: _buttonTexture,
                bounds: new Rectangle(startX, screenHeight / 2 + 80, buttonWidth, buttonHeight),
                text: "Ja",
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.ConfirmNo,
                texture: _buttonTexture,
                bounds: new Rectangle(startX + buttonWidth + buttonSpacing, screenHeight / 2 + 80, buttonWidth, buttonHeight),
                text: "Nein",
                font: _font,
                audioService: _audioService
            ));
            
            _overlayTexture = CreateOverlayTexture(game.GraphicsDevice, screenWidth, screenHeight);
            UpdateSelection(0);
        }

        protected override void HandleButtonClick(MenuButton button)
        {
            switch ((ButtonAction)button.Id)
            {
                case ButtonAction.ConfirmYes:
                    Game1.Instance.Exit();
                    break;

                case ButtonAction.ConfirmNo:
                    ScreenManager.RemoveScreen(this);
                    break;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Game1 game = Game1.Instance;
            SpriteBatch spriteBatch = game.SpriteBatch;

            spriteBatch.Begin();
            
            spriteBatch.Draw(_overlayTexture, Vector2.Zero, Color.Black * 0.7f);

            float messageScale = 2.0f;
            Vector2 messageSize = _font.MeasureString(_message) * messageScale;
            Vector2 messagePos = new Vector2(
                (game.GraphicsDevice.Viewport.Width - messageSize.X) / 2,
                game.GraphicsDevice.Viewport.Height / 2 - 40
            );

            spriteBatch.DrawString(_font, _message, messagePos, Color.White,
                0f, Vector2.Zero, messageScale, SpriteEffects.None, 0f);

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

        public override bool DrawLower => true;
        public override bool UpdateLower => false;
    }
}
