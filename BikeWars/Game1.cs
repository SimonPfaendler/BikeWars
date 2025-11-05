using System;
using BikeWars.Content;
using BikeWars.Content.engine;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        c1 = new BoxCollider(new Vector2(playerPosX, playerPosY), 10, 10);
        c2 = new BoxCollider(new Vector2(30, 30), 10, 10);
        cm = new CollisionManager();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        //texture = Content.Load<Texture2D>("myimage");
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            Vector2 crtPos = c1.Position;
            c1.Position = new Vector2(crtPos.X + 1, crtPos.Y);
        }
        Console.WriteLine(cm.isColliding(c1, c2));
        Console.WriteLine(c1.Position);
        
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        // spriteBatch.Draw(texture, new Vector2(100, 100), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}