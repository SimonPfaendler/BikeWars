using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;
using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.interfaces;
public class RacingBike: Bike
{
    public RacingBike(Vector2 start, Point size, BikeAttributes attributes) : base(start, size, attributes)
    {
        TexRight = managers.SpriteManager.GetTexture("RacingBike");
        CurrentTex = TexRight;
        Attributes = attributes;
    }

    public RacingBike(Vector2 start, Point size) : base(start, size)
    {
        TexRight = managers.SpriteManager.GetTexture("RacingBike");
        CurrentTex = TexRight;

        Attributes = new BikeAttributes(
            this, 200, 200, 10, 0, 300, 1.4f, 10, 1.4f, 1.4f, 1.4f
        );
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.White);
    }

    public override void Update(GameTime gameTime)
    {
    }
    public override bool Intersects(ICollider collider)
    {
        return Collider.Intersects(collider);
    }
}