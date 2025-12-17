using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using BikeWars.Content.src.utils.SaveLoadExample;

namespace BikeWars.Content.screens
{
    public class MainMenuScreen : MenuScreenBase, IScreen
    {
        private readonly AudioService _audioService;
        public string DesiredMusic => AudioAssets.MenuMusic;
        public float MusicVolume => 1f;
        public MainMenuScreen(Texture2D background, SpriteFont font, AudioService audioService)
            : base(background, font)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            InitializeButtons();
        }

        protected sealed override void InitializeButtons()
        {
            Game1 game = Game1.Instance;
            int screenWidth = game.GraphicsDevice.Viewport.Width;
            int screenHeight = game.GraphicsDevice.Viewport.Height;

            int buttonWidth = 250;
            int buttonHeight = 60;
            int verticalSpacing = 20;
            int horizontalSpacing = screenWidth / 15;

            int leftStartY = screenHeight / 7;
            int rightStartY = screenHeight / 7;

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);

            // Buttons on the left side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.NewGame,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY, buttonWidth, buttonHeight),
                text: "Neues Spiel",
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.LoadGame,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Spiel laden",
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Statistics,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + 2 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Statistiken",
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.TechDemo,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + 3 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Tech Demo",
                font: _font,
                audioService: _audioService
            ));

            // Buttons on the right side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Profile,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY, buttonWidth, buttonHeight),
                text: "Profil",
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Options,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Optionen",
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Exit,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + 2 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Beenden",
                font: _font,
                audioService: _audioService
            ));
            
            UpdateSelection(0);
        }


        protected override void HandleButtonClick(MenuButton button)
        {
            switch ((ButtonAction)button.Id)
            {
                case ButtonAction.NewGame:
                    GameConfigScreen gameConfigScreen = new GameConfigScreen(_backgroundTexture, _font, _audioService);
                    ScreenManager.AddScreen(gameConfigScreen);
                    break;

                case ButtonAction.LoadGame:
                    // Temporary we can just load the last game state.
                    try 
                    {
                        var loadedState = SaveLoad.LoadGame();
                        
                        GameMode savedMode = (GameMode)loadedState.GameMode;
                        
                        GameScreen gameScreen = new GameScreen(_audioService, savedMode);
                        
                        gameScreen.LoadContent(Game1.Instance.Content);
                        gameScreen.HandleLoadGame();

                        ScreenManager.RemoveScreen(this);
                        ScreenManager.AddScreen(gameScreen);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Fehler beim Laden: " + ex.Message);
                    }
                    break;

                case ButtonAction.Profile:
                    ProfileScreen profileScreen = new ProfileScreen(_backgroundTexture, _font, _audioService);
                    ScreenManager.AddScreen(profileScreen);
                    break;

                case ButtonAction.Statistics:
                    ScreenManager.AddScreen(new StatisticsScreen(_backgroundTexture, _font, _audioService));
                    break;

                case ButtonAction.TechDemo:
                    // TODO: Open Tech Demo Screen
                    ScreenManager.AddScreen(new TechDemoScreen(_audioService));
                    break;

                case ButtonAction.Options:
                    _audioService.Sounds.StopAll();
                    OptionScreen optionScreen = new OptionScreen(_backgroundTexture, _font, _audioService);
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
        public override bool DrawLower => false;
        public override bool UpdateLower => false;
    }
}