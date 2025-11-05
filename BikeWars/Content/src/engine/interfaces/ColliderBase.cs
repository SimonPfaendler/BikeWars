using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public abstract class ColliderBase : ICollider
{
    private Vector2 _position;
    public Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            Update();
        }
    }
    
    protected abstract void Update(); // Use this to update the logic like where the position is or resize the collision box
    public abstract bool Intersects(ICollider other);
}
