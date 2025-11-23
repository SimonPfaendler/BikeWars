using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using System.Collections.Generic;

namespace BikeWars.Content.engine;
public class PlayerMovement
{
    private IMoveable _currentMovement {get; set;}
    public IMoveable CurrentMovement {get => _currentMovement; set => _currentMovement = value;}

    public float Rotation = 0.0f; // in Radiant

    public float RotationAcceleration = 0.1f;
    public float SpeedAcceleration = 4f;
    public float MaxSpeed = 200f;
    public float Friction = 0.95f;

    public PlayerMovement(bool canMove, bool isMoving)
    {
        CurrentMovement = new BicycleMovement(canMove, isMoving, RotationAcceleration);
    }
    private List<MoveDirection> MakeMoveDirections()
    {
        List<MoveDirection> directions = [];
        if (InputHandler.IsHeld(GameAction.MOVE_UP))
        {
            directions.Add(MoveDirection.UP);
            directions.Add(MoveDirection.FORWARD);
        }
        if (InputHandler.IsHeld(GameAction.MOVE_DOWN))
        {
            directions.Add(MoveDirection.DOWN);
            directions.Add(MoveDirection.BACKWARD);
        }
        if (InputHandler.IsHeld(GameAction.MOVE_LEFT))
        {
            directions.Add(MoveDirection.LEFT);
        }
        if (InputHandler.IsHeld(GameAction.MOVE_RIGHT))
        {
            directions.Add(MoveDirection.RIGHT);
        }
        return directions;
    }
    public void Update()
    {
        CurrentMovement.HandleMovement(MakeMoveDirections(), CurrentMovement.Speed, SpeedAcceleration, Rotation, RotationAcceleration, 0, MaxSpeed);
    }

    public bool IsMoving()
    {
        return CurrentMovement.IsMoving;
    }
}
