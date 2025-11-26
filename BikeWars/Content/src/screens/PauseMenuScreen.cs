using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using BikeWars.Content.src.screens.Overlay;

namespace BikeWars.Content.screens
{
    public class PauseMenuScreen : MenuScreenBase, IScreen
    {
        private Overlay _overlay; 
        private readonly AudioService _audioService;
        public PauseMenuScreen(SpriteFont font, AudioService audioService)
            :base(null, font)
        {
            _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
            InitializeButtons();
        }
        
        protected sealed override void InitializeButtons()
        {
            Game1 game = Game1.Instance;
            int screenWidth = game.GraphicsDevice.Viewport.Width;
            int screenHeight = game.GraphicsDevice.Viewport.Height;
    
            int buttonWidth = 300;
            int buttonHeight = 60;
    
            int startY = screenHeight / 4;
            int verticalSpacing = 20;

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);
            
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
        }
        
        private Texture2D CreateSimpleTexture(GraphicsDevice graphicsDevice, int width, int height)
        {
            // screen gets darker when game is paused
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++) 
                data[i] = Color.DarkGray;
            texture.SetData(data);
            return texture;
        }
        
        
        protected override void HandleButtonClick(MenuButton button)
        {
            // Use _currentGameTime from ScreenBase class if GameTime is needed
            switch ((ButtonAction)button.Id)
            {
                case ButtonAction.Resume:
                    _audioService.Sounds.ResumeAll();
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
                    OptionScreen optionScreen = new OptionScreen(_font, _audioService);
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
        
        public override void Draw(GameTime gameTime)
        {
            Game1 game = Game1.Instance;
            SpriteBatch spriteBatch = game.SpriteBatch;
            
            spriteBatch.Begin();
            
            Texture2D overlay = CreateOverlayTexture(game.GraphicsDevice, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(overlay, Vector2.Zero, Color.White * 0.7f);
            
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
        public override bool DrawLower => true;    // GameScreen gets drawn
        public override bool UpdateLower => false; // GameScreen doesn't get updated
    }
}