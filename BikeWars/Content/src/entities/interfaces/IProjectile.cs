using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public interface IProjectile
{
    public void Update(GameTime gameTime);
    public bool Intersects(ICollider other);
}
