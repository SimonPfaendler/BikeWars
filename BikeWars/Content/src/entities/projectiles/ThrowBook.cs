using BikeWars.Content.engine.interfaces;
using BikeWars.Entities;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.projectiles;
/// <summary>
/// Book-specific wrapper around the generic lobbed projectile.
/// </summary>
///

public class ThrowBook : ThrowObject, IWeapon
{
    private static int MAX_LEVEL = 5;
    public ThrowBook(Vector2 start, Vector2 target, object owner)
        : base(start, target, owner, textureKey: "Book", damage: 12, speed: 100f, arcScale: 1.2f, lingerDuration: 0.25f)
    {
        // _wp = new WeaponAttributes(owner, _level, damage, speed, arcScale, lingerDuration);
    }
    public override void LevelUp()
    {
    }

    // public WeaponAttributes WeaponAttributes()
    // {
    // }
}
