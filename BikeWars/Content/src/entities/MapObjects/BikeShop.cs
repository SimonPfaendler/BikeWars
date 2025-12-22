using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Entities.Characters.MapObjects;

public class BikeShop: ItemBase
{
    public BikeShop(Vector2 start, Point size, TiledObjectInfo attributes)
    {
        Transform = new Transform(start, size);
        Collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.ITEM, this);
        TexRight = SpriteManager.GetTexture("Fahrradwerkstatt");
        CurrentTex = TexRight;
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (CurrentTex == null) return;
        spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.White);
    }

    public override void Update(GameTime gameTime)
    {
        // nichts
    }
    
    public override bool Intersects(ICollider other)
    {
        return Collider.Intersects(other);
    }
}