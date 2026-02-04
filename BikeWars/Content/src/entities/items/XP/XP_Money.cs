using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.items;

// is supposed to be dropped by Rentner
public class Xp_Money : Xp
{
    private const string TextureKey = "XP_Money"; 
    public Xp_Money(Vector2 start, Point size, int? value = null)
        : base(start, size, xp_value: value ?? 7, textureKey: TextureKey)
    {
    }
    
}