using System;
using BikeWars.Content.components;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.screens
{
    public class GraphicsConfigScreen : MenuScreenBase, IScreen
    {
        private AudioService _audioService;
        public string DesiredMusic => AudioAssets.MenuMusic;
        public float MusicVolume => 1f;

        public GraphicsConfigScreen(Texture2D background, SpriteFont font, AudioService audioService)
            : base(background, font)
        {
            _audioService = audioService;
            InitializeButtons();
        }

        protected sealed override void InitializeButtons()
        {
            Game1 game = Game1.Instance;
            int screenWidth = game.GraphicsDevice.Viewport.Width;
            int screenHeight = game.GraphicsDevice.Viewport.Height;

            int buttonWidth = 240;
            int buttonHeight = 50;
            int spacing = 15;

            
            int startY = screenHeight / 4;
            int leftColumnX = (screenWidth / 3) - (buttonWidth / 2);
            int rightColumnX = (2 * screenWidth / 3) - (buttonWidth / 2);

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);

            
            AddButton(ButtonAction.Resolution1920x1080, "1920 x 1080", leftColumnX, startY, buttonWidth, buttonHeight);
            AddButton(ButtonAction.Resolution1536x864, "1536 x 864", leftColumnX, startY + 1 * (buttonHeight + spacing), buttonWidth, buttonHeight);
            AddButton(ButtonAction.Resolution1280x720, "1280 x 720", leftColumnX, startY + 2 * (buttonHeight + spacing), buttonWidth, buttonHeight);
            AddButton(ButtonAction.Resolution800x600, "800 x 600", leftColumnX, startY + 3 * (buttonHeight + spacing), buttonWidth, buttonHeight);

            
            AddButton(ButtonAction.ResolutionPortrait1080x1920, "1080 x 1920", rightColumnX, startY, buttonWidth, buttonHeight);
            AddButton(ButtonAction.ResolutionPortrait864x1536, "864 x 1536", rightColumnX, startY + 1 * (buttonHeight + spacing), buttonWidth, buttonHeight);
            AddButton(ButtonAction.ResolutionPortrait720x1280, "720 x 1280", rightColumnX, startY + 2 * (buttonHeight + spacing), buttonWidth, buttonHeight);
            AddButton(ButtonAction.ResolutionPortrait600x800, "600 x 800", rightColumnX, startY + 3 * (buttonHeight + spacing), buttonWidth, buttonHeight);


            
            int footerY = startY + 4 * (buttonHeight + spacing) + 20;
            int centerX = (screenWidth - buttonWidth) / 2;

            string fullscreenText = game.GraphicsDevice.PresentationParameters.IsFullScreen ? "Fullscreen: ON" : "Fullscreen: OFF";
            AddButton(ButtonAction.ToggleFullscreen, fullscreenText, centerX, footerY, buttonWidth, buttonHeight);
            
            AddButton(ButtonAction.Back, "Back", centerX, footerY + (buttonHeight + spacing), buttonWidth, buttonHeight);

            UpdateSelection(0);
        }

        private void AddButton(ButtonAction action, string text, int x, int y, int w, int h)
        {
            _buttons.Add(new MenuButton(
                id: (int)action,
                texture: _buttonTexture,
                bounds: new Rectangle(x, y, w, h),
                text: text,
                font: _font,
                audioService: _audioService
            ));
        }

        protected override void HandleButtonClick(MenuButton button)
        {
            var game = Game1.Instance;

            switch ((ButtonAction)button.Id)
            {
                case ButtonAction.Resolution1920x1080:
                    game.SetResolution(1920, 1080, false);
                    break;
                case ButtonAction.Resolution1536x864:
                    game.SetResolution(1536, 864, false);
                    break;
                case ButtonAction.Resolution1280x720:
                    game.SetResolution(1280, 720, false);
                    break;
                case ButtonAction.Resolution800x600:
                    game.SetResolution(800, 600, false);
                    break;

                case ButtonAction.ResolutionPortrait1080x1920:
                    game.SetResolution(1080, 1920, false);
                    break;
                case ButtonAction.ResolutionPortrait864x1536:
                    game.SetResolution(864, 1536, false);
                    break;
                case ButtonAction.ResolutionPortrait720x1280:
                    game.SetResolution(720, 1280, false);
                    break;
                case ButtonAction.ResolutionPortrait600x800:
                    game.SetResolution(600, 800, false);
                    break;

                case ButtonAction.ToggleFullscreen:
                    bool currentFs = game.GraphicsDevice.PresentationParameters.IsFullScreen;
                    game.SetResolution(game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height, !currentFs);
                    break;
                case ButtonAction.Back:
                    ScreenManager.RemoveScreen(this);
                    break;
            }
        }
    }
}
