using BikeWars.Content.engine;
using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.interfaces;
public abstract class Bike: ItemBase, IBike
{
    private BikeAttributes _attributes {get;set;}
    public BikeAttributes Attributes {
        get => _attributes;
        set {
            _attributes = value;
            if (_attributes != null) _attributes.owner = this;
        }
    }
    public bool IsDestroyed => Attributes.Health <= 0;
    public bool IsGodMode { get; set; }

    public Bike(Vector2 start, Point size, BikeAttributes attributes)
    {
        IsBike = true;
        Transform = new Transform(start, size);
        Collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.ITEM, this);
        Attributes = attributes;
    }

    public Bike(Vector2 start, Point size)
    {
        IsBike = true;
        Transform = new Transform(start, size);
        Collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.ITEM, this);
    }

    public virtual void TakeDamage(int amount)
    {
        if (IsGodMode) return;
        if (Attributes == null) return;
        if (IsDestroyed) return;
        // OnTookDamage?.Invoke(this, amount);
        Attributes.Health -= amount;
    }

    public void Repair()
    {
        Attributes.Health = Attributes.MaxHealth;
    }
}