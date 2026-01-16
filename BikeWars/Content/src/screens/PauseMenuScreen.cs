using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.events;
using BikeWars.Content.managers;
using MonoGame.Extended.Content;
using System;

namespace BikeWars.Content.screens
{
    public class PauseMenuScreen : MenuScreenBase, IScreen
    {
        private readonly AudioService _audioService;
        public string DesiredMusic => AudioAssets.GameMusic;
        public float MusicVolume => 0.5f;
        public event Action<GraphicsCommand> GraphicsRequested;


        public PauseMenuScreen(SpriteFont font, AudioService audioService)
            :base(null, font)
        {
            _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
            InitializeButtons();
        }

        protected sealed override void InitializeButtons()
        {
            int screenWidth = Content.GetGraphicsDevice().Viewport.Width;
            int screenHeight = Content.GetGraphicsDevice().Viewport.Height;

            int buttonWidth = 300;
            int buttonHeight = 60;

            int startY = screenHeight / 4;
            int verticalSpacing = 20;


            _buttonTexture = CreateSimpleTexture(buttonWidth, buttonHeight);

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
                _buttons.Add(new MenuButton(
                    id: (int)buttonDefinitions[i].id,
                    texture: _buttonTexture,
                    bounds: new Rectangle((screenWidth - buttonWidth) / 2, startY + i * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                    text: buttonDefinitions[i].text,
                    font: _font,
                    audioService: _audioService
                ));
            }

            UpdateSelection(0);
        }

        protected override void HandleButtonClick(MenuButton button)
        {
            // Use _currentGameTime from ScreenBase class if GameTime is needed
            switch ((ButtonAction)button.Id)
            {
                case ButtonAction.Resume:
                    _audioService.Sounds.ResumeAll();
                    GameEvents.RaiseResumeTimer();
                    ScreenManager.RemoveScreen(this);
                    break;

                case ButtonAction.SaveGame:
                    // TODO: Save game logic
                    break;

                case ButtonAction.LoadGame:
                    // TODO: Load game logic
                    break;

                case ButtonAction.MainMenu:
                    _audioService.Sounds.StopAll();
                    _audioService.Sounds.Play(AudioAssets.SoftClick);
                    ScreenManager.ReturnToMainMenu();
                    break;

                case ButtonAction.Options:
                    OptionScreen optionScreen = new OptionScreen(_backgroundTexture, _font, _audioService);
                    optionScreen.LoadContent(Content);
                    optionScreen.GraphicsRequested += Forward;
                    ScreenManager.AddScreen(optionScreen);
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
        private void Forward(GraphicsCommand cmd)
        {
            GraphicsRequested?.Invoke(cmd);
        }

        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            var viewport = Content.GetGraphicsDevice().Viewport;
            sb.Begin();
            sb.Draw(RenderPrimitives.Pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * 0.7f);
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