using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework.Content;
using BikeWars.Entities;

namespace BikeWars.Content.entities.items;
public class Bullet: ProjectileBase
{
    private BoxCollider _collider { get; set; }
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
    public Bullet(Vector2 start, Point size, object owner, WeaponAttributes wa)
    {
        weaponAttributes = wa;
        Transform = new Transform(start, size);
        _collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.PROJECTILE, this);
        Movement = new BulletMovement(true, true);
        Owner = owner;
        HasHit = false;

        TexRight = managers.SpriteManager.GetTexture("Bullet");
        CurrentTex = TexRight;
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
            Transform.Position += Movement.Direction * weaponAttributes.Speed * delta;
            _collider.Position = Transform.Position;
        }
    }

    public override bool Intersects(ICollider collider)
    {
        return _collider.Intersects(collider);
    }

    public override void LoadContent(ContentManager content)
    {
        // Platzhalter damit abstrakte Anforderung der Basisklasse korrekt ist, sollte irgendwann entfernt werden
    }

    public override void LevelUp()
    {
        return;
    }
}
