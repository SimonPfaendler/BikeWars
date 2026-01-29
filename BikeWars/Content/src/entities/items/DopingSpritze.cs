using Microsoft.Xna.Framework;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.entities.items
{
    public class DopingSpritze : ItemBase, IPickable
    {
        public override bool InventoryItem => true;
        public override bool IsConsumable => true;
        public override int HealAmount => 0; // Does not heal

        public DopingSpritze(Vector2 start, Point size)
        {
            Transform = new Transform(start, size);

            InitpickupRange();

            TexRight = managers.SpriteManager.GetTexture("DopingSpritze");
            CurrentTex = TexRight;
        }
    }
}
