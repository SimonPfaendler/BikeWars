using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;
using BikeWars.Content.components;
using System;

namespace BikeWars.Content.engine;
public class BulletMovement: MovementBase, IMoveable
{
    public BulletMovement(bool canMove, bool isMoving)
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
        // Needs a better implementation. Now it just flies to the right.
        Vector2 direction = DirectionHelper.Get(MoveDirection.RIGHT);
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
