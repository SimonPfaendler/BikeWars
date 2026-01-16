using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using MonoGame.Extended.Content;
using System;

namespace BikeWars.Content.screens
{
    public class ConfirmationDialogScreen : MenuScreenBase, IScreen
    {
        private readonly string _message;
        private readonly IScreen _previousScreen;
        private readonly AudioService _audioService;
        public float MusicVolume => 0.5f;
        private Texture2D _overlayTexture;

        public event Action Exit;

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
            int buttonWidth = 150;
            int buttonHeight = 60;
            int buttonSpacing = 30;

            _buttonTexture = CreateSimpleTexture(buttonWidth, buttonHeight);

            int screenWidth = Content.GetGraphicsDevice().Viewport.Width;
            int screenHeight = Content.GetGraphicsDevice().Viewport.Height;

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

            // _overlayTexture = CreateOverlayTexture(screenWidth, screenHeight);
            UpdateSelection(0);
        }

        protected override void HandleButtonClick(MenuButton button)
        {
            switch ((ButtonAction)button.Id)
            {
                case ButtonAction.ConfirmYes:
                    Exit?.Invoke();
                    break;

                case ButtonAction.ConfirmNo:
                    ScreenManager.RemoveScreen(this);
                    break;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            sb.Begin();
            Viewport vp = sb.GraphicsDevice.Viewport;
            sb.Draw(RenderPrimitives.Pixel, new Rectangle(0,0, vp.Width, vp.Height), Color.Black * 0.7f);

            float messageScale = 2.0f;
            Vector2 messageSize = _font.MeasureString(_message) * messageScale;
            Vector2 messagePos = new Vector2(
                (sb.GraphicsDevice.Viewport.Width - messageSize.X) / 2,
                sb.GraphicsDevice.Viewport.Height / 2 - 40
            );

            sb.DrawString(_font, _message, messagePos, Color.White,
                0f, Vector2.Zero, messageScale, SpriteEffects.None, 0f);

            foreach (var button in _buttons)
            {
                button.Draw(sb);
            }
            sb.End();
        }

        // TODOJL
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
