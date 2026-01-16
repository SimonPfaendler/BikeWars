using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.engine;
using BikeWars.Content.managers;
using System;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Content;

namespace BikeWars.Content.screens;
public abstract class MenuScreenBase : IScreen, IDisposable
{
    protected ContentManager Content { get; private set; }
    protected Texture2D _backgroundTexture;
    protected Texture2D _buttonTexture;
    protected SpriteFont _font;
    protected List<MenuButton> _buttons;
    protected MouseState _previousMouseState;
    protected GameTime _currentGameTime;
    public ScreenManager ScreenManager { get; set; }

    protected double _clickCooldown = 300;
    protected double _lastClickTime = -9999;
    protected int _selectedIndex = 0;
    protected bool _usingMouse = true;

    // Resolution tracking
    protected int _lastScreenWidth;
    protected int _lastScreenHeight;

    protected MenuScreenBase(Texture2D background, SpriteFont font)
    {
        _backgroundTexture = background;
        _font = font;
        _buttons = new List<MenuButton>();
        _previousMouseState = Mouse.GetState();
    }

    protected abstract void InitializeButtons();

    public virtual void LoadContent(ContentManager contentManager)
    {
        Content = new ContentManager(contentManager.ServiceProvider, contentManager.RootDirectory);
        // Track initial resolution
        Viewport vp = Content.GetGraphicsDevice().Viewport;
        _lastScreenWidth = vp.Width;
        _lastScreenHeight = vp.Height;
        InitializeButtons();
    }

    public virtual void Update(GameTime gameTime)
    {
        // Check for Resolution Change
        var vp = Content.GetGraphicsDevice().Viewport;
        if (vp.Width != _lastScreenWidth || vp.Height != _lastScreenHeight)
        {
            _lastScreenWidth = vp.Width;
            _lastScreenHeight = vp.Height;
            _buttons.Clear();
            InitializeButtons();
        }

        // Controller / Keyboard Navigation
        if (InputHandler.IsPressed(GameAction.UI_DOWN))
        {
            _usingMouse = false;
            UpdateSelection(_selectedIndex + 1);
        }
        else if (InputHandler.IsPressed(GameAction.UI_UP))
        {
            _usingMouse = false;
            UpdateSelection(_selectedIndex - 1);
        }

        if (InputHandler.IsPressed(GameAction.UI_CONFIRM))
        {
            _usingMouse = false;
            HandleButtonClick(_buttons[_selectedIndex]);
            return;
        }

        _currentGameTime = gameTime;

        MouseState currentMouseState = Mouse.GetState();
        double now = gameTime.TotalGameTime.TotalMilliseconds;

        if (_lastClickTime < 0)
        {
            _lastClickTime = now;
        }

        foreach (var button in _buttons)
        {
            button.Update(currentMouseState, gameTime);

            if (button.IsHovered)
            {
                _usingMouse = true;
                UpdateSelection(_buttons.IndexOf(button));
            }

            if (_usingMouse && button.IsClicked(currentMouseState, _previousMouseState))
            {
                if (now - _lastClickTime >= _clickCooldown)
                {
                    _lastClickTime = now;
                    HandleButtonClick(button);
                }
            }
        }

        _previousMouseState = currentMouseState;
    }

    public virtual void Draw(GameTime gameTime, SpriteBatch sb)
    {
        sb.Begin();
        Rectangle destinationRect = new Rectangle(0, 0, Content.GetGraphicsDevice().Viewport.Width, Content.GetGraphicsDevice().Viewport.Height);
        sb.Draw(_backgroundTexture, destinationRect, Color.White);

        foreach (var button in _buttons)
        {
            button.Draw(sb);
        }
        sb.End();
    }

    // CreateSimpleTexture might be changed for a better graphic later
    protected Texture2D CreateSimpleTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(Content.GetGraphicsDevice(), width, height);
        Color[] data = new Color[width * height];
        for (int i = 0; i < data.Length; i++)
            data[i] = Color.White;
        texture.SetData(data);
        return texture;
    }

    // Every screen has to handle their own button clicks
    protected abstract void HandleButtonClick(MenuButton button);

    // helper-method for selecting a button
    protected void UpdateSelection(int newIndex)
    {
        if (_buttons.Count == 0) return;

        _selectedIndex = (newIndex + _buttons.Count) % _buttons.Count;

        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].IsSelected = (i == _selectedIndex);
        }
    }
    public virtual void Dispose()
    {
        _buttonTexture?.Dispose();
        _buttonTexture = null;

        Content?.Unload();
        Content = null;
    }

    public virtual bool DrawLower => false;
    public virtual bool UpdateLower => false;
}