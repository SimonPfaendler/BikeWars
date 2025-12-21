using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine;
public class BulletMovement: MovementBase
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
        // Direction is set externally (e.g. by GameScreen)
        Update(gameTime);
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
