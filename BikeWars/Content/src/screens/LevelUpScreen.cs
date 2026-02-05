using System;
using System.Collections.Generic;
using BikeWars.Utilities;
using BikeWars.Content.components;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.levelup;
using BikeWars.Content.managers;
using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// screen shown if level up is triggered if an option is selected is closes
// not in screen manager is loaded directly in gamescreen
namespace BikeWars.Content.screens;
public class LevelUpScreen : MenuScreenBase, IScreen
{
    public bool IsOpen { get; private set; }
    private readonly string _message;
    private readonly AudioService _audioService;

    private SkillTree.SkillId _option1;
    private SkillTree.SkillId _option2;
    private SkillTree.SkillId _option3;

    private int _selectedOption = 0;

    public event Action<SkillTree.SkillId> OnOptionSelected;
    public event Action Closed;

    public LevelUpScreen(SpriteFont font, string message, AudioService audioService, Viewport vp): base(null, font, vp)
    {
        _message = message;
        _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
    }

    public override void LoadContent(ContentManager content, GraphicsDevice gd)
    {
        base.LoadContent(content, gd);
        InitializeButtons();
    }

    public void Open(Player player)
    {
        if (player == null) return;

        IsOpen = true;
        _selectedOption = 0;
        // here different options can be listed, for example depending on which level it is or which where chosen before
        var possibleSkills = new List<SkillTree.SkillId>
        if (player.PlayerNumber == 1)
        {LevelUpPlayer1(player);}
        else
        {
            SkillTree.SkillId.MoreHp,
            SkillTree.SkillId.MoreDamage,
            SkillTree.SkillId.CritChance,
            SkillTree.SkillId.LongerSprintDuration
        };

        if (!player.Attributes.CanAutoAttack)
        {
            possibleSkills.Add(SkillTree.SkillId.AutomaticFire);
        }

        // Add weapons if not max level
        if (player.GetWeaponLevel(Player.WeaponType.Gun) < 5)
             possibleSkills.Add(SkillTree.SkillId.WeaponGun);
        
        if (player.GetWeaponLevel(Player.WeaponType.BananaThrow) < 5)
             possibleSkills.Add(SkillTree.SkillId.WeaponBanana);

        if (player.GetWeaponLevel(Player.WeaponType.BottleThrow) < 5)
             possibleSkills.Add(SkillTree.SkillId.WeaponBottle);
        int n = possibleSkills.Count;
        while (n > 1)
        {
            n--;
            int k = RandomUtil.NextInt(0, n + 1);
            var value = possibleSkills[k];
            possibleSkills[k] = possibleSkills[n];
            possibleSkills[n] = value;
        }

        _option1 = possibleSkills[0 % possibleSkills.Count];
        _option2 = possibleSkills[1 % possibleSkills.Count];
        _option3 = possibleSkills[2 % possibleSkills.Count];
            LevelUpPlayer2(player);
        }
            
        
    }
    public void Close()  // Game runs again and LevelUpScreen is closed
    {
        IsOpen = false;
        Closed?.Invoke();
    }

    public override void Update(GameTime gameTime)
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
    protected sealed override void InitializeButtons()
    {
    }

    public override void Draw(GameTime gameTime, SpriteBatch sb)
    {
        if (!IsOpen) return;

        int screenW = ViewPort.Width;
        int screenH = ViewPort.Height;

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
    public override bool DrawLower  => false;
    public override bool UpdateLower => false;

    public void OnEnter() { }
    public void OnExit()  { }
    public void Draw(GameTime gameTime)
    {
    }
    public override void Dispose()
    {
        throw new NotImplementedException();
    }

    public new void Unload()
    {
    }

    public override void OnActivated()
    {
        throw new NotImplementedException();
    }
    private void LevelUpPlayer1(Player player)
    {if (player.CurrentLevel == 2)
        {
            _option1 = SkillTree.SkillId.WeaponGun;
            _option2 = SkillTree.SkillId.WeaponBanana;
            _option3 = SkillTree.SkillId.WeaponBottle;
        }
        else
        {
            _option1 = SkillTree.SkillId.MoreHp;
            if (player.Attributes.CanAutoAttack)
            {
                _option2 = SkillTree.SkillId.MoreDamage;
            }
            else
            {
                _option2 = SkillTree.SkillId.AutomaticFire;
            }

            _option3 = SkillTree.SkillId.LongerSprintDuration;
        }}
    private void LevelUpPlayer2(Player player)
    {}
}