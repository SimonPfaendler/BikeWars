using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.entities.MapObjects;

public class DogBowl: ItemBase
{
    private bool _full;
    public bool Full => _full;
    private BoxCollider _collisionCollider {get;set;}
    public BoxCollider CollisionCollider {get => _collisionCollider; set => _collisionCollider = value; }
    private int PADDING_INTERACTION_AREA = 40;
    
    private static readonly CooldownWithDuration _bowlCooldown =
        new CooldownWithDuration(durationSeconds: 20f, cooldownSeconds: 5f);

    public static bool BowlIsActive => _bowlCooldown.IsActive;
    public static Vector2 BowlPosition { get; private set; }

    public DogBowl(Vector2 start, Point size, bool full = false)
    {
        _full = full;
        Transform = new Transform(start, size);
        Collider = new BoxCollider(new Vector2(Transform.Position.X - PADDING_INTERACTION_AREA / 2, Transform.Position.Y - PADDING_INTERACTION_AREA / 2), Transform.Size.X + PADDING_INTERACTION_AREA, Transform.Size.Y + PADDING_INTERACTION_AREA, CollisionLayer.INTERACT, this);
        CollisionCollider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.WALL, this);
        if (_full == false)
        {TexRight = managers.SpriteManager.GetTexture("Dog_Bowl");}
        else
        {TexRight = managers.SpriteManager.GetTexture("Dog_Bowl_full");}
        CurrentTex = TexRight;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.White);
    }

    public override void Update(GameTime gameTime)
    {
    }
    public override bool Intersects(ICollider collider)
    {
        return Collider.Intersects(collider);
    }

    public bool TryActivateDogBowl()
    {
        if (_bowlCooldown.Ready)
        {
            return true;
        }
        return false;
    }

    public void ActivateDogBowl(Vector2 bowlposition)
    {
        BowlPosition = bowlposition;
        _bowlCooldown.Activate();
    }
    
    public void FillUpDogBowl()
    {
        if (_full) return;
        _full = true;
        CurrentTex = managers.SpriteManager.GetTexture("Dog_Bowl_full");
    }

    public static void UpdateBowl(GameTime gameTime)
    {
        _bowlCooldown.Update(gameTime);
    }
    

}