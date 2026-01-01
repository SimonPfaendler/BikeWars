using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;

namespace BikeWars.Content.engine.input
{
    public class GamepadPlayerInput : IPlayerInput
    {
        private PlayerIndex _playerIndex;
        
        
        public bool IsAnalog => true;

        public GamepadPlayerInput(PlayerIndex playerIndex)
        {
            _playerIndex = playerIndex;
        }

        public void Update()
        {

        }

        private GamePadInfo GetPad() => InputHandler.GetGamePad(_playerIndex);

        public Vector2 GetMovementDirection(IMoveable currentMovement)
        {
             var pad = GetPad();
             return pad.LeftStick;
        }

        public Vector2 GetAimDirection(Vector2 currentPosition, Vector2 currentFacingDirection)
        {
            var pad = GetPad();
            var rightStick = pad.RightStick;
            rightStick.Y *= -1;

            
            if (rightStick.Length() > 0.2f)
            {
                return Vector2.Normalize(rightStick);
            }
            return Vector2.Zero;
        }

        public bool IsPressed(GameAction action)
        {
            Buttons[] buttons;
            var pad = GetPad();
            
            if (InputHandler.GamepadMap.TryGetValue(action, out buttons))
            {
                 foreach (var b in buttons)
                 {
                     if (pad.Pressed(b))
                        return true;
                 }
            }
            return false;
        }

        public bool IsHeld(GameAction action)
        {
            Buttons[] buttons;
            var pad = GetPad();
            
            if (InputHandler.GamepadMap.TryGetValue(action, out buttons))
            {
                 foreach (var b in buttons)
                 {
                     if (pad.Held(b))
                        return true;
                 }
            }
            return false;
        }
    }
}
