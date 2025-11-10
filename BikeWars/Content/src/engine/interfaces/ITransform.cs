using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public interface ITransform
{
    Vector2 Position {get;set;}
    Point Size {get;set;}
}