using Microsoft.Xna.Framework;

namespace BikeWars.Components;

public class Transform
{
    public Vector2 Position;
    public Point Size;
    public Transform(Vector2 position, Point size)
    {
        Position = position;
        Size = size;
    }
    public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Size.X, Size.Y);
}
