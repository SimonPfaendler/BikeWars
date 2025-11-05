using Microsoft.Xna.Framework;

namespace BikeWars.Content.components;
public readonly struct EnhancedRectangle
{
    public Rectangle Rectangle { get; }

    public EnhancedRectangle(Rectangle rectangle)
    {
        Rectangle = rectangle;
    }

    public bool Intersects(EnhancedRectangle other)
    {
        return Rectangle.Intersects(other.Rectangle);
    }
    
    public bool Intersects(Circle circ)
    {
        return Maths.CircleIntersectsRectangle(circ, Rectangle);
    }
}    

