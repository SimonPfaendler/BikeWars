using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public interface ICollider
{
    bool Intersects(ICollider otherCollider);
    Vector2 Position {get;set;}
    float Radius {get; set;}
    CollisionLayer Layer {get; set;}
    object Owner {get; set;} // This is necessary and just a reference to like Player or so
}
