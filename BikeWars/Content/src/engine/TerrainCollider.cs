using System;
using Microsoft.Xna.Framework;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.engine
{
    public class TerrainCollider : ColliderBase
    {
        public Rectangle Bounds { get; private set; }
        public TerrainType TerrainType { get; private set; }


        public TerrainCollider(Vector2 position, int width, int height, TerrainType type)
        {
            TerrainType = type;
            Width = width;
            Height = height;

            Position = position;
            Layer = CollisionLayer.TERRAIN;
            Owner = this;
            Radius = Math.Max(width, height) * 0.5f;

            Update();
        }

        protected override void Update()
        {
            Bounds = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                Width,
                Height
            );
        }

        public override bool Intersects(ICollider other)
        {
            if (other is BoxCollider)
            {
                Rectangle otherRect = new Rectangle(
                    (int)other.Position.X,
                    (int)other.Position.Y,
                    other.Width,
                    other.Height
                );

                return Bounds.Intersects(otherRect);
            }

            return false;
        }
    }
}

