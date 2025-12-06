using Microsoft.Xna.Framework;
using MonoGame.Extended.Collisions.Layers;

namespace BikeWars.Content.entities.items;

// is supposed to be dropped by betrunkenen
public class Xp_Beer : Xp
{
    private const string TextureKey = "XP_Beer"; 
    public Xp_Beer(Vector2 start, Point size)
        : base(start, size, xp_value: 5, textureKey: TextureKey)
    {
    }
    
}