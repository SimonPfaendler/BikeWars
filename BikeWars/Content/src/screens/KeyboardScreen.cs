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

public class KeyboardScreen: MenuScreenBase, IScreen
{
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;
    private float _uiScale;
    private float _imageScale;
    private int _imageOffsetX;
    
    private Texture2D _keyboardLayoutTexture;
    public KeyboardScreen(Texture2D background, SpriteFont font, AudioService audioService)
        : base(background, font)
    {
        _audioService = audioService;
        _keyboardLayoutTexture = Game1.Instance.Content
            .Load<Texture2D>("assets/images/TastaturBelegung");
        InitializeButtons();
    }
    
    protected sealed override void InitializeButtons()
    {
        // sizes of the screens components change when the screen size changes
        _buttons.Clear();

        Game1 game = Game1.Instance;
        var viewport = game.GraphicsDevice.Viewport;
        
        _uiScale = viewport.Height / 1080f;
        
        int buttonWidth = (int)(250 * _uiScale);
        int buttonHeight = (int)(60 * _uiScale);
        int verticalSpacing = (int)(20 * _uiScale);
        int horizontalSpacing = (int)(viewport.Width * 0.05f);

        int leftStartY = (int)(viewport.Height * 0.25f);

        _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, 1, 1);

        _buttons.Add(new MenuButton(
            id: (int)ButtonAction.Back,
            texture: _buttonTexture,
            bounds: new Rectangle(
                horizontalSpacing,
                leftStartY + 3 * (buttonHeight + verticalSpacing),
                buttonWidth,
                buttonHeight),
            text: "Back",
            font: _font,
            audioService: _audioService
        ));
        
        _imageScale = 0.75f * _uiScale;
        _imageOffsetX = (int)(viewport.Width * 0.07f);
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

        // draw keyboard image
        if (_keyboardLayoutTexture != null)
        {
            var viewport = game.GraphicsDevice.Viewport;
            
            int scaledWidth = (int)(_keyboardLayoutTexture.Width * _imageScale);
            int scaledHeight = (int)(_keyboardLayoutTexture.Height * _imageScale);
            

            int x = (viewport.Width - scaledWidth) / 2 + _imageOffsetX;
            int y = (viewport.Height - scaledHeight) / 2;
            
            Rectangle destRect = new Rectangle(x, y, scaledWidth, scaledHeight);

            spriteBatch.Draw(
                _keyboardLayoutTexture,
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