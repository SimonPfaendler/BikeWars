using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.managers;
public class CollisionManager
{
    public bool isColliding(ICollider collisionBox1, ICollider collisionBox2)
    {
        return collisionBox1.Intersects(collisionBox2);
    }
}