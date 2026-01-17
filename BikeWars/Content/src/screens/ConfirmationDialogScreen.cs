using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using System;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens
{
    public class ConfirmationDialogScreen : MenuScreenBase, IScreen
    {
        private readonly string _message;
        private readonly IScreen _previousScreen;
        private readonly AudioService _audioService;
        public float MusicVolume => 0.5f;

        public event Action Exit;

        public ConfirmationDialogScreen(SpriteFont font, string message, IScreen previousScreen, AudioService audioService, Viewport vp)
            : base(null, font, vp)
        {
            _message = message;
            _previousScreen = previousScreen;
            _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
        }

        public override void LoadContent(ContentManager content, GraphicsDevice gd)
        {
            base.LoadContent(content, gd);
            InitializeButtons();
        }
        protected sealed override void InitializeButtons()
        {
            int buttonWidth = 150;
            int buttonHeight = 60;
            int buttonSpacing = 30;

            int screenWidth = ViewPort.Width;
            int screenHeight = ViewPort.Height;

            int totalWidth = buttonWidth * 2 + buttonSpacing;
            int startX = (screenWidth - totalWidth) / 2;

            AddButton(new MenuButton(
                id: (int)ButtonAction.ConfirmYes,
                texture: RenderPrimitives.Pixel,
                bounds: new Rectangle(startX, screenHeight / 2 + 80, buttonWidth, buttonHeight),
                text: "Ja",
                font: _font,
                audioService: _audioService
            ));
            AddButton(new MenuButton(
                id: (int)ButtonAction.ConfirmNo,
                texture: RenderPrimitives.Pixel,
                bounds: new Rectangle(startX + buttonWidth + buttonSpacing, screenHeight / 2 + 80, buttonWidth, buttonHeight),
                text: "Nein",
                font: _font,
                audioService: _audioService
            ));
            UpdateSelection(0);
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

        public override bool DrawLower => true;
        public override bool UpdateLower => false;
    }
}
