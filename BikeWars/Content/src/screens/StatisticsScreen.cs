using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using System.Collections.Generic;
using BikeWars.Content.engine;
using BikeWars.Content.src.utils.SaveLoadExample;
using MonoGame.Extended.Content;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.screens;
public class StatisticsScreen : MenuScreenBase
{
    private ScrollBox _statistics;

    private readonly Texture2D bg_scroll;
    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;

    public List<Statistic> Statistics;

    private List<StatisticsComponent> _components;

    public StatisticsScreen(Texture2D background, SpriteFont font, AudioService audioService)
    : base(background, font)
    {
        _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));

        var state = SaveLoad.LoadGame();
        Statistics = state.Statistics ?? new List<Statistic>();

        _statistics = new ScrollBox(
            RenderPrimitives.Pixel,
            _font,
            new Rectangle(400, 100, 500, 300),
            MakeAchievementList,
            GetStatisticsHeight
        );

        _components = new List<StatisticsComponent>(Statistics.Count);
        foreach (var stat in Statistics)
        {
            _components.Add(new StatisticsComponent(stat));
        }
    }

    public override void LoadContent(ContentManager contentManager)
    {
        base.LoadContent(contentManager);
        InitializeButtons();
    }

    private float GetStatisticsHeight()
    {
        return _components.Count * 110f; // Content of every entry right now.
    }

    protected sealed override void InitializeButtons()
    {
        int screenWidth = Content.GetGraphicsDevice().Viewport.Width;
        int screenHeight = Content.GetGraphicsDevice().Viewport.Height;

        int buttonWidth = 250;
        int buttonHeight = 60;
        int verticalSpacing = 20;
        int horizontalSpacing = screenWidth / 15;

        int leftStartY = screenHeight / 2;

        _buttonTexture = CreateSimpleTexture(buttonWidth, buttonHeight);
        _buttons.Add(new MenuButton(
            id: (int)ButtonAction.Back,
            texture: _buttonTexture,
            bounds: new Rectangle(horizontalSpacing, leftStartY + (buttonHeight + verticalSpacing), buttonWidth, buttonHeight),
            text: "Back",
            font: _font,
            audioService: _audioService
        ));

        UpdateSelection(0);
    }

    protected override void HandleButtonClick(MenuButton button)
    {
        switch ((ButtonAction)button.Id)
        {
            case ButtonAction.Back:
                ScreenManager.RemoveScreen(this);
                break;
        }
    }

    private void MakeAchievementList(SpriteBatch sb, Vector2 startPos)
    {
        foreach (var comp in _components)
        {
            comp.Draw(sb, RenderPrimitives.Pixel, new Color(50, 50, 50, 200), startPos, _font);
            startPos.Y += 110;
        }
}

    public override void Draw(GameTime gameTime, SpriteBatch sb)
    {
        sb.Begin();
        Rectangle destinationRect = new Rectangle(0, 0, Content.GetGraphicsDevice().Viewport.Width, Content.GetGraphicsDevice().Viewport.Height);
        sb.Draw(_backgroundTexture, destinationRect, Color.White);

        foreach (var button in _buttons)
        {
            button.Draw(sb);
        }

        sb.End();
        _statistics.Draw(sb);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _statistics.Update();
    }

    public override bool DrawLower => false;
    public override bool UpdateLower => false;
}