using System.Drawing;
using Microsoft.Xna.Framework.Graphics;

// We actually use this a lot to handle the drawing of the 2d Images
// It should help now that we don't have memory leaks because of this.
public static class RenderPrimitives
{
    public static Texture2D Pixel { get; private set; }

    public static void Init(GraphicsDevice gd)
    {
        Pixel = new Texture2D(gd, 1, 1, false, SurfaceFormat.Color);
        Pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
    }
    public static void Dispose()
    {
            Pixel?.Dispose();
    }
}
