using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework.Content;
using System;

namespace BikeWars.Content.entities.items;
public class Chest: ItemBase, IPickable
{
    public override bool InventoryItem => true;

    private const String TEXTURE_PATH = "assets/sprites/chest_texture";
    private BoxCollider _collider { get; set; }
    public override BoxCollider Collider
    {
        get { return _collider; }
    }

    public Chest(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);
        _collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.CHARACTER);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.White);
    }

    public override void Update(GameTime gameTime)
    {
    }
    public override void LoadContent(ContentManager content)
    {
        TexRight = content.Load<Texture2D>(TEXTURE_PATH);
        CurrentTex = TexRight;
    }
    public override bool Intersects(ICollider collider)
    {
        return _collider.Intersects(collider);
    }
}
