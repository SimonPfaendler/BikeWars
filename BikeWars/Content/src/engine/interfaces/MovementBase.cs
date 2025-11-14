using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public abstract class MovementBase : IMoveable
{
    private Vector2 _direction { get; set; }
    private bool _isMoving { get; set; }
    private bool _canMove { get; set; }

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
    public abstract void Update(GameTime gameTime);
    public abstract void HandleMovement(GameTime gameTime);
}
