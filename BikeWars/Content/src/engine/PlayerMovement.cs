using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;
using static DirectionHelper;

namespace BikeWars.Content.engine;
public class PlayerMovement: MovementBase
{
    private readonly InputHandler _input;

    public PlayerMovement(bool canMove, bool isMoving, InputHandler inputHandler)
    {
        Direction = Vector2.Zero;
        CanMove = canMove;
        IsMoving = isMoving;
        _input = inputHandler;
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
        if (_input.PressingAction(GameAction.MOVE_UP))
        {
            direction += Get(global::Direction.UP);
        }
        if (_input.PressingAction(GameAction.MOVE_DOWN))
        {
            direction += Get(global::Direction.DOWN);
        }
        if (_input.PressingAction(GameAction.MOVE_LEFT))
        {
            direction += Get(global::Direction.LEFT);
        }
        if (_input.PressingAction(GameAction.MOVE_RIGHT))
        {
            direction += Get(global::Direction.RIGHT);
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

