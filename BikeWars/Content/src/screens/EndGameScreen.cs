using System;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;

namespace BikeWars.Content.screens
{
    public abstract class EndGameScreen : MenuScreenBase, IScreen
    {
        protected readonly AudioService _audioService;
        public string DesiredMusic => null;
        public float MusicVolume => 1.0f;

        protected float _musicDelayTimer = 2.5f;
        protected bool _musicStarted = false;

        public Statistic Statistic { get; set; }

        public EndGameScreen(SpriteFont font, AudioService audioService, Statistic statistic, Viewport vp)
            : base(null, font, vp)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            Statistic = statistic;
        }

        protected override void InitializeButtons()
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

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            foreach (var button in _buttons)
            {
                button.Draw(sb);
            }

            StatisticsComponent sc = new StatisticsComponent(Statistic);
            sc.Draw(sb, RenderPrimitives.Pixel, Color.DarkSlateGray, new Vector2(400, 600), UIAssets.DefaultFont);
        }

        protected void DrawTitle(SpriteBatch sb, string title, Color color, GameTime gameTime, int yPosition)
        {
            float pulse = MathF.Sin((float)gameTime.TotalGameTime.TotalSeconds * 2.2f);
            float scale = 5.0f + pulse * 0.5f;

            Vector2 textSize = _font.MeasureString(title) * scale;

            Vector2 position = new Vector2(
                (sb.GraphicsDevice.Viewport.Width - textSize.X) / 2,
                yPosition
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
                color,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f
            );
        }
        public override bool DrawLower => true;
        public override bool UpdateLower => false;
    }
}
