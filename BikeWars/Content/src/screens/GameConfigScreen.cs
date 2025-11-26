using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;

namespace BikeWars.Content.screens;

public class GameConfigScreen : MenuScreenBase, IScreen
{
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;
    public GameConfigScreen(Texture2D background, SpriteFont font, AudioService audioService)
        :base(background, font)
    {
        _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
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
            
            int leftStartY = screenHeight / 4;
            int rightStartY = screenHeight / 4;

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);

            // Buttons on the left side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.NewProfile,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY, buttonWidth, buttonHeight),
                text: "Neues Profil",
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Back,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Back", 
                font: _font,
                audioService: _audioService
            ));

            // Buttons on the right side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Singleplayer,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY, buttonWidth, buttonHeight),
                text: "Singleplayer",
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Multiplayer,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Multiplayer",
                font: _font,
                audioService: _audioService
            ));
            
            // centre start Button
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.StartGame,
                texture: _buttonTexture,
                bounds: new Rectangle(
                    (screenWidth - buttonWidth) / 2,
                    screenHeight / 3 - buttonHeight / 2,
                    buttonWidth,
                    buttonHeight
                ),
                text: "Spiel starten",
                font: _font,
                audioService: _audioService
            ));
        }
        
        // CreateSimpleTexture might be changed for a better graphic later
        private Texture2D CreateSimpleTexture(GraphicsDevice graphicsDevice, int width, int height)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++) 
                data[i] = Color.White;
            texture.SetData(data);
            return texture;
        }
        
        
        protected override void HandleButtonClick(MenuButton button)
        {
            switch ((ButtonAction)button.Id)
            {
                case ButtonAction.StartGame:
                    GameScreen gameScreen = new GameScreen(_audioService);
                    gameScreen.LoadContent(Game1.Instance.Content);
                    ScreenManager.RemoveScreen(this);
                    ScreenManager.AddScreen(gameScreen);
                    break;
            
                case ButtonAction.Back:
                    ScreenManager.RemoveScreen(this);
                    break;
            
                case ButtonAction.NewProfile:
                    // TODO: Profile Creation Logic
                    break;

                case ButtonAction.Singleplayer:
                    // TODO: Choose Singleplayer Gamemode
                    break;

                case ButtonAction.Multiplayer:
                    // TODO: Choose Multiplayer Gamemode
                    break;
            }
        }
        public override bool DrawLower => false;
        public override bool UpdateLower => false;
}