using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.projectiles
{
    /// <summary>
    /// Book-specific wrapper around the generic lobbed projectile.
    /// </summary>
    public class ThrowBook : ThrowObject
    {
        public ThrowBook(Vector2 start, Vector2 target, object owner)
            : base(start, target, owner, textureKey: "Book", damage: 12, speed: 100f, arcScale: 1.2f, lingerDuration: 0.25f)
        {
        }
    }
}
