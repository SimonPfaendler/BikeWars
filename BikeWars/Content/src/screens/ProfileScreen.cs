using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using MonoGame.Extended.Content;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens;

public class ProfileScreen: MenuScreenBase, IScreen
{
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;
    public ProfileScreen(Texture2D background, SpriteFont font, AudioService audioService, Viewport vp)
        : base(background, font, vp)
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

            int buttonWidth = 250;
            int buttonHeight = 60;
            int verticalSpacing = 20;
            int horizontalSpacing = screenWidth / 15;

            int leftStartY = screenHeight / 4;
            int rightStartY = screenHeight / 4;

            // _buttonTexture = CreateSimpleTexture(buttonWidth, buttonHeight);
            // _buttonTexture = CreateSimpleTexture(buttonWidth, buttonHeight);

            // Buttons on the left side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.NewProfile,
                // texture: _buttonTexture,
                texture: RenderPrimitives.Pixel,
                bounds: new Rectangle(horizontalSpacing, leftStartY, buttonWidth, buttonHeight),
                text: "Neues Profil",
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Back,
                texture: RenderPrimitives.Pixel,
                bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Back",
                font: _font,
                audioService: _audioService
            ));

            // Buttons on the right side
            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.AchievementsCharacter1,
                texture: RenderPrimitives.Pixel,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY, buttonWidth, buttonHeight),
                text: "Achievements",
                font: _font,
                audioService: _audioService
            ));

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.AchievementsCharacter2,
                texture: RenderPrimitives.Pixel,
                bounds: new Rectangle(screenWidth - buttonWidth - horizontalSpacing, rightStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Achievements",
                font: _font,
                audioService: _audioService
            ));

            UpdateSelection(0);
        }


        // protected override void HandleButtonClick(MenuButton button, ContentManager content, GraphicsDevice gd)
        // {
        //     switch ((ButtonAction)button.Id)
        //     {
        //         case ButtonAction.Back:
        //             // ScreenManager.RemoveScreen(this);
        //             break;

        //         case ButtonAction.NewProfile:
        //             // TODO: Profile Creation Logic
        //             break;

        //         case ButtonAction.AchievementsCharacter1:
        //             // TODO: open achievements of first character
        //             break;

        //         case ButtonAction.AchievementsCharacter2:
        //             // TODO: open achievements of second character
        //             break;
        //     }
        // }

        public override bool DrawLower => false;
        public override bool UpdateLower => false;
}