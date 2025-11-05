using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine;
public class BoxCollider : ColliderBase
{
    private int width { get; set; }
    private int height { get; set; }

    private Vector2 _position;
    public Vector2 Position
    {
        get { return _position; }
        set
        {
            _position = value;
            Update();
        }
    }
    private EnhancedRectangle _collisionShape { get; set; }
    public EnhancedRectangle CollisionShape
    {
        get { return _collisionShape; }
    }
    public BoxCollider(Vector2 pos, int w, int h)
    {
        width = w;
        height = h;
        Position = pos;
    }

    public void SetSize(int w, int h)
    {
        width = w;
        height = h;
        Update();
    }
    
    protected override void Update()
    {
        _collisionShape = new EnhancedRectangle(new Rectangle((int)_position.X, (int)_position.Y, width, height));
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
