using Microsoft.Xna.Framework;

namespace BikeWars.Content.components;

/// Circle is not in the library of Monogame so we needed to implement it on our own.
/// With enhanced Rectangle we can now even intersect with Circle. 
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

