using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public interface ICharacter
{
    public void Update(GameTime gameTime); // Use this to update the logic like where the position is or resize the collision box
    public bool Intersects(ICollider other);
}    
