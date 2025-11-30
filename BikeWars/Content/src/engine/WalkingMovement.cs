using System;
using System.Collections.Generic;
using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine;
public class WalkingMovement : IMoveable
{
    private Vector2 _direction { get; set; }
    private bool _isMoving { get; set; }
    private bool _canMove { get; set; }
    private float _speed { get; set; }
    private float _rotation { get; set; }

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
    public float Rotation {
        get => _rotation;
        set
        {
            _rotation = value;
        }
    }

    private bool MakeIsMoving()
    {
        return Direction != Vector2.Zero;
    }

    public WalkingMovement(bool canMove, bool isMoving)
    {
        CanMove = canMove;
        IsMoving = isMoving;
    }
    public void HandleMovement(List<MoveDirection> moveDirections, float currentSpeed, float speedAcceleration, float currentRotation, float rotationAcceleration, float minSpeed, float maxSpeed)
    {
        Rotation = HandleRotation(moveDirections);
        Direction = HandleDirection(moveDirections);
        IsMoving = MakeIsMoving();
        Speed = HandleSpeed(moveDirections, currentSpeed, speedAcceleration, minSpeed, maxSpeed);
    }

    public float HandleRotation(List<MoveDirection> moveDirections)
    {
        return 0;
    }
    public Vector2 HandleDirection(List<MoveDirection> moveDirections)
    {
        Direction = Vector2.Zero;
        foreach (var dir in moveDirections)
        {
            Direction += DirectionHelper.Get(dir);
        }
        return Direction;
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
