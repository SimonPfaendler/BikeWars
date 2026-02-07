#nullable enable
using Microsoft.Xna.Framework;
using BikeWars.Content.components;
using BikeWars.Entities;

namespace BikeWars.Content.entities.projectiles
{
    /// <summary>
    /// Banana-specific wrapper around the generic lobbed projectile.
    /// </summary>
    public class ThrowBanana : ThrowObject
    {
        public ThrowBanana(Vector2 start, Vector2 target, object owner, WeaponAttributes? attributes = null)
            : base(start, target, owner, textureKey: "Banana", damage: 45, speed: 100f, arcScale: 1.2f, lingerDuration: 0.25f, attributes: attributes)
        {
        }
        public ThrowBanana(Vector2 start, Vector2 target)
            : base(start, target, null, textureKey: "Banana", damage: 45, speed: 100f, arcScale: 1.2f, lingerDuration: 0.25f)
        {
        }
    }
}
