using System;
using System.Collections.Generic;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public class BicycleMovement : IMoveable
{
    private Vector2 _direction { get; set; }
    private bool _isMoving { get; set; }
    private bool _canMove { get; set; }
    private float _speed { get; set; }
    private float _maxSpeed { get; set; }
    private float _sprintAcceleration { get; set; }
    private float _rotation { get; set; }
    private float _rotationAcceleration { get; set; }

    private const float BreakAcceleration = 1.5f; // Right now this works for every bike but question would be to make it dynamic so other bikes can break faster or so
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

     public float RotationAcceleration {
        get => _rotationAcceleration;
        set
        {
            _rotationAcceleration = value;
        }
    }

    public float MaxSpeed {
        get => _maxSpeed;
        set
        {
            _maxSpeed = value;
        }
    }

    public float SprintAcceleration {
        get => _sprintAcceleration;
        set
        {
            if (value < 0)
            {
                _sprintAcceleration = 0;
                return;
            }
            _sprintAcceleration = value;
        }
    }

    private SteeringCurve _steeringCurve;
    public SteeringCurve Curve => _steeringCurve;

    public BicycleMovement(bool canMove, bool isMoving, float speed, float maxSpeed, float speedAcceleration, float sprintAcceleration, float rotationAcceleration)
    {
        CanMove = canMove;
        IsMoving = isMoving;
        Speed = speed;
        MaxSpeed = maxSpeed;
        SprintAcceleration = sprintAcceleration;
        RotationAcceleration = rotationAcceleration;
        _steeringCurve = new SteeringCurve();
    }
    private bool MakeIsMoving()
    {
        return Direction != Vector2.Zero;
    }
    private float MovingForward(float currentSpeed, float acceleration, float minSpeed, float maxSpeed)
    {
        return MathHelper.Clamp(currentSpeed + acceleration, minSpeed, maxSpeed);
    }

    private float MovingBackwards(float currentSpeed, float acceleration, float minSpeed, float maxSpeed)
    {
        return MathHelper.Clamp(currentSpeed - acceleration, minSpeed, maxSpeed);
    }

    public float HandleRotation(List<MoveDirection> moveDirections, float rotationAcceleration)
    {
        float steerFactor = _steeringCurve.Evaluate(Speed);
        float adjustedRotation = rotationAcceleration * steerFactor;

        foreach (var dir in moveDirections)
        {
            if (dir == MoveDirection.LEFT)
            {
                Rotation -= adjustedRotation;
            }
            if (dir == MoveDirection.RIGHT)
            {
                Rotation += adjustedRotation;
            }
        }
        return Rotation;
    }
    public Vector2 HandleDirection(List<MoveDirection> moveDirections)
    {
        return new Vector2(
            (float)Math.Cos(Rotation),
            (float)Math.Sin(Rotation)
        );
    }

    public float HandleSpeed(List<MoveDirection> moveDirections, float currentSpeed, float acceleration, float minSpeed, float maxSpeed)
    {
        foreach (var dir in moveDirections)
        {
            switch (dir)
            {
                case MoveDirection.FORWARD:
                    return MovingForward(currentSpeed, acceleration, minSpeed, maxSpeed);
                case MoveDirection.BACKWARD:
                    return MovingBackwards(currentSpeed, acceleration * BreakAcceleration, minSpeed, maxSpeed);
                default:
                    break;
            }
        }
        return MovingBackwards(currentSpeed, acceleration, minSpeed, maxSpeed);
    }

    public void HandleMovement(List<MoveDirection> moveDirections, float currentSpeed, float speedAcceleration, float currentRotation, float rotationAcceleration, float minSpeed, float maxSpeed)
    {
        Rotation = HandleRotation(moveDirections, rotationAcceleration);
        Direction = HandleDirection(moveDirections);
        IsMoving = MakeIsMoving();
        Speed = HandleSpeed(moveDirections, currentSpeed, speedAcceleration, minSpeed, maxSpeed);
    }
}
