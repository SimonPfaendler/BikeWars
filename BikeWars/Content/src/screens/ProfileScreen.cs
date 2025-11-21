using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.managers;

namespace BikeWars.Content.screens;

public class ProfileScreen: ScreenBase, IScreen
{
    public ProfileScreen(Texture2D background, SpriteFont font)
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
            
            int leftStartY = screenHeight / 4;
            int rightStartY = screenHeight / 4;

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);

            // Buttons on the left side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.NewProfile,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY, buttonWidth, buttonHeight),
                text: "Neues Profil",
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Back,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Back", 
                font: _font
            ));

            // Buttons on the right side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.AchievementsCharacter1,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY, buttonWidth, buttonHeight),
                text: "Achievements",
                font: _font
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.AchievementsCharacter2,
                texture: _buttonTexture,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Achievements",
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
                case ButtonAction.Back:
                    ScreenManager.RemoveScreen(this);
                    break;
            
                case ButtonAction.NewProfile:
                    // TODO: Profile Creation Logic
                    break;

                case ButtonAction.AchievementsCharacter1:
                    // TODO: open achievements of first character
                    break;

                case ButtonAction.AchievementsCharacter2:
                    // TODO: open achievements of second character
                    break;
            }
        }
        
        public override bool DrawLower => false;
        public override bool UpdateLower => false;
}