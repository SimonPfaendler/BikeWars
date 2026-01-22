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
    
    public bool IsExpired { get; private set; }

    public static bool BeerIsActive { get; private set; }
    
    private static readonly CooldownWithDuration _beerCooldown =
        new CooldownWithDuration(durationSeconds: 20f, cooldownSeconds: 0.5f);

    public Beer(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);

        InitpickupRange();

        TexRight = managers.SpriteManager.GetTexture("Beer");
        CurrentTex = TexRight;
    }

    public override void Update(GameTime gameTime)
    {
        _beerCooldown.Update(gameTime);
        if (BeerIsActive && !_beerCooldown.IsActive)
        {
            BeerIsActive = false;
            IsExpired = true;
        }
    }
    public void LandedBeer(Vector2 beerposition)
    {
        BeerPosition = beerposition;
        CurrentTex = managers.SpriteManager.GetTexture("Beer_destroyed");
        BeerIsActive = true;
        // wenn cooldown vorbei sowas wie OnRemove(beer)
    }
    
    public static bool TryActivateBeer()
    {
        if (_beerCooldown.Ready)
        {
            _beerCooldown.Activate();
            return true;
        }
        return false;
    }
}