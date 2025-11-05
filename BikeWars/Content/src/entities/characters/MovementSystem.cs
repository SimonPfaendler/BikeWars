using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BikeWars.Entities.Characters;

namespace BikeWars.Systems
{
    public class MovementSystem
    {
        public Player player;


        public MovementSystem(Player player)
        {
            this.player = player;
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

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                player.Transform.Position += direction * player.Speed * delta;
            }
        }
    }
}