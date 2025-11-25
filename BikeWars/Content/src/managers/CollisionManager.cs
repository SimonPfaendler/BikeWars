using System;
using System.Numerics;
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
}