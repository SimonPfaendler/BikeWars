using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BikeWars.Components;
using BikeWars.Content.entities.items;
using BikeWars.Entities.Characters;
using BikeWars.Utilities;
using Microsoft.Xna.Framework.Audio;

namespace BikeWars;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _texture;

    private List<TestItem> _testItems;
    
    private int playerPosX = 1;
    private int playerPosY = 30;
    Player player;

    private SpriteFont _debugFont;
    private Debugger _debugger;
    private SoundEffect walkingSound;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Just for Testing
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
    }

    protected override void Initialize()
    {
        _testItems = new List<TestItem>();
        _testItems.Add(new TestItem(new Vector2(_graphics.PreferredBackBufferWidth / 2 + 50, _graphics.PreferredBackBufferHeight / 2 + 50), new Point(32, 32)));
        _testItems.Add(new TestItem(new Vector2(_graphics.PreferredBackBufferWidth / 2 - 50, _graphics.PreferredBackBufferHeight / 2 + 50), new Point(32, 32)));
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _debugFont = Content.Load<SpriteFont>("assets/fonts/Arial");

        // Get screen dimensions
        int width = _graphics.PreferredBackBufferWidth;
        int height = _graphics.PreferredBackBufferHeight;

        // Spawn player in center of screen
        player = new Player(new Vector2(width / 2, height / 2), new Point(32, 32));

        _debugger = new Debugger(_debugFont, player);
        
        // Load Soundeffects
        walkingSound = Content.Load<SoundEffect>("assets/sounds/Walking");
        player.LoadContent(Content, walkingSound);
        
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        player.Update(gameTime);
        
        if (player.Intersects(_testItems[0].Collider))
        {
            player.Transform = new Transform(new Vector2(player.lastTransform.Position.X, player.lastTransform.Position.Y), player.lastTransform.Size);
            player.UpdateCollider();
        }

        if (_testItems.Count > 1)
        {
            if (player.Intersects(_testItems[1].Collider))
            {
                _testItems.RemoveAt(1);
            }    
        }
        
        _debugger.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        player.Draw(_spriteBatch);
        foreach (var item in _testItems)
        {
            item.Draw(_spriteBatch);
        }
        _debugger.Draw(_spriteBatch);
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}