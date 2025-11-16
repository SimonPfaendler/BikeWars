using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;
using BikeWars.Content.components;



namespace BikeWars.Content.engine;
public class PlayerMovement: MovementBase
{
    public PlayerMovement(bool canMove, bool isMoving)
    {
        Direction = Vector2.Zero;
        CanMove = canMove;
        IsMoving = isMoving;
    }

    public override void HandleMovement(GameTime gameTime)
    {
        if (!CanMove)
        {
            return;
        }
        Direction = MakeDirection();
        Update(gameTime);
    }

    private Vector2 MakeDirection()
    {
        Vector2 direction = Vector2.Zero;
        if (InputHandler.IsHeld(GameAction.MOVE_UP))
        {
            direction += DirectionHelper.Get(MoveDirection.UP);
        }
        if (InputHandler.IsHeld(GameAction.MOVE_DOWN))
        {
            direction += DirectionHelper.Get(MoveDirection.DOWN);
        }
        if (InputHandler.IsHeld(GameAction.MOVE_LEFT))
        {
            direction += DirectionHelper.Get(MoveDirection.LEFT);
        }
        if (InputHandler.IsHeld(GameAction.MOVE_RIGHT))
        {
            direction += DirectionHelper.Get(MoveDirection.RIGHT);
        }

        Vector2 stick = InputHandler.GamePad.LeftStick;
        if (stick != Vector2.Zero)
        {
            direction += stick;
        }
        return direction;
    }

    private bool UpdateMoving()
    {
        return Direction != Vector2.Zero;
    }
    public override void Update(GameTime gameTime)
    {
        IsMoving = UpdateMoving();
    }
}
