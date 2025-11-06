using System;
using BikeWars.Content;
using BikeWars.Content.engine;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BikeWars.Components;
using BikeWars.Content.entities.items;
using BikeWars.Entities.Characters;
using BikeWars.Utilities;

namespace BikeWars;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _texture;

    private BoxCollider c1;
    private BoxCollider c2;
    private CollisionManager cm;

    private int playerPosX = 1;
    private int playerPosY = 30;
    Player player;
    private TestItem tItem;

    private SpriteFont _debugFont;
    private Debugger _debugger;


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
        c1 = new BoxCollider(new Vector2(playerPosX, playerPosY), 10, 10);
        c2 = new BoxCollider(new Vector2(30, 30), 10, 10);
        cm = new CollisionManager();
        base.Initialize();
        tItem = new TestItem(new Vector2(_graphics.PreferredBackBufferWidth / 2 + 50, _graphics.PreferredBackBufferHeight / 2 + 50), new Point(32, 32));
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        //texture = Content.Load<Texture2D>("myimage");

        _debugFont = Content.Load<SpriteFont>("assets/fonts/Arial");

        // Get screen dimensions
        int width = _graphics.PreferredBackBufferWidth;
        int height = _graphics.PreferredBackBufferHeight;

        // Spawn player in center of screen
        player = new Player(new Vector2(width / 2, height / 2), new Point(32, 32));

        _debugger = new Debugger(_debugFont, player);

    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        player.Update(gameTime);
        if (player.Intersects(tItem.Collider))
        {
            Console.WriteLine("player.Transform.Position");
            Console.WriteLine(player.Transform.Position);
            Console.WriteLine("player.lastTransform.Position");
            Console.WriteLine(player.lastTransform.Position);
            player.Transform = new Transform(new Vector2(player.lastTransform.Position.X, player.lastTransform.Position.Y), player.lastTransform.Size);
            player.UpdateCollider();
        }
        _debugger.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        // spriteBatch.Draw(texture, new Vector2(100, 100), Color.White);
        player.Draw(_spriteBatch);
        tItem.Draw(_spriteBatch);
        _debugger.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}