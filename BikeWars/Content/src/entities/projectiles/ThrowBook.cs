using BikeWars.Content.engine.interfaces;
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
        : base(start, target, owner, textureKey: "Book", damage: 5, speed: 100f, arcScale: 1.2f, lingerDuration: 0.25f)
    {
    }
    public ThrowBook(Vector2 start, Vector2 target)
        : base(start, target, null, textureKey: "Book", damage: 45, speed: 100f, arcScale: 1.2f, lingerDuration: 0.25f)
    {
    }
    public override void LevelUp()
    {
    }
}
