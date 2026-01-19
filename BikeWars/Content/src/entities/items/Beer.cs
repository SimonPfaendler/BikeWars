using Microsoft.Xna.Framework;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.entities.items;

public class Beer: ItemBase, IPickable
{
    public override bool InventoryItem => true;
    public override bool IsConsumable => false;

    public Beer(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);

        InitpickupRange();

        TexRight = managers.SpriteManager.GetTexture("Beer");
        CurrentTex = TexRight;
    } 
}