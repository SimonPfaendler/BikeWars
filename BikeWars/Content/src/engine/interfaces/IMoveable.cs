using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public interface IMoveable
{
    Vector2 Direction { get; set; }
    bool IsMoving { get; set; }
    bool CanMove { get; set; }
    void Update(GameTime gameTime);
    void HandleMovement(GameTime gameTime); // User or other input
}    
