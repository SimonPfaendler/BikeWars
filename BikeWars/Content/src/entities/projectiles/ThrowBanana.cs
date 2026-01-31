using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.projectiles
{
    /// <summary>
    /// Banana-specific wrapper around the generic lobbed projectile.
    /// </summary>
    public class ThrowBanana : ThrowObject
    {
        public ThrowBanana(Vector2 start, Vector2 target, object owner)
            : base(start, target, owner, textureKey: "Banana", damage: 45, speed: 100f, arcScale: 1.2f, lingerDuration: 0.25f)
        {
        }
    }
}
