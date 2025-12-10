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
        public override int HealAmount => 30;            // +30 HP

        private BoxCollider _collider;
        public override ICollider Collider => _collider;

        public EnergyGel(Vector2 start, Point size)
        {
            Transform = new Transform(start, size);

            _collider = new BoxCollider(
                new Vector2(Transform.Position.X, Transform.Position.Y),
                Transform.Size.X,
                Transform.Size.Y,
                CollisionLayer.ITEM,
                this
            );
            
            TexRight = managers.SpriteManager.GetTexture("EnergyGel");
            CurrentTex = TexRight;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            // EnergyGel hat keine Animation
        }

        public override bool Intersects(ICollider collider)
        {
            return _collider.Intersects(collider);
        }
    }
}