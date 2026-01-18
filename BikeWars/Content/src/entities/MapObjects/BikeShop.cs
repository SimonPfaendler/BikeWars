using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Entities.Characters.MapObjects;

public class BikeShop: ObjectBase
{
    private BoxCollider _collisionCollider {get;set;}
    public BoxCollider CollisionCollider {get => _collisionCollider; set => _collisionCollider = value; } // Now this collider is for collision

    private int PADDING_INTERACTION_AREA = 40;
    public BikeShop(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);
        Collider = new BoxCollider(new Vector2(Transform.Position.X - PADDING_INTERACTION_AREA / 2, Transform.Position.Y - PADDING_INTERACTION_AREA / 2), Transform.Size.X + PADDING_INTERACTION_AREA, Transform.Size.Y + PADDING_INTERACTION_AREA, CollisionLayer.INTERACT, this);
        CollisionCollider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.WALL, this);
        CurrentTex = SpriteManager.GetTexture("Fahrradwerkstatt");
    }
}