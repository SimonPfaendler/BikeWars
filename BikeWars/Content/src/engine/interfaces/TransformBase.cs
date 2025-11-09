using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public abstract class TransformBase : ITransform
{
    private Vector2 _position { get; set; }
    public Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            Update();
        }
    }

    private Point _size { get; set; }
    public Point Size
    {
        get => _size;
        set
        {
            _size = value;
            Update();
        }
    }

    private Rectangle _bounds;
    public Rectangle Bounds { get; protected set; }
    protected abstract void Update();
}