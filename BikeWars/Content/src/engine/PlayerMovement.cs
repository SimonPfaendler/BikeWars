using System;
using Microsoft.Xna.Framework;
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
        List<MoveDirection> directions = new List<MoveDirection>();
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

        
        var leftStick = InputHandler.GamePad.LeftStick;

        if (leftStick != Vector2.Zero)
        {
            if (CurrentMovement is BicycleMovement)
            {
                // Bicycle Logic: Steer towards stick direction, always accelerate (Forward)
                directions.Add(MoveDirection.FORWARD);

                float targetAngle = (float)System.Math.Atan2(leftStick.Y, leftStick.X);
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
                // Walking Movement Logic: Move in stick direction
                if (leftStick.Y < -0.5f)
                {
                    directions.Add(MoveDirection.UP);
                    directions.Add(MoveDirection.FORWARD);
                }
                else if (leftStick.Y > 0.5f)
                {
                    directions.Add(MoveDirection.DOWN);
                    directions.Add(MoveDirection.BACKWARD);
                }

                if (leftStick.X < -0.5f)
                {
                    directions.Add(MoveDirection.LEFT);
                }
                else if (leftStick.X > 0.5f)
                {
                    directions.Add(MoveDirection.RIGHT);
                }
            }
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
