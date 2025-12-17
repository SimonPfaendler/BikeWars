using Microsoft.Xna.Framework;
using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using System.Collections.Generic;
using BikeWars.Content.entities.interfaces;
using System;

namespace BikeWars.Content.engine;
public class PlayerMovement
{
    private IMoveable _currentMovement {get; set;}
    public IMoveable CurrentMovement {get => _currentMovement; set => _currentMovement = value;}
    private IPlayerInput _input;

    private Bike _bike {get; set;}
    public Bike CrtBike {get => _bike; set => _bike = value;}

    public event Action<Bike> OnDismounted;

    private bool owns_bike {get; set;}
    public bool OwnsBike {
        get => owns_bike;
        set => owns_bike = value;
    }

    public float WalkingSpeed = 120f;
    public float SprintAcceleration = 1.5f;

    public float Rotation = 0.0f; // in Radiant
    public float RotationAcceleration = 0.1f;
    public float SpeedAcceleration = 4f;

    public PlayerMovement(bool canMove, bool isMoving, IPlayerInput input)
    {
        _input = input;
        if (OwnsBike && CrtBike != null)
        {
            CurrentMovement = new BicycleMovement(canMove, isMoving, 0, CrtBike.Attributes.MaxSpeed, CrtBike.Attributes.SpeedAcceleration, CrtBike.Attributes.SprintAcceleration, CrtBike.Attributes.RotationAcceleration);
            return;
        }
        CurrentMovement = new WalkingMovement(canMove, isMoving, WalkingSpeed, SprintAcceleration);
    }

    public void SwitchBicycle(Bike b)
    {
        if (CurrentMovement is WalkingMovement)
        {
            CurrentMovement = new BicycleMovement(CurrentMovement.CanMove, CurrentMovement.IsMoving, 0, b.Attributes.MaxSpeed, b.Attributes.SpeedAcceleration, b.Attributes.SprintAcceleration, b.Attributes.RotationAcceleration);
            switch (b) {
                case Frelo:
                    CrtBike = new Frelo(b.Transform.Position, b.Transform.Size);
                    break;
                case RacingBike:
                    CrtBike = new RacingBike(b.Transform.Position, b.Transform.Size);
                    break;
            }
            OwnsBike = true;
            return;
        }
    }

    public void Dismount()
    {
        CurrentMovement = new WalkingMovement(CurrentMovement.CanMove, CurrentMovement.IsMoving, WalkingSpeed, SprintAcceleration);
        OwnsBike = false;
        OnDismounted?.Invoke(CrtBike);
        CrtBike = null;
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
        CurrentMovement.HandleMovement(MakeMoveDirections(), CurrentMovement.Speed, SpeedAcceleration, Rotation, RotationAcceleration, 0, CurrentMovement.MaxSpeed);
    }

    public bool IsMoving()
    {
        return CurrentMovement.IsMoving;
    }
}
