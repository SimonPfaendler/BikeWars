using System;
using Microsoft.Xna.Framework;
using BikeWars.Utilities;

namespace BikeWars.Content.components;
///  Circle is not in the standard library we had to implement that on our own.
public readonly struct Circle : IEquatable<Circle>
{
    private static readonly Circle s_empty = new Circle();
    public readonly float X;
    public readonly float Y;
    public readonly float Radius;
    public readonly Point Location => new Point((int)X, (int)Y);
    public static Circle Empty => s_empty;
    public readonly bool IsEmpty => X == 0 && Y == 0 && Radius == 0;

    // Gets the y-coordinate of the highest point on this circle.
    public readonly float Top => Y - Radius;

    // Gets the y-coordinate of the lowest point on this circle.
    public readonly float Bottom => Y + Radius;

    // Gets the x-coordinate of the leftmost point on this circle.
    public readonly float Left => X - Radius;

    // Gets the x-coordinate of the rightmost point on this circle.
    public readonly float Right => X + Radius;

    // Creates a new circle with the specified position and radius.
    // <param name="x">The x-coordinate of the center of the circle.</param>
    // <param name="y">The y-coordinate of the center of the circle.</param>
    // <param name="radius">The length from the center of the circle to an edge.</param>
    public Circle(float x, float y, float radius)
    {
        X = x;
        Y = y;
        Radius = radius;
    }

    // Creates a new circle with the specified position and radius.
    // <param name="location">The center of the circle.</param>
    // <param name="radius">The length from the center of the circle to an edge.</param>
    public Circle(Point location, int radius)
    {
        X = location.X;
        Y = location.Y;
        Radius = radius;
    }
    public bool Intersects(Circle other)
    {
        float radiiSquared = (Radius + other.Radius) * (Radius + other.Radius);
        float distanceSquared = Vector2.DistanceSquared(Location.ToVector2(), other.Location.ToVector2());
        return distanceSquared < radiiSquared;
    }

    public bool Intersects(Rectangle rect)
    {
        return Maths.CircleIntersectsRectangle(this, rect);
    }
    // Returns a value that indicates whether this circle and the specified circle are equal.
    // <param name="other">The circle to compare with this circle.</param>
    // <returns>true if this circle and the specified circle are equal; otherwise, false.</returns>
    public readonly bool Equals(Circle other) => X == other.X &&
                                                 Y == other.Y &&
                                                 Radius == other.Radius;
}

