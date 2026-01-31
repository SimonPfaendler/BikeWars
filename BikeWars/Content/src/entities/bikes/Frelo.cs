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
        Attributes = attributes;
        InitpickupRange();
    }

    public Frelo(Vector2 start, Point size) : base(start, size)
    {
        TexRight = managers.SpriteManager.GetTexture("Frelo");
        CurrentTex = TexRight;

        Attributes = new BikeAttributes(
            this, 150, 150, 3, 0, 180, 1.2f, 3, 1.2f, 1.2f, 1.2f
        );
    }
}