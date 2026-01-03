using System;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Entities.Characters.MapObjects;

public class DestructibleObject : ItemBase
{
    public int Health { get; private set; }

    public DestructibleObject(Vector2 start, Point size, TiledObjectInfo attributes)
    {
        Transform = new Transform(start, size);

        // default HP = 1, can be set via Tiled object property "hp"
        int hp = 1;
        try
        {
            if (attributes.Properties.ContainsKey("hp"))
                int.TryParse(attributes.Properties["hp"], out hp);
        }
        catch { }
        Health = Math.Max(1, hp);

        // Try to load a texture via SpriteManager using property "sprite" (key)
        Texture2D? tex = null;
        try
        {
            string key = attributes.Properties.ContainsKey("sprite") ? attributes.Properties["sprite"] : "Chest";
            tex = SpriteManager.GetTexture(key);
            CurrentTex = tex;
        }
        catch
        {
            CurrentTex = null;
        }

        // If the Tiled object had no explicit size, use the texture size so it's visible
        if ((Transform.Size.X == 0 || Transform.Size.Y == 0) && tex != null)
        {
            Transform.Size = new Point(tex.Width, tex.Height);
        }

        // Use WALL layer so this object blocks movement like other map objects
        Collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.WALL, this);
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;
    }

    public override void Update(GameTime gameTime)
    {
        // nothing for now
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (CurrentTex == null) return;
        spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.White);
    }

    public override bool Intersects(BikeWars.Content.engine.interfaces.ICollider other)
    {
        return Collider.Intersects(other);
    }
}
