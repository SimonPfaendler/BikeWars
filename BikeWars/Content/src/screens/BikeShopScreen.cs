using System;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
using BikeWars.Entities.Characters;
using BikeWars.Entities.Characters.MapObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BikeWars.Content.components;
using BikeWars.Content.entities.interfaces;

namespace BikeWars.Content.screens;

public class BikeShopScreen : IScreen
{
    public bool IsOpen { get; private set; }

    public enum ShopOption
    {
        HealFull,
        RepairBike,
        BuyFrelo,
        BuyRacingBike,
        Close
    }

    private readonly ShopOption _option1 = ShopOption.HealFull;
    private readonly ShopOption _option2 = ShopOption.RepairBike;
    private readonly ShopOption _option3 = ShopOption.BuyFrelo;
    private readonly ShopOption _option4 = ShopOption.BuyRacingBike;
    private readonly ShopOption _option5 = ShopOption.Close;

    public Viewport ViewPort {get;set;}
    public event Action<int, IScreen> BtnClicked { add { } remove { } }
    public event Action<Bike> Repair;
    public event Action<Vector2> SpawnFrelo;

    private BikeShop _shop;
    public event Action<Vector2> SpawnRacingBike;

    private int _selectedOption = 0;

    private Player _player;

    public event Action Closed;

    public BikeShopScreen(Viewport vp)
    {
        ViewPort = vp;
    }

    public void Open(Player player, BikeShop shop)
    {
        IsOpen = true;
        _selectedOption = 0;
        _shop = shop;
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
        if (InputHandler.IsPressed(GameAction.UI_UP))
            _selectedOption = (_selectedOption + 4) % 5; // wie -1
        else if (InputHandler.IsPressed(GameAction.UI_DOWN))
            _selectedOption = (_selectedOption + 1) % 5;

        if (ks.IsKeyDown(Keys.D1) || ks.IsKeyDown(Keys.NumPad1))
        {
            if (ApplyOption(_option1))
            {Close();}
            return;
        }

       else if (ks.IsKeyDown(Keys.D2) || ks.IsKeyDown(Keys.NumPad2))
        {
            if (ApplyOption(_option2))
            {Close();}
            return;
        }

        else if (ks.IsKeyDown(Keys.D3) || ks.IsKeyDown(Keys.NumPad3))
        {
            if (ApplyOption(_option3))
            {Close();}

            return;
        }
        else if (ks.IsKeyDown(Keys.D4) || ks.IsKeyDown(Keys.NumPad4))
        {
            if (ApplyOption(_option4))
            {Close();}

            return;
        }
        else if (ks.IsKeyDown(Keys.D5) || ks.IsKeyDown(Keys.NumPad5))
        {
            ApplyOption(_option5);
            Close();
            return;
        }
        if (InputHandler.IsPressed(GameAction.UI_CONFIRM))
        {
            ShopOption selected = _selectedOption switch
            {
                0 => _option1,
                1 => _option2,
                2 => _option3,
                3 => _option4,
                _ => _option5
            };

            if (ApplyOption(selected))
                Close();
        };
    }


    private bool ApplyOption(ShopOption option)
    {
        if (_player == null) return false;

        switch (option)
        {
            case ShopOption.HealFull:
                _player.Attributes.Health = _player.Attributes.MaxHealth;
                _shop.SetCooldown(120);
                return true;

            case ShopOption.RepairBike:
                if (_player.CurrentBike == null)
                    return false;
                if (_player.TrySpendXp(10))
                {
                    Bike bike = _player.CurrentBike;

                    Repair?.Invoke(bike);
                    _shop.SetCooldown(15);
                    return true;
                }
                return false;
        case ShopOption.BuyFrelo:
                if (_player.TrySpendXp(15))
                {
                    var dropPos = _shop.Transform.Position + new Vector2(50, -50);
                    SpawnFrelo?.Invoke(dropPos);
                    _shop.SetCooldown(20);
                    return true;
                }
                return false;
            case ShopOption.BuyRacingBike:
                if (_player.TrySpendXp(20))
                {
                    var dropPos1 = _shop.Transform.Position + new Vector2(50, -50);
                    SpawnRacingBike?.Invoke(dropPos1);
                    _shop.SetCooldown(20);
                    return true;
                }
                return false;
            case ShopOption.Close:
            default:
                return true;
        }
    }

    public void Draw(GameTime gameTime, SpriteBatch sb)
    {
        if (!IsOpen) return;

        int screenW = ViewPort.Width;
        int screenH = ViewPort.Height;

        // Fullscreen overlay
        sb.Draw(RenderPrimitives.Pixel, new Rectangle(0, 0, screenW, screenH), Color.Black * 0.4f);

        int boxW = 400;
        int boxH = 350;
        int boxX = (screenW - boxW) / 2;
        int boxY = (screenH - boxH) / 2;

        Rectangle box = new Rectangle(boxX, boxY, boxW, boxH);
        sb.Draw(RenderPrimitives.Pixel, box, Color.DarkGray);

        string title = "BIKE SHOP";
        Vector2 titleSize = UIAssets.DefaultFont.MeasureString(title);
        Vector2 titlePos = new Vector2(
            boxX + (boxW - titleSize.X) / 2,
            boxY + 30
        );
        sb.DrawString(UIAssets.DefaultFont, title, titlePos, Color.DarkRed);

        // Options
        int startOptionY = boxY + 70;
        int optionSpacing = 50;
        int textX = boxX + 40;

        DrawOption(sb, _option1, new Vector2(textX, startOptionY), _selectedOption == 0);
        DrawOption(sb, _option2, new Vector2(textX, startOptionY + optionSpacing), _selectedOption == 1);
        DrawOption(sb, _option3, new Vector2(textX, startOptionY + optionSpacing * 2), _selectedOption == 2);
        DrawOption(sb, _option4, new Vector2(textX, startOptionY + optionSpacing * 3), _selectedOption == 3);
        DrawOption(sb, _option5, new Vector2(textX, startOptionY + optionSpacing * 4), _selectedOption == 4);
    }

    private void DrawOption(SpriteBatch sb, ShopOption option, Vector2 position, bool selected)
    {
        Color color = selected ? Color.Gold : Color.White;
        int xp = _player.XpCounter;

        string message = option switch
        {
            ShopOption.HealFull => "Leben auf Max | 120s Cooldown",
            ShopOption.RepairBike => $"Fahrrad Leben auf Max {xp}Xp/10Xp | 20s Cooldown",
            ShopOption.BuyFrelo => $"Kaufe ein Frelo {xp}Xp/15Xp | 20s Cooldown",
            ShopOption.BuyRacingBike  => $"Kaufe ein Rennrad {xp}Xp/20Xp | 20s Cooldown",
            ShopOption.Close => "Close",

            _ => option.ToString()
        };

        sb.DrawString(UIAssets.DefaultFont, message, position, color);
    }

    // Just for IScreen
    public ScreenManager ScreenManager
    {
        get => null;
        set { }
    }
    public bool DrawLower => false;
    public bool UpdateLower => false;

    public virtual void Dispose() {
    }
    public void OnActivated()
    {
        throw new NotImplementedException();
    }

    public void Unload() {
    }
}
