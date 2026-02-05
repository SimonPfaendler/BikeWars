using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.interfaces;
public class RacingBike: Bike
{
    public RacingBike(Vector2 start, Point size, BikeAttributes attributes) : base(start, size, attributes)
    {
        TexRight = managers.SpriteManager.GetTexture("RacingBike");
        CurrentTex = TexRight;
        Attributes = attributes;
        InitpickupRange();
    }

    public RacingBike(Vector2 start, Point size) : base(start, size)
    {
        TexRight = managers.SpriteManager.GetTexture("RacingBike");
        CurrentTex = TexRight;

        Attributes = new BikeAttributes(
            this, 250, 250, 2, 0, 300, 1.4f, 10, 1.4f, 1.4f, 1.4f
        );
    }

}