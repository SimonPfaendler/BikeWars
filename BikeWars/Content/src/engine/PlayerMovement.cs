using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

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

    // public Vector2 Direction { get; set; }
    // public bool IsMoving { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    // public bool CanMove { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public PlayerMovement(bool canMove, bool isMoving)
    {
        // Direction = Vector2.Zero;
        // CanMove = canMove;
        // IsMoving = isMoving;
        // CurrentMovement = new BicycleMovement(canMove, isMoving, RotationAcceleration);
        CurrentMovement = new WalkingMovement(canMove, isMoving);
    }

    public void HandleMovement(GameTime gameTime)
    {
        if (!CurrentMovement.CanMove)
        {
            return;
        }
        UpdateMoving();
        // CurrentMovement.Direction = CurrentMovement.HandleDirection(Rotation);
        List<MoveDirection> moveDirections = MakeMoveDirections();
        CurrentMovement.Direction = CurrentMovement.HandleDirection(moveDirections);

        // if (InputHandler.IsHeld(GameAction.MOVE_UP))
        // {
        //     moveDirection = MoveDirection.UP;
        //     // direction += DirectionHelper.Get(MoveDirection.UP);
        //     // direction += DirectionHelper.Get(MoveDirection.UP);
        //     // Speed = CurrentMovement.MovingFoward()MathHelper.Clamp(Speed + SpeedAcceleration, 0, MaxSpeed);
        // } else
        // {
        //     moveDirection = MoveDirection.NONE;
        //     // Speed = MathHelper.Clamp(Speed - SpeedAcceleration * 0.9f, 0, MaxSpeed);
        // }
        // if (InputHandler.IsHeld(GameAction.MOVE_DOWN))
        // {
        //     // direction += DirectionHelper.Get(MoveDirection.UP);
        //     // Speed = MathHelper.Clamp(Speed - SpeedAcceleration * 1.5f, 0, MaxSpeed);
        //     moveDirection = MoveDirection.DOWN;
        //     // direction += DirectionHelper.Get(MoveDirection.DOWN);
        // }

        // Speed = CurrentMovement.HandleSpeed(moveDirections, Speed, SpeedAcceleration, 0 , MaxSpeed);
        // Console.WriteLine(Speed);
        // CurrentMovement.Direction = MakeDirection();
        // MakeRotation();
        // Update(gameTime);
    }

    private List<MoveDirection> MakeMoveDirections()
    {
        List<MoveDirection> directions = [];
        // Vector2 direction = Vector2.Zero;
        if (InputHandler.IsHeld(GameAction.MOVE_UP))
        {
            directions.Add(MoveDirection.UP);
            directions.Add(MoveDirection.FORWARD);
            // direction += DirectionHelper.Get(MoveDirection.UP);
            // direction += DirectionHelper.Get(MoveDirection.UP);
            // Speed = CurrentMovement.MovingFoward()MathHelper.Clamp(Speed + SpeedAcceleration, 0, MaxSpeed);
        }
        if (InputHandler.IsHeld(GameAction.MOVE_DOWN))
        {
            directions.Add(MoveDirection.DOWN);
            directions.Add(MoveDirection.BACKWARD);
        }
        if (InputHandler.IsHeld(GameAction.MOVE_LEFT))
        {
            directions.Add(MoveDirection.LEFT);
            // return MoveDirection.LEFT;
        }
        if (InputHandler.IsHeld(GameAction.MOVE_RIGHT))
        {
            directions.Add(MoveDirection.RIGHT);
        }

        // } else
        // {
        //     Speed = MathHelper.Clamp(Speed - SpeedAcceleration * 0.9f, 0, MaxSpeed);
        // }
        // if (InputHandler.IsHeld(GameAction.MOVE_DOWN))
        // {
        //     // direction += DirectionHelper.Get(MoveDirection.UP);
        //     Speed = MathHelper.Clamp(Speed - SpeedAcceleration * 1.5f, 0, MaxSpeed);
        //     // direction += DirectionHelper.Get(MoveDirection.DOWN);
        // }
        // if (InputHandler.IsHeld(GameAction.MOVE_LEFT))
        // {
        //     // direction += DirectionHelper.Get(MoveDirection.LEFT);
        // }
        // if (InputHandler.IsHeld(GameAction.MOVE_RIGHT))
        // {
        //     // direction += DirectionHelper.Get(MoveDirection.RIGHT);
        // }
        // direction = new Vector2(
        //     (float)Math.Cos(Rotation),
        //     (float)Math.Sin(Rotation)
        // );
        // Vector2 stick = InputHandler.GamePad.LeftStick;
        // if (stick != Vector2.Zero)
        // {
        //     direction += stick;
        // }
        return directions;
    }
    private void MakeRotation()
    {
        if (InputHandler.IsHeld(GameAction.MOVE_LEFT))
        {
            Rotation -= RotationAcceleration;
        }
        if (InputHandler.IsHeld(GameAction.MOVE_RIGHT))
        {
            Rotation += RotationAcceleration;
        }
    }
    private bool UpdateMoving()
    {
        return CurrentMovement.Direction != Vector2.Zero;
    }

    public void Update(GameTime gameTime)
    {
        // HandleMovement(gameTime);
        if (!CurrentMovement.CanMove)
        {
            return;
        }
        UpdateMoving();
        // CurrentMovement.Direction = CurrentMovement.HandleDirection(Rotation);
        // MoveDirection moveDirection = MakeMoveDirection();
        // CurrentMovement.Direction = CurrentMovement.HandleDirection(moveDirection);
        // CurrentMovement.HandleMovement(MoveDirection.UP, Speed, SpeedAcceleration, Rotation, RotationAcceleration, 0, MaxSpeed);
        CurrentMovement.HandleMovement(MakeMoveDirections(), CurrentMovement.Speed, SpeedAcceleration, Rotation, RotationAcceleration, 0, MaxSpeed);
    }
}
