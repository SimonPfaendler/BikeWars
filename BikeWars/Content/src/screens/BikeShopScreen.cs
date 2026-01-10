using System;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
using BikeWars.Entities.Characters;
using BikeWars.Entities.Characters.MapObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BikeWars.Content.screens;

public class BikeShopScreen : IScreen
{
    public bool IsOpen { get; private set; }

    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;

    public enum ShopOption
    {
        HealFull,
        RepairBike,
        Close
    }

    private readonly ShopOption _option1 = ShopOption.HealFull;
    private readonly ShopOption _option2 = ShopOption.RepairBike;
    private readonly ShopOption _option3 = ShopOption.Close;

    private int _selectedOption = 0;

    private Player _player;

    public event Action Closed;

    public BikeShopScreen()
    {
        _pixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _font = Game1.Instance.Content.Load<SpriteFont>("assets/fonts/Arial");
    }

    public void Open(Player player, BikeShop shop)
    {
        IsOpen = true;
        _selectedOption = 0;

        _player = player;
    }

    public void Close()
    {
        IsOpen = false;
        Closed?.Invoke();
    }

    public void Update(GameTime gameTime)
    {
        if (!IsOpen) return;
        var ks = Keyboard.GetState();
        if (InputHandler.IsPressed(GameAction.PAUSE))
        {
            Close();
            return;
        }


        if (ks.IsKeyDown(Keys.D1) || ks.IsKeyDown(Keys.NumPad1))
        {
            ApplyOption(_option1);
            Close();
            return;
        }

        if (ks.IsKeyDown(Keys.D2) || ks.IsKeyDown(Keys.NumPad2))
        {
            ApplyOption(_option2);
            Close();
            return;
        }

        if (ks.IsKeyDown(Keys.D3) || ks.IsKeyDown(Keys.NumPad3))
        {
            ApplyOption(_option3);
            Close();
        }
    }


    private void ApplyOption(ShopOption option)
    {
        if (_player == null) return;

        switch (option)
        {
            //TODO Option don't change anything
            case ShopOption.HealFull:
                _player.Attributes.Health = _player.Attributes.MaxHealth;
                break;

            case ShopOption.RepairBike:
                if (_player.CurrentBike != null)
                {
                    //TODO add logic
                }

                break;

            case ShopOption.Close:
            default:
                break;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen) return;

        var viewport = Game1.Instance.GraphicsDevice.Viewport;
        int screenW = viewport.Width;
        int screenH = viewport.Height;

        // Fullscreen overlay
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, screenW, screenH), Color.Black * 0.4f);

        int boxW = 400;
        int boxH = 250;
        int boxX = (screenW - boxW) / 2;
        int boxY = (screenH - boxH) / 2;

        Rectangle box = new Rectangle(boxX, boxY, boxW, boxH);
        spriteBatch.Draw(_pixel, box, Color.DarkGray);

        string title = "BIKE SHOP";
        Vector2 titleSize = _font.MeasureString(title);
        Vector2 titlePos = new Vector2(
            boxX + (boxW - titleSize.X) / 2,
            boxY + 30
        );
        spriteBatch.DrawString(_font, title, titlePos, Color.DarkRed);

        // Options
        int startOptionY = boxY + 70;
        int optionSpacing = 50;
        int textX = boxX + 40;

        DrawOption(spriteBatch, _option1, new Vector2(textX, startOptionY), _selectedOption == 0);
        DrawOption(spriteBatch, _option2, new Vector2(textX, startOptionY + optionSpacing), _selectedOption == 1);
        DrawOption(spriteBatch, _option3, new Vector2(textX, startOptionY + optionSpacing * 2), _selectedOption == 2);
    }

    private void DrawOption(SpriteBatch spriteBatch, ShopOption option, Vector2 position, bool selected)
    {
        Color color = selected ? Color.Gold : Color.White;

        string message = option switch
        {
            ShopOption.HealFull => "Druecke 1: Leben auf Max",
            ShopOption.RepairBike => "Druecke 2: Fahrrad Leben auf Max",
            ShopOption.Close => "Druecke 3: Close",
            _ => option.ToString()
        };

        spriteBatch.DrawString(_font, message, position, color);
    }

    // Just for IScreen
    public ScreenManager ScreenManager
    {
        get => null;
        set { }
    }
    public bool DrawLower => false;
    public bool UpdateLower => false;

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Draw(GameTime gameTime)
    {
    }
}
