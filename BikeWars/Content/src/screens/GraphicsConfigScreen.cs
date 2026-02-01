using BikeWars.Content.components;
using BikeWars.Content.engine.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens
{
    public class GraphicsConfigScreen : MenuScreenBase, IScreen
    {
        private AudioService _audioService;
        public string DesiredMusic => AudioAssets.MenuMusic;
        public float MusicVolume => 1f;

        public GraphicsConfigScreen(Texture2D background, SpriteFont font, AudioService audioService, Viewport vp)
            : base(background, font, vp)
        {
            _audioService = audioService;
        }

        public override void LoadContent(ContentManager content, GraphicsDevice gd)
        {
            base.LoadContent(content, gd);
            string fullscreenText = gd.PresentationParameters.IsFullScreen ? "Fullscreen: ON" : "Fullscreen: OFF";
            int screenWidth = ViewPort.Width;
            int screenHeight = ViewPort.Height;

            int buttonWidth = 240;
            int buttonHeight = 50;
            int spacing = 15;
            int startY = screenHeight / 4;
            int footerY = startY + 4 * (buttonHeight + spacing) + 20;
            int centerX = (screenWidth - buttonWidth) / 2;

            AddButton(ButtonAction.ToggleFullscreen, fullscreenText, centerX, footerY, buttonWidth, buttonHeight);
            InitializeButtons();
        }

        protected sealed override void InitializeButtons()
        {
            int screenWidth = ViewPort.Width;
            int screenHeight = ViewPort.Height;

            int buttonWidth = 240;
            int buttonHeight = 50;
            int spacing = 15;


            int startY = screenHeight / 4;
            int leftColumnX = (screenWidth / 3) - (buttonWidth / 2);
            int rightColumnX = (2 * screenWidth / 3) - (buttonWidth / 2);

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

            AddButton(ButtonAction.Back, "Back", centerX, footerY + (buttonHeight + spacing), buttonWidth, buttonHeight);

            UpdateSelection(0);
        }

        private void AddButton(ButtonAction action, string text, int x, int y, int w, int h)
        {
            MenuButton mb = new MenuButton(
                id: (int)action,
                texture: RenderPrimitives.Pixel,
                bounds: new Rectangle(x, y, w, h),
                text: text,
                font: _font,
                audioService: _audioService
            );
            mb.Clicked += RaiseBtnClicked;
            _buttons.Add(mb);
        }
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
