using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BikeWars.Content.engine.interfaces;
using System;
using BikeWars.Content.components;

namespace BikeWars.Content.engine.input
{
    public class GamepadPlayerInput : IPlayerInput
    {
        private PlayerIndex _playerIndex;
        private GamePadState _currentState;
        private GamePadState _previousState;

        public bool IsAnalog => true;

        public GamepadPlayerInput(PlayerIndex playerIndex)
        {
            _playerIndex = playerIndex;
        }

        public void Update()
        {
            _previousState = _currentState;
            _currentState = GamePad.GetState(_playerIndex);
        }

        public Vector2 GetMovementDirection(IMoveable currentMovement)
        {
             var leftStick = _currentState.ThumbSticks.Left;
             leftStick.Y *= -1;
             if (leftStick.Length() < 0.2f) return Vector2.Zero;
             
             return leftStick;
        }

        public Vector2 GetAimDirection(Vector2 currentPosition, Vector2 currentFacingDirection)
        {
            Vector2 rightStick = _currentState.ThumbSticks.Right;
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
            if (InputHandler.GamepadMap.TryGetValue(action, out buttons))
            {
                 foreach (var b in buttons)
                 {
                     if (_currentState.IsButtonDown(b) && !_previousState.IsButtonDown(b))
                        return true;
                 }
            }
            return false;
        }

        public bool IsHeld(GameAction action)
        {
            Buttons[] buttons;
            if (InputHandler.GamepadMap.TryGetValue(action, out buttons))
            {
                 foreach (var b in buttons)
                 {
                     if (_currentState.IsButtonDown(b))
                        return true;
                 }
            }
            return false;
        }
    }
}
