using System;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.levelup;
using BikeWars.Content.managers;
using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// screen shown if level up is triggered if an option is selected is closes
// not in screen manager is loaded directly in gamescreen
namespace BikeWars.Content.screens;
public class LevelUpScreen : IScreen
{
    public bool IsOpen { get; private set; }

    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;

    private SkillTree.SkillId _option1;
    private SkillTree.SkillId _option2;
    private SkillTree.SkillId _option3;

    public event Action<SkillTree.SkillId> OnOptionSelected;

    public LevelUpScreen()
    {
        _pixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _font = Game1.Instance.Content.Load<SpriteFont>("assets/fonts/Arial");
    }

    public void Open(Player player)
    {
        IsOpen = true;
        // here different options can be listed, for example depending on which level it is or which where chosen before
        _option1 = SkillTree.SkillId.MoreHp;
        if (player.Attributes.CanAutoAttack)
        {
            _option2 = SkillTree.SkillId.MoreDamage;
        } else
        {
            _option2 = SkillTree.SkillId.AutomaticFire;
        }
        _option3 = SkillTree.SkillId.LongerSprintDuration;
    }
    public void Close() => IsOpen = false; // Game runs again and LevelUpScreen is closed

    public void Update(GameTime gameTime)
    {
        if (!IsOpen) return;
        // three different Options
        // can be selected by the numbers, so multiple options can be selected
        if (InputHandler.IsPressed(GameAction.INVENTORY_1))
        {
            OnOptionSelected?.Invoke(_option1);
            Close();
        }

        else if (InputHandler.IsPressed(GameAction.INVENTORY_2))
        {
            OnOptionSelected?.Invoke(_option2);
            Close();
        }

        else if (InputHandler.IsPressed(GameAction.INVENTORY_3))
        {
            OnOptionSelected?.Invoke(_option3);
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
        spriteBatch.DrawString(_font, "!!LEVEL UP!!", new Vector2(540, 230), Color.DarkRed);
        spriteBatch.DrawString(_font, "1: " +SkillTree.All[_option1], new Vector2(480, 270), Color.White);
        spriteBatch.DrawString(_font, "2: " +SkillTree.All[_option2],  new Vector2(480, 320), Color.White);
        spriteBatch.DrawString(_font, "3: " +SkillTree.All[_option3], new Vector2(480, 370), Color.White);
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