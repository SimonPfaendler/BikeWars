using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Components;

namespace BikeWars.Content.entities.items;
public class TestItem
{
    public Transform Transform;
    public Color Tint = Color.Red;

    public static Texture2D pixel;

    public TestItem(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (pixel == null)
        {
            // Create a 1x1 white texture if it doesn't exist
            pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
        }
        spriteBatch.Draw(pixel,Transform.Bounds, Tint);
    }
}
