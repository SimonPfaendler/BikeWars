using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BikeWars.Content.engine;
using System;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens;
public abstract class MenuScreenBase : IScreen, IDisposable
{
    protected Texture2D _backgroundTexture;
    protected Texture2D _buttonTexture;
    protected SpriteFont _font;
    protected List<MenuButton> _buttons;
    protected MouseState _previousMouseState;
    protected GameTime _currentGameTime;

    protected double _clickCooldown = 300;
    protected double _lastClickTime = -9999;
    protected int _selectedIndex = 0;
    protected bool _usingMouse = true;

    public Viewport ViewPort {get;set;}

    public event Action<int, IScreen> BtnClicked;

    // Resolution tracking
    protected int _lastScreenWidth;
    protected int _lastScreenHeight;
    private bool _pendingLayoutRefresh = false;

    private void RefreshLayout()
    {
        _lastScreenWidth = ViewPort.Width;
        _lastScreenHeight = ViewPort.Height;
        _buttons.Clear();
        InitializeButtons();
        UpdateSelection(0);
        _previousMouseState = Mouse.GetState();
    }

    protected MenuScreenBase(Texture2D background, SpriteFont font, Viewport viewport)
    {
        _backgroundTexture = background;
        _font = font;
        _buttons = new List<MenuButton>();
        _previousMouseState = Mouse.GetState();
        ViewPort = viewport;
    }

    protected abstract void InitializeButtons();

    public virtual void LoadContent(ContentManager content, GraphicsDevice gd)
    {
        // Track initial resolution
        RefreshLayout();
    }

    public virtual void OnViewportChanged(Viewport viewport)
    {
        ViewPort = viewport;
        RefreshLayout();
        _pendingLayoutRefresh = false;
    }

    protected void AddButton(MenuButton button)
    {
        button.Clicked += id => RaiseBtnClicked(id);
        _buttons.Add(button);
    }

    public virtual void OnActivated()
    {
        _previousMouseState = Mouse.GetState();
        _usingMouse = true;
        _lastClickTime = -9999;
        UpdateSelection(0);
    }

    public virtual void Update(GameTime gameTime)
    {
        // Check for Resolution Change or a queued external refresh
        bool layoutChanged = false;
        if (_pendingLayoutRefresh || ViewPort.Width != _lastScreenWidth || ViewPort.Height != _lastScreenHeight)
        {
            _pendingLayoutRefresh = false;
            RefreshLayout();
            layoutChanged = true;
        }

        if (layoutChanged)
        {
            // Skip input processing this frame; layout will be correct next frame.
            return;
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
            _buttons[_selectedIndex].TriggerClick();
            // HandleButtonClick(_buttons[_selectedIndex], content, gd);
        }

        _currentGameTime = gameTime;

        MouseState currentMouseState = Mouse.GetState();
        double now = gameTime.TotalGameTime.TotalMilliseconds;

        if (_lastClickTime < 0)
        {
            _lastClickTime = now;
        }

        var buttonsSnapshot = _buttons.ToArray();
        foreach (var button in buttonsSnapshot)
        {
            button.Update(currentMouseState, gameTime);

            if (button.IsHovered)
            {
                _usingMouse = true;
                int idx = _buttons.IndexOf(button);
                if (idx >= 0)
                {
                    UpdateSelection(idx);
                }
            }

            if (_usingMouse && button.IsClicked(currentMouseState, _previousMouseState))
            {
                if (now - _lastClickTime >= _clickCooldown)
                {
                    _lastClickTime = now;
                    button.TriggerClick();
                    // HandleButtonClick(button, content, gd);
                }
            }
        }

        _previousMouseState = currentMouseState;
    }

    protected void RaiseBtnClicked(int id)
    {
        BtnClicked?.Invoke(id, this);
    }

    public virtual void Draw(GameTime gameTime, SpriteBatch sb)
    {
        sb.Begin();
        Rectangle destinationRect = new Rectangle(0, 0, ViewPort.Width, ViewPort.Height);
        sb.Draw(_backgroundTexture, destinationRect, Color.White);

        foreach (var button in _buttons)
        {
            button.Draw(sb);
        }
        sb.End();
    }

    // CreateSimpleTexture might be changed for a better graphic later
    // protected Texture2D CreateSimpleTexture(int width, int height)
    // {
    //     Texture2D texture = new Texture2D(ScreenManager.GraphicsDevice, width, height);
    //     Color[] data = new Color[width * height];
    //     for (int i = 0; i < data.Length; i++)
    //         data[i] = Color.White;
    //     texture.SetData(data);
    //     return texture;
    // }

    // Every screen has to handle their own button clicks
    // protected abstract void HandleButtonClick(MenuButton button, ContentManager content, GraphicsDevice gd);

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
    }

    public void Unload()
    {

    }

    public virtual bool DrawLower => false;
    public virtual bool UpdateLower => false;
}