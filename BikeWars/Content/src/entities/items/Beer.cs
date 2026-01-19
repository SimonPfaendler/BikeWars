using Microsoft.Xna.Framework;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.entities.items;

public class Beer: ItemBase, IPickable
{
    public override bool InventoryItem => true;
    public override bool IsConsumable => true;
    
    public static Vector2 BeerPosition { get; private set; }
    public static bool BeerIsActive { get; private set; }
    
    private static readonly CooldownWithDuration _beerCooldown =
        new CooldownWithDuration(durationSeconds: 10f, cooldownSeconds: 0.5f);

    public Beer(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);

        InitpickupRange();

        TexRight = managers.SpriteManager.GetTexture("Beer");
        CurrentTex = TexRight;
    }
    
    public void ActivateBeer(Vector2 beerposition)
    {
        BeerPosition = beerposition;
        _beerCooldown.Activate();
        CurrentTex = managers.SpriteManager.GetTexture("Beer_destroyed");
        // wenn cooldown vorbei sowas wie OnRemove(beer)
    }
    
    public bool TryActivateBeer()
    {
        if (_beerCooldown.Ready)
        {
            return true;
        }
        return false;
    }
}