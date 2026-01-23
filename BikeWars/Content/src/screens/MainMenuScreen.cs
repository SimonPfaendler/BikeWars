using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using BikeWars.Content.src.utils.SaveLoadExample;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens
{
    public class MainMenuScreen : MenuScreenBase, IScreen
    {
        private readonly AudioService _audioService;
        public string DesiredMusic => AudioAssets.MenuMusic;
        public float MusicVolume => 1f;
        public event Action Exit;
        public event Action<GraphicsCommand> GraphicsRequested;
        public MainMenuScreen(Texture2D background, SpriteFont font, AudioService audioService, Viewport vp)
            : base(background, font, vp)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        }

        public override void LoadContent(ContentManager content, GraphicsDevice gd)
        {
            base.LoadContent(content, gd);
        }

        protected sealed override void InitializeButtons()
        {
            int screenWidth = ViewPort.Width;
            int screenHeight = ViewPort.Height;

            int buttonWidth = 250;
            int buttonHeight = 60;
            int verticalSpacing = 20;
            int horizontalSpacing = screenWidth / 15;

            int leftStartY = screenHeight / 7;
            int rightStartY = screenHeight / 7;

            // _buttonTexture = CreateSimpleTexture(buttonWidth, buttonHeight);

            // Buttons on the left side
            MenuButton mb = new MenuButton(
                id: (int)ButtonAction.NewGame,
                texture: RenderPrimitives.Pixel,
                // texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY, buttonWidth, buttonHeight),
                text: "Neues Spiel",
                font: _font,
                audioService: _audioService
            );
            mb.Clicked += RaiseBtnClicked;
            _buttons.Add(mb);

            mb = new MenuButton(
                id: (int)ButtonAction.LoadGame,
                texture: RenderPrimitives.Pixel,
                // texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Spiel laden",
                font: _font,
                audioService: _audioService
            );
            mb.Clicked += RaiseBtnClicked;
            _buttons.Add(mb);

            mb = new MenuButton(
                id: (int)ButtonAction.Statistics,
                texture: RenderPrimitives.Pixel,
                // texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + 2 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Statistiken",
                font: _font,
                audioService: _audioService
            );
            mb.Clicked += RaiseBtnClicked;
            _buttons.Add(mb);

            mb = new MenuButton(
                id: (int)ButtonAction.TechDemo,
                texture: RenderPrimitives.Pixel,
                // texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + 3 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Tech Demo",
                font: _font,
                audioService: _audioService
            );

            mb.Clicked += RaiseBtnClicked;
            _buttons.Add(mb);

            mb = new MenuButton(
                id: (int)ButtonAction.Profile,
                texture: RenderPrimitives.Pixel,
                // texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY, buttonWidth, buttonHeight),
                text: "Profil",
                font: _font,
                audioService: _audioService
            );

            mb.Clicked += RaiseBtnClicked;

            // Buttons on the right side
            _buttons.Add(mb);

            mb = new MenuButton(
                id: (int)ButtonAction.Options,
                texture: RenderPrimitives.Pixel,
                // texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Optionen",
                font: _font,
                audioService: _audioService
            );
            mb.Clicked += RaiseBtnClicked;
            _buttons.Add(mb);

            mb = new MenuButton(
                id: (int)ButtonAction.Exit,
                texture: RenderPrimitives.Pixel,
                // texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + 2 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Beenden",
                font: _font,
                audioService: _audioService
            );
            mb.Clicked += RaiseBtnClicked;
            _buttons.Add(mb);

            UpdateSelection(0);
        }

        protected void HandleButtonClick(MenuButton button, ContentManager content, GraphicsDevice gd)
        {

        }

        private void Forward(GraphicsCommand cmd)
        {
            GraphicsRequested?.Invoke(cmd);
        }

        private void OnExit()
        {
            Exit?.Invoke();
        }
        public virtual void Dispose()
        {

        }

        public override bool DrawLower => false;
        public override bool UpdateLower => false;
    }
}