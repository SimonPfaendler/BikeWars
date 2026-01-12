using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.entities.items;

public class DogFood: ItemBase, IPickable
{
    public override bool InventoryItem => true;
    public override bool IsConsumable => false;

    public DogFood(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);

        int pickupRange = 40;

        Collider = new BoxCollider(
            new Vector2(Transform.Position.X - pickupRange / 2f,
                Transform.Position.Y - pickupRange / 2f),
            Transform.Size.X + pickupRange,
            Transform.Size.Y + pickupRange,
            CollisionLayer.ITEM,
            this
        );

        TexRight = managers.SpriteManager.GetTexture("DogFood");
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
        return Collider.Intersects(collider);
    }
}
