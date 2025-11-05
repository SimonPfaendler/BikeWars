using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public interface ICollider
{
    bool Intersects(ICollider otherCollider);
    Vector2 Position {get;set;}
}    
