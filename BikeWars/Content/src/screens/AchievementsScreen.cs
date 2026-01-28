using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using System.Collections.Generic;
using BikeWars.Content.engine;
using BikeWars.Content.src.utils.SaveLoadExample;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens;
public class AchievementsScreen : MenuScreenBase
{
    private ScrollBox _achievements;
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;

    private static int HEIGHT_OF_COMPONENT = 10 + 11*20;

    public List<Achievement> Achievements;

    private List<AchievementsComponent> _components;

    public AchievementsScreen(Texture2D background, SpriteFont font, AudioService audioService, Viewport vp)
    : base(background, font, vp)
    {
        _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));

        var state = SaveLoad.LoadGame();
        Achievements = state.Achievements ?? new List<Achievement>();

        _achievements = new ScrollBox(
            RenderPrimitives.Pixel,
            _font,
            new Rectangle(400, 100, 500, 300),
            MakeAchievementList,
            GetStatisticsHeight
        );

        _components = new List<AchievementsComponent>(Achievements.Count);
        foreach (Achievement achieve in Achievements)
        {
            _components.Add(new AchievementsComponent(achieve));
        }
    }

    public override void LoadContent(ContentManager content, GraphicsDevice gd)
    {
        base.LoadContent(content, gd);
        InitializeButtons();
    }

    private float GetStatisticsHeight()
    {
        return _components.Count * HEIGHT_OF_COMPONENT; // Content of every entry right now.
    }

    protected sealed override void InitializeButtons()
    {
        int screenWidth = ViewPort.Width;
        int screenHeight = ViewPort.Height;

        int buttonWidth = 250;
        int buttonHeight = 60;
        int verticalSpacing = 20;
        int horizontalSpacing = screenWidth / 15;

        int leftStartY = screenHeight / 2;
        AddButton(new MenuButton(
            id: (int)ButtonAction.Back,
            texture: RenderPrimitives.Pixel,
            bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
            text: "Back",
            font: _font,
            audioService: _audioService
        ));

        UpdateSelection(0);
    }

    private void MakeAchievementList(SpriteBatch sb, Vector2 startPos)
    {
        foreach (var comp in _components)
        {
            comp.Draw(sb, RenderPrimitives.Pixel, new Color(50, 50, 50, 200), startPos, _font);
            startPos.Y += HEIGHT_OF_COMPONENT;
        }
    }
    public override void Draw(GameTime gameTime, SpriteBatch sb)
    {
        sb.Begin();
        Rectangle destinationRect = new Rectangle(0, 0, ViewPort.Width, ViewPort.Height);
        sb.Draw(_backgroundTexture, destinationRect, Color.White);

        foreach (var button in _buttons)
        {
            button.Draw(sb);
        }
        sb.End();
        _achievements.Draw(sb);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _achievements.Update();
    }

    public override bool DrawLower => false;
    public override bool UpdateLower => false;
}