using BikeWars.Content.components;
using Microsoft.Xna.Framework;

namespace BikeWars.Utilities
{
    public static class Maths
    {
        public static bool CircleIntersectsRectangle(Circle circ, Rectangle rect)
        {
            float closestX = MathHelper.Clamp(circ.Location.X, rect.Left, rect.Right);
            float closestY = MathHelper.Clamp(circ.Location.Y, rect.Top, rect.Bottom);
            float distance = Vector2.Distance(new Vector2(circ.Location.X, circ.Location.Y), new Vector2(closestX, closestY));
            return distance < circ.Radius;
        }
        public static Vector2 Middle(Vector2 first, Vector2 second)
        {
            return (first + second) / 2;
        }
    }
}
