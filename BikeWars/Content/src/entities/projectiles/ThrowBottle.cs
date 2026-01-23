using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.projectiles
{
    /// <summary>
    /// Bottle-specific wrapper around the generic lobbed projectile.
    /// </summary>
    public class ThrowBottle : ThrowObject
    {
        public ThrowBottle(Vector2 start, Vector2 target, object owner)
            : base(start, target, owner, textureKey: "Bottle", damage: 12, speed: 100f, arcScale: 1.2f, lingerDuration: 0.25f)
        {
        }
    }
}
