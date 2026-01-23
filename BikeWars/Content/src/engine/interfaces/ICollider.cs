using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public interface ICollider
{
    bool Intersects(ICollider otherCollider);
    Vector2 Position {get;set;}
    int Height {get; set;}
    int Width {get; set;}
    float Radius {get; set;}
    CollisionLayer Layer {get; set;}
    object Owner {get; set;} // This is necessary and just a reference to like Player or so
    int LastQueryId { get; set; } // We use this only for the spatial hash. Especially not to insert it double in a cell
}
