using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using System;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens
{
    public class PauseMenuScreen : MenuScreenBase, IScreen
    {
        private readonly AudioService _audioService;
        public string DesiredMusic => AudioAssets.GameMusic;
        public float MusicVolume => 0.5f;
        public event Action<GraphicsCommand> GraphicsRequested;

        public PauseMenuScreen(SpriteFont font, AudioService audioService, Viewport vp)
            :base(null, font, vp)
        {
            _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
        }

        public override void LoadContent(ContentManager content, GraphicsDevice gd)
        {
            base.LoadContent(content, gd);
            InitializeButtons();
        }

        protected sealed override void InitializeButtons()
        {
            int screenWidth = ViewPort.Width;
            int screenHeight = ViewPort.Height;

            int buttonWidth = 300;
            int buttonHeight = 60;

            int startY = screenHeight / 4;
            int verticalSpacing = 20;

            _buttons.Clear();

            var buttonDefinitions = new[]
            {
                (id: ButtonAction.Resume, text: "Spiel fortsetzen"),
                (id: ButtonAction.SaveGame, text: "Spiel speichern"),
                (id: ButtonAction.LoadGame, text: "Spiel laden"),
                (id: ButtonAction.MainMenu, text: "Hauptmenu"),
                (id: ButtonAction.Options, text: "Optionen"),
                (id: ButtonAction.Exit, text: "Spiel beenden")
            };

            for (int i = 0; i < buttonDefinitions.Length; i++)
            {
                AddButton(new MenuButton(
                    id: (int)buttonDefinitions[i].id,
                    texture: RenderPrimitives.Pixel,
                    bounds: new Rectangle((screenWidth - buttonWidth) / 2, startY + i * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                    text: buttonDefinitions[i].text,
                    font: _font,
                    audioService: _audioService
                ));
            }

            UpdateSelection(0);
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            sb.Begin();
            sb.Draw(RenderPrimitives.Pixel, new Rectangle(0, 0, ViewPort.Width, ViewPort.Height), Color.Black * 0.7f);
            foreach (var button in _buttons)
            {
                button.Draw(sb);
            }
            sb.End();
        }

        public override bool DrawLower => true;    // GameScreen gets drawn
        public override bool UpdateLower => false; // GameScreen doesn't get updated
    }
}