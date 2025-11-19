using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework.Content;
using System;

namespace BikeWars.Content.entities.items;
public class Bullet: ProjectileBase
{
    private const string TEXTURE_PATH = "assets/sprites/projectiles/bullet";
    private BoxCollider _collider { get; set; }
    public float Speed = 400f;
    public override BoxCollider Collider
    {
        get { return _collider; }
    }

    private BulletMovement _movement {get;set;}
    public new BulletMovement Movement
    {
        get { return _movement; }
        set { _movement = value;}
    }
    public Bullet(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);
        _collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y);
        Movement = new BulletMovement(true, true);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.White);
    }

    public override void Update(GameTime gameTime)
    {
        Movement.HandleMovement(gameTime);
        if (Movement.IsMoving)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Movement.Direction is already normalized when set
            Transform.Position += Movement.Direction * Speed * delta;
            _collider.Position = Transform.Position;
        }
    }
    public override void LoadContent(ContentManager content)
    {
        TexRight = content.Load<Texture2D>(TEXTURE_PATH);
        CurrentTex = TexRight;
    }
    public override bool Intersects(ICollider collider)
    {
        return _collider.Intersects(collider);
    }
}
