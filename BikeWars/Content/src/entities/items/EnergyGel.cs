using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.entities.items
{
    public class EnergyGel : ItemBase, IPickable
    {
        public override bool InventoryItem => true;
        public override bool IsConsumable => true;
        public override int HealAmount => 30;

        public EnergyGel(Vector2 start, Point size)
        {
            Transform = new Transform(start, size);
            InitpickupRange();
            TexRight = managers.SpriteManager.GetTexture("EnergyGel");
            CurrentTex = TexRight;
        }
    }
}