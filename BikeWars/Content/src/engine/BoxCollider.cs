using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine;
public class BoxCollider : ColliderBase
{
    private EnhancedRectangle _collisionShape { get; set; }
    public EnhancedRectangle CollisionShape
    {
        get { return _collisionShape; }
    }
    public BoxCollider(Vector2 pos, int w, int h, CollisionLayer layer, object owner)
    {
        Width = w;
        Height = h;
        Position = pos;
        Layer = layer;
        Owner = owner;
    }

    public void SetSize(int w, int h)
    {
        Width = w;
        Height = h;
        Update();
    }

    protected override void Update()
    {
        _collisionShape = new EnhancedRectangle(new Rectangle((int)Position.X, (int)Position.Y, Width, Height));
    }
    public override bool Intersects(ICollider other)
    {
        if (other is BoxCollider bc)
        {
            return _collisionShape.Intersects(bc.CollisionShape);
        }

        if (other is CircleCollider cc)
        {
            return _collisionShape.Intersects(cc.CollisionShape);
        }
        return false;
    }
}
