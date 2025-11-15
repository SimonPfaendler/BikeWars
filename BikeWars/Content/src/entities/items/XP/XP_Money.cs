using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.items;

// is supposed to be dropped by Rentner
public class Xp_Money : Xp
{
    private const string Path = "assets/sprites/XP/xp_money_texture";
    protected override string TEXTURE_PATH => Path;

    public Xp_Money(Vector2 start, Point size)
        : base(start, size, xp_value: 3)
    {
    }
}