using System;
using System.Numerics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.managers;
public class CollisionManager
{
    // private SpatialHash _spatialHash {get; set;}
    private SpatialHash _dynamicHash {get; set;}
    public SpatialHash DynamicHash {get => _dynamicHash; set => _dynamicHash = value;}
    private SpatialHash _staticHash {get; set;}
    public SpatialHash StaticHash {get => _staticHash; set => _staticHash = value;}
    public CollisionManager(int cellSize, float insertRadius)
    {
        _dynamicHash = new SpatialHash(cellSize, insertRadius);
        _staticHash = new SpatialHash(cellSize, insertRadius);
    }
    public bool isColliding(ICollider collisionBox1, ICollider collisionBox2)
    {
        return collisionBox1.Intersects(collisionBox2);
    }

    public Vector2 GetPenetrationVector(ICollider a, ICollider b)
    {

        // TODO if we want to use Circle too we need to implement it
        if (a is BoxCollider bca && b is BoxCollider bcb)
        {
            // Calculate center to center
            float aWidthHalf = bca.Width / 2;
            float aHeightHalf = bca.Height / 2;
            float bWidthHalf = bcb.Width / 2;
            float bHeightHalf = bcb.Height / 2;

            Vector2 aCenter = new Vector2(a.Position.X + aWidthHalf, a.Position.Y + aHeightHalf);
            Vector2 bCenter = new Vector2(b.Position.X + bWidthHalf, b.Position.Y + bHeightHalf);

            float dx = bCenter.X - aCenter.X;
            float dy = bCenter.Y - aCenter.Y;
            float px = aWidthHalf  + bWidthHalf  - Math.Abs(dx);
            float py = aHeightHalf  + bHeightHalf  - Math.Abs(dy);

            if (px <= 0 || py <= 0) {
                return Vector2.Zero; // No collision
            }

            if (px < py)
            {
                return new Vector2(dx < 0 ? -px : px, 0);
            }
            else
            {
                return new Vector2(0, dy < 0 ? -py : py);
            }
        }
        return new Vector2(0,0);
    }
}