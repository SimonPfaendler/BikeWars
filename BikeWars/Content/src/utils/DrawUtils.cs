using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BikeWars.Utilities
{
    public static class DrawUtils
    {
        public static void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            spriteBatch.Draw(pixel,
                new Rectangle((int)start.X, (int)start.Y, (int)length, 2), // 2 is thickness
                null,
                color,
                angle,
                Vector2.Zero,
                SpriteEffects.None,
                0);
        }

        public static void DrawArc(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, float radius, float angle, float sweep, Color color, int segments = 16)
        {
            float startAngle = angle - sweep / 2f;
            float step = sweep / segments;

            for (int i = 0; i < segments; i++)
            {
                float theta1 = startAngle + i * step;
                float theta2 = startAngle + (i + 1) * step;

                Vector2 p1 = center + new Vector2((float)Math.Cos(theta1), (float)Math.Sin(theta1)) * radius;
                Vector2 p2 = center + new Vector2((float)Math.Cos(theta2), (float)Math.Sin(theta2)) * radius;

                DrawLine(spriteBatch, pixel, p1, p2, color);
            }
        }

        public static void DrawCircleOutline(
            SpriteBatch spriteBatch,
            Texture2D pixel,
            Vector2 center,
            float radius,
            Color color,
            int segments = 24)
        {

            Vector2 prevPoint = center + new Vector2(radius, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Vector2 nextPoint = center + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
                );

                DrawLine(spriteBatch, pixel, prevPoint, nextPoint, color);
                prevPoint = nextPoint;
            }
        }
    }
}