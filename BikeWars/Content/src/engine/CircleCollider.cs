using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine;
public class CircleCollider : ColliderBase
{
    private Circle _collisionShape { get; set; }

    public CircleCollider(float r, Vector2 pos, CollisionLayer layer, object owner)
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

    public Vector2 Center()
    {
        return new Vector2(Position.X, Position.Y);
    }

    protected override void Update()
    {
        _collisionShape = new Circle(Position.X, Position.Y, Radius);
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
