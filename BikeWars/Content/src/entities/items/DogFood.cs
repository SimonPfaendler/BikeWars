using System;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.MapObjects;

namespace BikeWars.Content.entities.items;

public class DogFood: ItemBase, IPickable
{
    public override bool InventoryItem => true;
    public override bool IsConsumable => true;
    
    public DogFood(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);

        InitpickupRange();

        TexRight = managers.SpriteManager.GetTexture("DogFood");
        CurrentTex = TexRight;
    }
}
