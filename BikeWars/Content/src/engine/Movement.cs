using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine;
public class Movement: MovementBase
{
    public Movement(bool canMove, bool isMoving)
    {
        Direction = Vector2.Zero;
        CanMove = canMove;
        IsMoving = isMoving;
    }
    public void HandleBasicDirections(GameTime gameTime)
    {
        if (!CanMove)
        {
            return;
        }
    }
    public override void HandleMovement(GameTime gameTime)
    {
        if (!CanMove)
        {
            return;
        }
    }

    public override void Update(GameTime gameTime){}
}

