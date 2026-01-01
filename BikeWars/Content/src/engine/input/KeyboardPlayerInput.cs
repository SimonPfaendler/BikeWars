using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;

namespace BikeWars.Content.engine.input
{
    public class KeyboardPlayerInput : IPlayerInput
    {
        private Camera2D _camera;

        public bool IsAnalog => false;

        public KeyboardPlayerInput(Camera2D camera)
        {
            _camera = camera;
        }

        public Vector2 GetMovementDirection(IMoveable currentMovement)
        {
            Vector2 direction = Vector2.Zero;
            // Standard Keyboard Logic
            if (IsHeld(GameAction.MOVE_UP)) direction.Y -= 1;
            if (IsHeld(GameAction.MOVE_DOWN)) direction.Y += 1;
            if (IsHeld(GameAction.MOVE_LEFT)) direction.X -= 1;
            if (IsHeld(GameAction.MOVE_RIGHT)) direction.X += 1;

            return direction;
        }

        public Vector2 GetAimDirection(Vector2 currentPosition, Vector2 currentFacingDirection)
        {
             // Mouse Aiming Logic
            Vector2 mousePos = InputHandler.MakeMouseWorldPosByCamera(_camera);
            Vector2 diff = mousePos - currentPosition;
            if (diff != Vector2.Zero)
            {
                return Vector2.Normalize(diff);
            }
            return Vector2.Zero;
        }

        public bool IsPressed(GameAction action)
        {
            if (InputHandler.MouseMapping.TryGetValue(action, out var mouseButtons))
            {
                foreach (var mb in mouseButtons)
                {
                    if (InputHandler.Mouse.Pressed(mb))
                        return true;
                }
            }
            if (InputHandler.KeyMapping.TryGetValue(action, out var keys))
            {
                foreach (var key in keys)
                {
                    if (InputHandler.Keyboard.IsKeyPressed(key))
                        return true;
                }
            }
            return false;
        }

        public bool IsHeld(GameAction action)
        {
            if (InputHandler.MouseMapping.TryGetValue(action, out var mouseButtons))
            {
                foreach (var mb in mouseButtons)
                {
                    if (InputHandler.Mouse.Held(mb))
                        return true;
                }
            }
            if (InputHandler.KeyMapping.TryGetValue(action, out var keys))
            {
                foreach (var key in keys)
                {
                    if (InputHandler.Keyboard.IsKeyHeld(key))
                        return true;
                }
            }
            return false;
        }

        public void Update()
        {

        }
    }
}
