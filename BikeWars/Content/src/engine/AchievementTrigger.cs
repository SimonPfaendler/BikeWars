#nullable enable
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;

namespace BikeWars.Entities;
public class AchievementTrigger : ObjectBase
{
    public AchievementIds Id;
    public AchievementTrigger(string id, Vector2 start, Point size, TiledObjectInfo attributes)
    {
        Id = AchievementIdConverter.Convert(id);
        Transform = new Transform(start, size);
        // Use WALL layer so this object blocks movement like other map objects
        Collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.TRIGGER, this);
    }

    public override void Update(GameTime gameTime) {
    }

    public override bool Intersects(ICollider other)
    {
        return Collider.Intersects(other);
    }
}
