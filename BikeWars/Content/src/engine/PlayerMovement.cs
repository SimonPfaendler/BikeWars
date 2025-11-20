using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;
using BikeWars.Content.components;
using System;

namespace BikeWars.Content.engine;
public class PlayerMovement: MovementBase
{
    public float Rotation = 0.0f; // in Radiant
    public float Speed = 0.0f;

    public float RotationAcceleration = 0.1f;
    public float SpeedAcceleration = 1f;
    public float MaxSpeed = 200f;
    public float Friction = 0.95f;
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
        MakeRotation();
        Update(gameTime);
    }

    private Vector2 MakeDirection()
    {
        Vector2 direction = Vector2.Zero;
        if (InputHandler.IsHeld(GameAction.MOVE_UP))
        {
            // direction += DirectionHelper.Get(MoveDirection.UP);
            // direction += DirectionHelper.Get(MoveDirection.UP);
            Speed = MathHelper.Clamp(Speed + SpeedAcceleration, 0, MaxSpeed);
        }
        if (InputHandler.IsHeld(GameAction.MOVE_DOWN))
        {
            // direction += DirectionHelper.Get(MoveDirection.UP);
            Speed = MathHelper.Clamp(Speed - 2*SpeedAcceleration, 0, MaxSpeed);
            // direction += DirectionHelper.Get(MoveDirection.DOWN);
        }
        if (InputHandler.IsHeld(GameAction.MOVE_LEFT))
        {
            // direction += DirectionHelper.Get(MoveDirection.LEFT);
        }
        if (InputHandler.IsHeld(GameAction.MOVE_RIGHT))
        {
            // direction += DirectionHelper.Get(MoveDirection.RIGHT);
        }
        direction = new Vector2(
            (float)Math.Cos(Rotation),
            (float)Math.Sin(Rotation)
        );
        Vector2 stick = InputHandler.GamePad.LeftStick;
        if (stick != Vector2.Zero)
        {
            direction += stick;
        }
        return direction;
    }
    private void MakeRotation()
    {
        if (InputHandler.IsHeld(GameAction.MOVE_LEFT))
        {
            Rotation -= RotationAcceleration;
        }
        if (InputHandler.IsHeld(GameAction.MOVE_RIGHT))
        {
            Rotation += RotationAcceleration;
        }
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
