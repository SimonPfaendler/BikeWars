using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;

public class Transform: TransformBase
{
    public Transform(Vector2 position, Point size)
    {
        Position = position;
        Size = size;
    }
    protected override void Update()
    {
        Bounds = new Rectangle((int)Position.X, (int)Position.Y, Size.X, Size.Y);
    }
}