using System.Collections.Generic;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public abstract class MovementBase : IMoveable
{
    private Vector2 _direction { get; set; }
    private bool _isMoving { get; set; }
    private bool _canMove { get; set; }
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
    private float _speed { get; set; }
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
            if (value < 0)
            {
                _rotation = 0;
                return;
            }
            _rotation = value;
        }
    }

    public abstract void Update(GameTime gameTime);
    public abstract void HandleMovement(GameTime gameTime);

    public void HandleMovement(List<MoveDirection> moveDirection, float currentSpeed, float speedAcceleration, float currentRotation, float rotationAcceleration, float minSpeed, float maxSpeed)
    {

    }

    public Vector2 HandleDirection(List<MoveDirection> moveDirection)
    {
        return Vector2.Zero;
    }

    public float HandleSpeed(List<MoveDirection> direction, float currentSpeed, float acceleration, float minSpeed, float maxSpeed)
    {
        return 0f;
    }

    public float HandleRotation(List<MoveDirection> moveDirections)
    {
        return 0f;
    }
}
