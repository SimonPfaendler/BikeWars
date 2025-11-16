using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;



namespace BikeWars.Content.engine;
public class EnemyMovement: MovementBase
{
    private double _walkTimer {set; get;}
    public EnemyMovement(bool canMove, bool isMoving)
    {
        Direction = Vector2.Zero;
        CanMove = canMove;
        IsMoving = isMoving;
        _walkTimer = 0;
    }

    public override void HandleMovement(GameTime gameTime)
    {
        if (!CanMove)
        {
            return;
        }
        _walkTimer += gameTime.ElapsedGameTime.TotalSeconds; // This should be just temporary until we implement it correctly.
        Direction = MakeDirection();
        Update(gameTime);
    }

    private Vector2 MakeDirection()
    {
        Vector2 direction = Vector2.Zero;
        if (_walkTimer <= 2) // Needs improvement but it's ok for the start
        {
            direction = DirectionHelper.Get(MoveDirection.LEFT);
        } else if (_walkTimer <= 4)
        {
            direction = DirectionHelper.Get(MoveDirection.RIGHT);
        }
        else
        {
            _walkTimer = 0;
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
