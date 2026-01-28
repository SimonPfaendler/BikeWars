using Microsoft.Xna.Framework;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine;
using BikeWars.Entities.Characters;

namespace BikeWars.Content.entities.items;

public class WeaponItem : ItemBase
{
    public Player.WeaponType Type { get; }

    public WeaponItem(Vector2 position, Point size, Player.WeaponType weaponType)
    {
        Type = weaponType;
        Transform = new Transform(position, size);
        
       
        // weapon textures
        string textureKey = weaponType switch
        {
            Player.WeaponType.Gun => "Revolver",
            Player.WeaponType.Flamethrower => "flamethrower_icon",
            Player.WeaponType.IceTrail => "Eisreifen",
            Player.WeaponType.FireTrail => "Feuerreifen",
            Player.WeaponType.DamageCircle => "klingel",
            Player.WeaponType.BananaThrow => "banana_icon",
            Player.WeaponType.BottleThrow => "pfand_icon",
            _ => "Book"
        };
        
        try 
        {
            CurrentTex = managers.SpriteManager.GetTexture(textureKey);
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            // Fallback
            CurrentTex = managers.SpriteManager.GetTexture("Book");
        }
        InitpickupRange();
    }
}
