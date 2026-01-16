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

    private SkillTree.SkillId _option1;
    private SkillTree.SkillId _option2;
    private SkillTree.SkillId _option3;

    private int _selectedOption = 0;

    public event Action<SkillTree.SkillId> OnOptionSelected;
    public event Action Closed;

    public LevelUpScreen()
    {
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
    public void Close()  // Game runs again and LevelUpScreen is closed
    {
        IsOpen = false;
        Closed?.Invoke();
    }


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

    public void Draw(GameTime gameTime, SpriteBatch sb)
    {
        if (!IsOpen) return;

        var viewport = sb.GraphicsDevice.Viewport;
        int screenW = viewport.Width;
        int screenH = viewport.Height;

        // Fullscreen overlay
        sb.Draw(RenderPrimitives.Pixel, new Rectangle(0, 0, screenW, screenH), Color.Black * 0.4f);

        int boxW = 400;
        int boxH = 250;
        int boxX = (screenW - boxW) / 2;
        int boxY = (screenH - boxH) / 2;

        Rectangle box = new Rectangle(boxX, boxY, boxW, boxH);
        sb.Draw(RenderPrimitives.Pixel, box, Color.DarkGray);

        // Header
        string title = "!!LEVEL UP!!";
        Vector2 titleSize = UIAssets.DefaultFont.MeasureString(title);
        Vector2 titlePos = new Vector2(
            boxX + (boxW - titleSize.X) / 2,
            boxY + 30
        );
        sb.DrawString(UIAssets.DefaultFont, title, titlePos, Color.DarkRed);

        // Options
        int startOptionY = boxY + 70;
        int optionSpacing = 50;
        // Center text in box - approximate X offset or use generic padding
        int textX = boxX + 40;

        DrawOption(sb, _option1, new Vector2(textX, startOptionY), _selectedOption == 0);
        DrawOption(sb, _option2, new Vector2(textX, startOptionY + optionSpacing), _selectedOption == 1);
        DrawOption(sb, _option3, new Vector2(textX, startOptionY + optionSpacing * 2), _selectedOption == 2);
    }

    private void DrawOption(SpriteBatch spriteBatch, SkillTree.SkillId option, Vector2 position, bool selected)
    {
        Color color = selected ? Color.Gold : Color.White;
        string message = SkillTree.All.ContainsKey(option) ? SkillTree.All[option] : option.ToString();
        spriteBatch.DrawString(UIAssets.DefaultFont, message, position, color);
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
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}