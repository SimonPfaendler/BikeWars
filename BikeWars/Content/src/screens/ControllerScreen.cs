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

public class ControllerScreen: MenuScreenBase, IScreen
{
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;
    private Texture2D _controllerLayoutTexture;
    public ControllerScreen(Texture2D background, SpriteFont font, AudioService audioService)
        : base(background, font)
    {
        _audioService = audioService;
        _controllerLayoutTexture = Game1.Instance.Content
            .Load<Texture2D>("assets/images/ControllerBelegung");
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

            _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);

            // Buttons on the left side

            _buttons.Add(new MenuButton(
                id: (int)ButtonAction.Back,
                texture: _buttonTexture,
                bounds: new Rectangle(horizontalSpacing, leftStartY + 3 * (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
                text: "Back",
                font: _font,
                audioService: _audioService
            ));
            
            UpdateSelection(0);
        }
    
    public override void Draw(GameTime gameTime)
    {
        Game1 game = Game1.Instance;
        SpriteBatch spriteBatch = game.SpriteBatch;

        spriteBatch.Begin();
        
        Rectangle destinationRect = new Rectangle(
            0, 0,
            game.GraphicsDevice.Viewport.Width,
            game.GraphicsDevice.Viewport.Height);

        spriteBatch.Draw(_backgroundTexture, destinationRect, Color.White);

        // draw controller image
        if (_controllerLayoutTexture != null)
        {
            var viewport = game.GraphicsDevice.Viewport;
            
            float scale = 0.5f;
            
            int scaledWidth = (int)(_controllerLayoutTexture.Width * scale);
            int scaledHeight = (int)(_controllerLayoutTexture.Height * scale);
            
            int offsetX = 140;

            int x = (viewport.Width - scaledWidth) / 2 + offsetX;
            int y = (viewport.Height - scaledHeight) / 2;
            
            Rectangle destRect = new Rectangle(x, y, scaledWidth, scaledHeight);

            spriteBatch.Draw(
                _controllerLayoutTexture,
                destRect,
                Color.White
            );
        }
        
        // draw buttons
        foreach (var button in _buttons)
        {
            button.Draw(spriteBatch);
        }

        spriteBatch.End();
    }


        
    protected override void HandleButtonClick(MenuButton button)
    {
        switch ((ButtonAction)button.Id)
        {
            case ButtonAction.Back:
                ScreenManager.RemoveScreen(this);
                break;
        }
    }
    public override bool DrawLower => false;
    public override bool UpdateLower => false;
}