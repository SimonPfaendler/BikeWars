using Microsoft.Xna.Framework;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.entities.items;
public class EnergyBar : ItemBase, IPickable
{
    public override bool InventoryItem => false;
    public override bool IsConsumable => true;
    public override int HealAmount => 2;
    public float DecreaseSprintCoolDown = 1f;
    public EnergyBar(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);

        InitpickupRange(20);

        TexRight = managers.SpriteManager.GetTexture("EnergyBar");
        CurrentTex = TexRight;
    }
}
