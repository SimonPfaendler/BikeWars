using System.Collections.Generic;
using BikeWars.Content.components;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public interface IMoveable
{
    Vector2 Direction { get; set; }
    bool IsMoving { get; set; }
    bool CanMove { get; set; }
    float Speed {get; set;}
    float Rotation {get; set;}
    // void Update(GameTime gameTime);
    void HandleMovement(List<MoveDirection> moveDirections, float currentSpeed, float speedAcceleration, float currentRotation, float rotationAcceleration, float minSpeed, float maxSpeed); // User or other input
    Vector2 HandleDirection(List<MoveDirection> moveDirections);
    float HandleSpeed(List<MoveDirection> moveDirections, float currentSpeed, float acceleration, float minSpeed, float maxSpeed);
    float HandleRotation(List<MoveDirection> moveDirections);
}
