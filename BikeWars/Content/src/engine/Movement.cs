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
    public override void HandleMovement(GameTime gameTime){
    }
    public override void Update(GameTime gameTime){}
}

