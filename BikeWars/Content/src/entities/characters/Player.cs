using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BikeWars.Components;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Entities.Characters
{
    public class Player
    {
        public Transform Transform;
        public Transform lastTransform;
        public float Speed = 200f;
        public Color Tint = Color.Black;
        private BoxCollider _collider { get; set; }

        public void UpdateCollider()
        {
            _collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y);
        }
        
        // 1x1 Texture to represent the player
        public static Texture2D pixel;

        public Player(Vector2 start, Point size)
        {
            Transform = new Transform(start, size);
            lastTransform = new Transform(start, size);
            UpdateCollider();
        }
        public bool Intersects(ICollider collider)
        {
            return _collider.Intersects(collider);
        }

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();

            Vector2 direction = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.W))
                direction.Y -= 1;
            if (keyboardState.IsKeyDown(Keys.S))
                direction.Y += 1;
            if (keyboardState.IsKeyDown(Keys.A))
                direction.X -= 1;
            if (keyboardState.IsKeyDown(Keys.D))
                direction.X += 1;

            lastTransform = new Transform(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size);
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                Transform.Position += direction * Speed * delta;
            }
            UpdateCollider();
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