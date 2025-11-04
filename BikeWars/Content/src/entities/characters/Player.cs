using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Components;


namespace BikeWars.Entities.Characters
{
    public class Player
    {
        public Transform Transform;
        public float Speed = 200f;
        public Color Tint = Color.Black;

        // 1x1 Texture to represent the player
        public static Texture2D pixel;

        public Player(Vector2 start, Point size)
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
            // Draw the player as a colored rectangle
            spriteBatch.Draw(pixel,Transform.Bounds, Tint);
        }
    }
}