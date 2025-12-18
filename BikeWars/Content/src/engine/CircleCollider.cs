using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine;
public class CircleCollider : ColliderBase
{
    private Circle _collisionShape { get; set; }

    public CircleCollider(int r, Vector2 pos, CollisionLayer layer, object owner)
    {
        Radius = r;
        Position = pos;
        Layer = layer;
        Owner = owner;
    }

    public Circle CollisionShape
    {
        get => _collisionShape;
    }

    protected override void Update()
    {
        _collisionShape = new Circle((int)Position.X, (int)Position.Y, (int)Radius);
    }

    public override bool Intersects(ICollider otherCollider)
    {
        if (otherCollider is BoxCollider bc)
        {
            return _collisionShape.Intersects(bc.CollisionShape.Rectangle);
        }

        if (otherCollider is CircleCollider cc)
        {
            return _collisionShape.Intersects(cc.CollisionShape);
        }
        return false;
    }
}
