using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public abstract class ColliderBase : ICollider
{
    private Vector2 _position { get; set; }
    public Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            Update();
        }
    }

    // Necessary to calculate with Spatial Hashing and even configure which layer interacts with which
    private float _radius {get; set;}
    public float Radius {get => _radius; set => _radius = value;}

    private CollisionLayer _layer {get; set;}
    public CollisionLayer Layer {get => _layer; set => _layer = value;}

    private object _owner {get; set;}
    public object Owner {get => _owner; set => _owner = value;}

    protected abstract void Update(); // Use this to update the logic like where the position is or resize the collision box
    public abstract bool Intersects(ICollider other);
}
