using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public interface IItem
{
    public void Update(GameTime gameTime);
    public bool Intersects(ICollider other);
}    
