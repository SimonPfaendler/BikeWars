using System;
using System.Collections.Generic;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public class WalkingMovement : IMoveable
{
    private Vector2 _direction { get; set; }
    private bool _isMoving { get; set; }
    private bool _canMove { get; set; }
    private float _speed { get; set; }

    public bool IsMoving { get => _isMoving; set => _isMoving = value; }
    public bool CanMove {
        get => _canMove;
        set {
            _canMove = value;
            if (!_canMove)
            {
                IsMoving = false;
                Direction = Vector2.Zero;
            }
        }
    }

    public Vector2 Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            if (_direction.X == 1)
            {

            }
            // Update();
        }
    }

    public float Speed {
        get => _speed;
        set
        {
            if (value < 0)
            {
                _speed = 0;
                return;
            }
            _speed = value;
        }
    }

    private bool UpdateMoving()
    {
        return Direction != Vector2.Zero;
    }
    public void Update(GameTime gameTime)
    {
        IsMoving = UpdateMoving();
    }

    public WalkingMovement(bool canMove, bool isMoving)
    {
        CanMove = canMove;
        IsMoving = isMoving;
    }
    public void HandleMovement(List<MoveDirection> moveDirections, float currentSpeed, float speedAcceleration, float currentRotation, float rotationAcceleration, float minSpeed, float maxSpeed)
    {
        Direction = HandleDirection(moveDirections);
        Speed = HandleSpeed(moveDirections, currentSpeed, speedAcceleration, minSpeed, maxSpeed);
    }

    // public Vector2 HandleDirection(float rotation)
    // public Vector2 HandleDirection(float rotation)
    public Vector2 HandleDirection(List<MoveDirection> moveDirections)
    {
        Direction = Vector2.Zero;
        // if (moveDirection == MoveDirection.UP)
        // {
        foreach (var dir in moveDirections)
        {
            Direction += DirectionHelper.Get(dir);
        }
        return Direction;
        // }
        // if (InputHandler.IsHeld(GameAction.MOVE_UP))
        // {
        //     moveDirection = MoveDirection.UP;
        //     // direction += DirectionHelper.Get(MoveDirection.UP);
        //     // direction += DirectionHelper.Get(MoveDirection.UP);
        //     // Speed = CurrentMovement.MovingFoward()MathHelper.Clamp(Speed + SpeedAcceleration, 0, MaxSpeed);
        // } else
        // {
        //     moveDirection = MoveDirection.NONE;
        //     // Speed = MathHelper.Clamp(Speed - SpeedAcceleration * 0.9f, 0, MaxSpeed);
        // }
        // if (InputHandler.IsHeld(GameAction.MOVE_DOWN))
        // {
        //     // direction += DirectionHelper.Get(MoveDirection.UP);
        //     // Speed = MathHelper.Clamp(Speed - SpeedAcceleration * 1.5f, 0, MaxSpeed);
        //     moveDirection = MoveDirection.DOWN;
        //     // direction += DirectionHelper.Get(MoveDirection.DOWN);
        // }

        // return;
        // return new Vector2(
        //     (float)Math.Cos(rotation),
        //     (float)Math.Sin(rotation)
        // );
    }

    public float HandleSpeed(List<MoveDirection> moveDirections, float currentSpeed, float acceleration, float minSpeed, float maxSpeed)
    {
        if (moveDirections.Count < 1)
        {
            return 0;
        }
        return maxSpeed;
    }
}
