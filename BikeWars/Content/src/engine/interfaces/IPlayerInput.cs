using Microsoft.Xna.Framework;
using BikeWars.Content.engine;

namespace BikeWars.Content.engine.interfaces
{
    public interface IPlayerInput
    {
        bool IsAnalog { get; }
        Vector2 GetMovementDirection(IMoveable currentMovement);
        Vector2 GetAimDirection(Vector2 currentPosition, Vector2 currentFacingDirection);
        bool IsPressed(GameAction action);
        bool IsHeld(GameAction action);
        void Update();
    }
}
