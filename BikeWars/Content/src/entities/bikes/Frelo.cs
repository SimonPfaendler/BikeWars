using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;
using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.interfaces;
public class Frelo: Bike
{
    public Frelo(Vector2 start, Point size, BikeAttributes attributes) : base(start, size, attributes)
    {
        TexRight = managers.SpriteManager.GetTexture("Frelo");
        CurrentTex = TexRight;
    }

    public Frelo(Vector2 start, Point size) : base(start, size)
    {
        TexRight = managers.SpriteManager.GetTexture("Frelo");
        CurrentTex = TexRight;

        Attributes = new BikeAttributes(
            this, 300, 300, 3, 180, 1.2f, 3, 1.2f, 1.2f
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