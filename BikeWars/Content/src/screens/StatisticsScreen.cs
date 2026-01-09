using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using System.Collections.Generic;
using BikeWars.Content.engine;
using BikeWars.Content.src.utils.SaveLoadExample;

namespace BikeWars.Content.screens;
public class StatisticsScreen : MenuScreenBase, IScreen
{
    private ScrollBox _statistics;

    private readonly AudioService _audioService;
    public string DesiredMusic => AudioAssets.MenuMusic;
    public float MusicVolume => 1f;

    public List<Statistic> Statistics;
    public StatisticsScreen(Texture2D background, SpriteFont font, AudioService audioService)
        :base(background, font)
    {
        _audioService = audioService ?? throw new System.ArgumentNullException(nameof(audioService));
        var state = SaveLoad.LoadGame();
        Statistics = state.Statistics ?? new List<Statistic>();

        Texture2D bg = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        bg.SetData(new[] { Color.Black });

        _statistics = new ScrollBox(
            bg,
            _font,
            new Rectangle(400, 100, 500, 300),
            Game1.Instance.SpriteBatch,
            MakeAchievementList,
            GetStatisticsHeight
        );
        InitializeButtons();
    }

    private float GetStatisticsHeight()
    {
        return Statistics.Count * 110f; // Content of every entry right now.
    }

    protected sealed override void InitializeButtons()
    {
        Game1 game = Game1.Instance;
        int screenWidth = game.GraphicsDevice.Viewport.Width;
        int screenHeight = game.GraphicsDevice.Viewport.Height;

        int buttonWidth = 250;
        int buttonHeight = 60;
        int verticalSpacing = 20;
        int horizontalSpacing = screenWidth / 15;

        int leftStartY = screenHeight / 2;

        _buttonTexture = CreateSimpleTexture(game.GraphicsDevice, buttonWidth, buttonHeight);
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
        int row = 0;
        if (Statistics == null)
        {
            return;
        }

        Texture2D overlay = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        overlay.SetData(new[] { Color.White });
        foreach (Statistic statistic in Statistics) {
            new StatisticsComponent(statistic)
                .Draw(sb, overlay, startPos + new Vector2(0, row), _font);
            row += 110;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Game1 game = Game1.Instance;
        SpriteBatch spriteBatch = game.SpriteBatch;

        spriteBatch.Begin();
        Rectangle destinationRect = new Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
        spriteBatch.Draw(_backgroundTexture, destinationRect, Color.White);

        foreach (var button in _buttons)
        {
            button.Draw(spriteBatch);
        }

        spriteBatch.End();
        _statistics.Draw();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _statistics.Update();
    }
    public override bool DrawLower => false;
    public override bool UpdateLower => false;
}