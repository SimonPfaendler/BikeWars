using System;
using BikeWars;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// screen shon if level up is triggered if an option is selected is closes
// not in screen manager is loaded directly in gamescreen
namespace BikeWars.Content.screens;
public class LevelUpScreen : IScreen
{
    public bool IsOpen { get; private set; }

    public event Action OnOptionLeftSelected;
    public event Action OnOptionRightSelected;

    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;

    public LevelUpScreen()
    {
        _pixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _font = Game1.Instance.Content.Load<SpriteFont>("assets/fonts/Arial");
    }

    public void Open()  => IsOpen = true;
    public void Close() => IsOpen = false;

    public void Update(GameTime gameTime)
    {
        if (!IsOpen) return;
        // two different Options 
        if (InputHandler.Mouse.Pressed(MouseButton.Left))
        {
            OnOptionLeftSelected?.Invoke();
            Close();
        }
        
        if (InputHandler.Mouse.Pressed(MouseButton.Right))
        {
            OnOptionRightSelected?.Invoke();
            Close();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen) return;
        // makes backgound a little bit darker
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1280, 720), Color.Black * 0.4f);

        Rectangle box = new Rectangle(440, 200, 400, 250);
        spriteBatch.Draw(_pixel, box, Color.DarkGray);
        // shown text
        spriteBatch.DrawString(_font, "Level Up!", new Vector2(520, 230), Color.White);
        spriteBatch.DrawString(_font, "Left Click: Option 1",  new Vector2(480, 300), Color.White);
        spriteBatch.DrawString(_font, "Right Click: Option 2", new Vector2(480, 350), Color.White);
    }

    // the code below doesn't do anything its just for IScreen
    public ScreenManager ScreenManager
    {
        get => null;
        set { }
    }
    public bool DrawLower  => false; 
    public bool UpdateLower => false;
    public void OnEnter() { }
    public void OnExit()  { }
    public void Draw(GameTime gameTime)
    {
    }
}