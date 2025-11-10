using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine;
public class Movement
{
    private Vector2 _direction;

    public Movement()
    {
        _direction = Vector2.Zero;
    }
    public void HandleBasicDirections()
    {
        // if (keyboardState.IsKeyDown(Keys.W))
        //         _direction.Y -= 1;
        //     if (keyboardState.IsKeyDown(Keys.S))
        //         _direction.Y += 1;
        //     if (keyboardState.IsKeyDown(Keys.A))
        //         _direction.X -= 1;
        //     if (keyboardState.IsKeyDown(Keys.D))
        //         _direction.X += 1;   
    }
}

