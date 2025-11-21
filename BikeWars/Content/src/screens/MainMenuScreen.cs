using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.managers;

namespace BikeWars.Content.screens
{
    public class MainMenuScreen : ScreenBase, IScreen
    {
        
        public MainMenuScreen(Texture2D background, SpriteFont font)
            : base(background, font)
        {
            
        }
        
        protected override void InitializeButtons()
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
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.LoadGame,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Spiel laden", 
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Statistics,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + 2 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Statistiken",
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.TechDemo,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + 3 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Tech Demo",
                font: _font
            ));

            // Buttons on the right side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Profile,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY, buttonWidth, buttonHeight),
                text: "Profil",
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Options,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Optionen",
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Exit,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + 2 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Beenden",
                font: _font
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
                case ButtonAction.NewGame:
                    GameConfigScreen gameConfigScreen = new GameConfigScreen(_backgroundTexture, _font);
                    ScreenManager.AddScreen(gameConfigScreen);
                    break;
            
                case ButtonAction.LoadGame:
                    // TODO: Load game Logic
                    break;
            
                case ButtonAction.Profile:
                    ProfileScreen profileScreen = new ProfileScreen(_backgroundTexture, _font);
                    ScreenManager.AddScreen(profileScreen);
                    break;

                case ButtonAction.Statistics:
                    // TODO: Open Statistics Screen
                    break;

                case ButtonAction.TechDemo:
                    // TODO: Open Tech Demo Screen
                    break;

                case ButtonAction.Options:
                    OptionScreen optionScreen = new OptionScreen(_font);
                    ScreenManager.AddScreen(optionScreen);
                    break;
            
                case ButtonAction.Exit:
                    ConfirmationDialogScreen confirmDialog = new ConfirmationDialogScreen(
                        _font, 
                        "Bist Du Dir sicher?", 
                        this
                    );
                    ScreenManager.AddScreen(confirmDialog);
                    break;
            }
        }
        public override bool DrawLower => false;
        public override bool UpdateLower => false;
    }
}