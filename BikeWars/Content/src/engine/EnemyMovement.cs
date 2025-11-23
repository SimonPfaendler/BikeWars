using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;

// Handles the enemy movement:
// the enemy chases the player
// if it collides with an object on the map, the enemy will turn 90 degrees
// to the right and try again

namespace BikeWars.Content.engine;

public enum EnemyState
{
    Chasing,
    Sidestepping
}

public class EnemyMovement : MovementBase
{
    public EnemyState State { get; private set; } = EnemyState.Chasing;

    private float _sidestepTimeLeft = 0f;
    private const float SidestepDuration = 0.4f;

    private Vector2 _sidestepDirection;

    public Vector2 PlayerPosition;
    public Vector2 EnemyPosition;

    private const float StopRadius = 50f;

    private int _sidestepCount = 0;

    public EnemyMovement(bool canMove, bool isMoving)
    {
        Direction = Vector2.Zero;
        CanMove = canMove;
        IsMoving = isMoving;
    }

    // controls chasing and a timed sidestepping 
    public override void HandleMovement(GameTime gameTime)
    {
        if (!CanMove) return;

        if (State == EnemyState.Chasing)
        {
            Direction = DirectionToTarget();
        }
        else if (State == EnemyState.Sidestepping)
        {
            Direction = _sidestepDirection;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _sidestepTimeLeft -= delta;

            if (_sidestepTimeLeft <= 0)
            {
                State = EnemyState.Chasing;
                _sidestepCount = 0; 
            }
        }

        Update(gameTime);
    }

    // calculates the length from the enemy to the player
    private Vector2 DirectionToTarget()
    {
        Vector2 toTarget = PlayerPosition - EnemyPosition;

        if (toTarget.LengthSquared() < StopRadius * StopRadius)
            return Vector2.Zero;

        toTarget.Normalize();
        return toTarget;
    }

    public void StartSidestepping(Vector2 currentDirection)
    {
        if (currentDirection == Vector2.Zero)
            currentDirection = DirectionToTarget();

        if (currentDirection == Vector2.Zero)
            return;

        currentDirection.Normalize();

        // Rotate the current direction by 90° per sidestep to find the next escape direction.
        float angle = MathHelper.ToRadians(90 * (_sidestepCount + 1));

        Vector2 rotated = new Vector2(
            currentDirection.X * (float)Math.Cos(angle) - currentDirection.Y * (float)Math.Sin(angle),
            currentDirection.X * (float)Math.Sin(angle) + currentDirection.Y * (float)Math.Cos(angle)
        );

        rotated.Normalize();
        _sidestepDirection = rotated;

        _sidestepTimeLeft = SidestepDuration;
        State = EnemyState.Sidestepping;

        _sidestepCount++;
        if (_sidestepCount > 3)
            _sidestepCount = 0;
    }

    private bool UpdateMoving() => Direction != Vector2.Zero;

    public override void Update(GameTime gameTime)
    {
        IsMoving = UpdateMoving();
    }
}
