using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.projectiles
{
    /// <summary>
    /// Book-specific wrapper around the generic lobbed projectile.
    /// </summary>
    public class ThrowBeer : ThrowObject
    {
        public ThrowBeer(Vector2 start, Vector2 target, object owner)
            : base(start, target, owner, textureKey: "Beer", damage: 12, speed: 500f, arcScale: 1.2f, lingerDuration: 0.25f)
        {
        }
    }
}