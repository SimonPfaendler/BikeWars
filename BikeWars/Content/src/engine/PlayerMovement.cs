using Microsoft.Xna.Framework;
using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using System.Collections.Generic;

namespace BikeWars.Content.engine;
public class PlayerMovement
{
    private IMoveable _currentMovement {get; set;}
    public IMoveable CurrentMovement {get => _currentMovement; set => _currentMovement = value;}
    private IPlayerInput _input;

    public float Rotation = 0.0f; // in Radiant

    public float RotationAcceleration = 0.1f;
    public float SpeedAcceleration = 4f;
    public float MaxSpeed = 200f;
    public float Friction = 0.95f;

    public PlayerMovement(bool canMove, bool isMoving, IPlayerInput input)
    {
        CurrentMovement = new BicycleMovement(canMove, isMoving, RotationAcceleration);
        _input = input;
    }
    private List<MoveDirection> MakeMoveDirections()
    {
        List<MoveDirection> directions = new List<MoveDirection>();
        
        Vector2 inputDir = _input.GetMovementDirection(CurrentMovement);

        if (inputDir == Vector2.Zero) 
            return directions;

        if (CurrentMovement is BicycleMovement)
        {
             // Bicycle Logic Mapping
             // Check if input is Analog (Gamepad) for special Steering Logic
             if (_input.IsAnalog)
             {
                 // Analog Stick Logic
                 directions.Add(MoveDirection.FORWARD); // Always accelerate

                 float targetAngle = (float)System.Math.Atan2(inputDir.Y, inputDir.X);
                 float currentRotation = CurrentMovement.Rotation;

                 // Normalize difference to -Pi to +Pi
                 float diff = targetAngle - currentRotation;
                 while (diff <= -MathHelper.Pi) diff += MathHelper.TwoPi;
                 while (diff > MathHelper.Pi) diff -= MathHelper.TwoPi;

                 // Deadzone for rotation stability
                 if (System.Math.Abs(diff) > 0.1f)
                 {
                     if (diff > 0)
                         directions.Add(MoveDirection.RIGHT);
                     else
                         directions.Add(MoveDirection.LEFT);
                 }
             }
             else
             {
                 
                 if (inputDir.Y < -0.3f) // UP / Accelerate
                 {
                     directions.Add(MoveDirection.UP);
                     directions.Add(MoveDirection.FORWARD);
                 }
                 if (inputDir.Y > 0.3f) // DOWN / Brake
                 {
                     directions.Add(MoveDirection.DOWN);
                     directions.Add(MoveDirection.BACKWARD);
                 }
                 
                 // Rotation
                 if (inputDir.X < -0.3f)
                 {
                     directions.Add(MoveDirection.LEFT);
                 }
                 if (inputDir.X > 0.3f)
                 {
                     directions.Add(MoveDirection.RIGHT);
                 }
             }
        }
        else
        {
            // Walking Logic
            if (inputDir.Y < -0.3f)
            {
                directions.Add(MoveDirection.UP);
                directions.Add(MoveDirection.FORWARD);
            }
            if (inputDir.Y > 0.3f)
            {
                directions.Add(MoveDirection.DOWN);
                directions.Add(MoveDirection.BACKWARD);
            }
            if (inputDir.X < -0.3f)
            {
                directions.Add(MoveDirection.LEFT);
            }
            if (inputDir.X > 0.3f)
            {
                directions.Add(MoveDirection.RIGHT);
            }
        }

        return directions;
    }
    public void Update()
    {
        _input.Update(); 
        CurrentMovement.HandleMovement(MakeMoveDirections(), CurrentMovement.Speed, SpeedAcceleration, Rotation, RotationAcceleration, 0, MaxSpeed);
    }

    public bool IsMoving()
    {
        return CurrentMovement.IsMoving;
    }
}
