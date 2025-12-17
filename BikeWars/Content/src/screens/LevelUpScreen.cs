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
    
    private int _selectedOption = 0;

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
        _selectedOption = 0;
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
        
        if (InputHandler.IsPressed(GameAction.UI_UP))
            _selectedOption = (_selectedOption + 2) % 3; // wrap-around
        else if (InputHandler.IsPressed(GameAction.UI_DOWN))
            _selectedOption = (_selectedOption + 1) % 3;

        if (InputHandler.IsPressed(GameAction.UI_CONFIRM))
        {
            SkillTree.SkillId selected = _selectedOption switch
            {
                0 => _option1,
                1 => _option2,
                2 => _option3,
                _ => _option1
            };

            OnOptionSelected?.Invoke(selected);
            Close();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen) return;
        
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1280, 720), Color.Black * 0.4f);

        Rectangle box = new Rectangle(440, 200, 400, 250);
        spriteBatch.Draw(_pixel, box, Color.DarkGray);

        // Header
        spriteBatch.DrawString(_font, "!!LEVEL UP!!", new Vector2(540, 230), Color.DarkRed);
        
        DrawOption(spriteBatch, _option1, new Vector2(480, 270), _selectedOption == 0);
        DrawOption(spriteBatch, _option2, new Vector2(480, 320), _selectedOption == 1);
        DrawOption(spriteBatch, _option3, new Vector2(480, 370), _selectedOption == 2);
    }

    private void DrawOption(SpriteBatch spriteBatch, SkillTree.SkillId option, Vector2 position, bool selected)
    {
        Color color = selected ? Color.Gold : Color.White;
        string message = SkillTree.All.ContainsKey(option) ? SkillTree.All[option] : option.ToString();
        spriteBatch.DrawString(_font, message, position, color);
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