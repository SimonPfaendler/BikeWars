#nullable enable
using Microsoft.Xna.Framework;
using BikeWars.Content.components;
using BikeWars.Entities;
namespace BikeWars.Content.entities.projectiles
{
    /// <summary>
    /// Bottle-specific wrapper around the generic lobbed projectile.
    /// </summary>
    public class ThrowBottle : ThrowObject
    {
        public ThrowBottle(Vector2 start, Vector2 target, object owner, WeaponAttributes? attributes = null)
            : base(start, target, owner, textureKey: "Bottle", damage: 45, speed: 100f, arcScale: 1.2f, lingerDuration: 0.25f, attributes: attributes)
        {
        }
        public ThrowBottle(Vector2 start, Vector2 target)
            : base(start, target, null, textureKey: "Bottle", damage: 45, speed: 100f, arcScale: 1.2f, lingerDuration: 0.25f)
        {
        }
    }
}
