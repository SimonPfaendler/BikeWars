using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.entities.items;
public class Item: ItemBase
{
    public Color Tint = Color.Red;
    public static Texture2D pixel;
    private BoxCollider _collider { get; set; }
    public override BoxCollider Collider
    {
        get { return _collider; }
    }
    
    public Item(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);
        _collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y);
    }

    public override void Update(GameTime gameTime)
    {

    }

    public override void LoadContent(ContentManager contentManager)
    {
        
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (pixel == null)
        {
            // Create a 1x1 white texture if it doesn't exist
            pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
        }
        spriteBatch.Draw(pixel, Transform.Bounds, Tint);
    }
    public override bool Intersects(ICollider collider)
        {
            return _collider.Intersects(collider);
        }   
}
