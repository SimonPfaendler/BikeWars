using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

// ============================================================
// InputHandler.cs
// ------------------------------------------------------------
// Description:
//    Centralized input management for all keyboard and mouse input.
//    Provides easy-to-use methods for querying game actions, e.g.:
//
//        InputHandler.IsHeld(GameAction.MOVE_LEFT)
//        InputHandler.IsPressed(GameAction.SAVE)
//
//    This system encapsulates MonoGame's raw Keyboard and Mouse state logic,
//    removing the need for direct "IsKeyDown(Keys...)" calls throughout the code.
//

namespace BikeWars.Content.engine
{
    public enum GameAction
    {
        MOVE_LEFT,
        MOVE_RIGHT,
        MOVE_UP,
        MOVE_DOWN,
        SAVE,
        LOAD,
        RESET,
        DEBUG_TOGGLE,
        TOGGLE_CAMERA,
        ESC,
        SPRINT,
        PAUSE,
        SHOOT,
        INTERACT,
        SWITCH
    }

    public class KeyboardInfo
    {
        private KeyboardState _current;
        private KeyboardState _previous;

        public void Update()
        {
            _previous = _current;
            _current = Keyboard.GetState();
        }

        public bool IsKeyHeld(Keys key)
        {
            return _current.IsKeyDown(key);
        }

        public bool IsKeyPressed(Keys key)
        {
            return _current.IsKeyDown(key) && !_previous.IsKeyDown(key);
        }

        public bool IsKeyReleased(Keys key)
        {
            return !_current.IsKeyDown(key) && _previous.IsKeyDown(key);
        }
    }


    public class MouseInfo
    {
        private MouseState _current;
        private MouseState _previous;

        public Point Position => _current.Position;
        public int X => _current.X;
        public int Y => _current.Y;

        public Point Delta => _current.Position - _previous.Position;

        public bool LeftHeld => _current.LeftButton == ButtonState.Pressed;
        public bool LeftPressed => _current.LeftButton == ButtonState.Pressed && _previous.LeftButton == ButtonState.Released;

        public bool RightHeld => _current.RightButton == ButtonState.Pressed;
        public bool RightPressed => _current.RightButton == ButtonState.Pressed && _previous.RightButton == ButtonState.Released;

        public int ScrollDelta => _current.ScrollWheelValue - _previous.ScrollWheelValue;


        public void Update()
        {
            _previous = _current;
            _current = Mouse.GetState();
        }


    }

    public class GamePadInfo
    {
        private GamePadState _current;
        private GamePadState _previous;

        private readonly PlayerIndex _playerIndex;
        private const float DeadZone = 0.25f;

        public GamePadInfo(PlayerIndex playerIndex = PlayerIndex.One)
        {
            _playerIndex = playerIndex;
        }

        public void Update()
        {
            _previous = _current;
            _current = GamePad.GetState(_playerIndex);
        }

        public bool Connected => _current.IsConnected;

        public Vector2 LeftStick
        {
            get
            {
                var v = _current.ThumbSticks.Left;
                v.Y *= -1;

                if (v.Length() < DeadZone)
                    return Vector2.Zero;
                return v;
            }
        }
        public Vector2 RightStick => _current.ThumbSticks.Right;

        public bool Held(Buttons button) => _current.IsButtonDown(button);
        public bool Pressed(Buttons button) => _current.IsButtonDown(button) && !_previous.IsButtonDown(button);


    }

    public static class InputHandler
    {
        public static readonly KeyboardInfo Keyboard = new();
        public static readonly MouseInfo Mouse = new();
        public static readonly GamePadInfo GamePad = new();


        // Keyboard Mapping
        public static Dictionary<GameAction, Keys[]> KeyMapping { get; } = new()
        {
            { GameAction.MOVE_LEFT,new[] { Keys.A, Keys.Left } },
            { GameAction.MOVE_RIGHT, new[] { Keys.D, Keys.Right } },
            { GameAction.MOVE_UP, new[] { Keys.W, Keys.Up } },
            { GameAction.MOVE_DOWN, new[] { Keys.S, Keys.Down } },
            { GameAction.SAVE, new[]  { Keys.T } },
            { GameAction.LOAD, new[] { Keys.L } },
            { GameAction.RESET, new[] { Keys.R } },
            { GameAction.DEBUG_TOGGLE, new[] { Keys.P } },
            { GameAction.TOGGLE_CAMERA,new [] {Keys.C } },
            { GameAction.ESC, new[] {Keys.Escape } },
            { GameAction.SPRINT, new[] { Keys.LeftShift, Keys.RightShift } },
            { GameAction.PAUSE, new[] { Keys.Escape, Keys.P } },
            {GameAction.SHOOT, new[] { Keys.G } },
            { GameAction.INTERACT, new[] {Keys.Q } },
            { GameAction.SWITCH, new[] {Keys.X } }
        };

        public static Dictionary<GameAction, Buttons[]> GamepadMap { get; } = new()
        {

            { GameAction.SAVE, new[]  {Buttons.DPadUp} },
            { GameAction.LOAD, new[] {Buttons.DPadRight} },
            { GameAction.RESET, new[] {Buttons.DPadLeft} },
            { GameAction.DEBUG_TOGGLE, new[] {Buttons.DPadDown} },
            { GameAction.TOGGLE_CAMERA,new [] {Buttons.A} },
            { GameAction.ESC, new[] { Buttons.B} },
            { GameAction.SPRINT, new[] { Buttons.LeftTrigger} },
            { GameAction.PAUSE, new[] { Buttons.Start} },
            {GameAction.SHOOT, new[] { Buttons.RightTrigger } },
            { GameAction.INTERACT, new[] { Buttons.X } }
            /* INTERACT should be A not X, but X is already used and I m not sure for what.
             Should be fixed later*/

        };

        public static void Update()
        {
            Keyboard.Update();
            Mouse.Update();
            GamePad.Update();
        }

        public static bool IsHeld(GameAction action)
        {

            if (KeyMapping.TryGetValue(action, out var keys))
            {
                foreach (var key in keys)
                {
                    if (Keyboard.IsKeyHeld(key))
                        return true;
                }

            }
            if (GamepadMap.TryGetValue(action, out var buttons))
            {
                foreach (var button in buttons)
                {
                    if (GamePad.Held(button))
                        return true;
                }

            }
            return false;
        }

        public static bool IsPressed(GameAction action)
        {
            if (KeyMapping.TryGetValue(action, out var keys))
            {
                foreach (var key in keys)
                {
                    if (Keyboard.IsKeyPressed(key))
                        return true;
                }

            }
            if (GamepadMap.TryGetValue(action, out var buttons))
            {
                foreach (var button in buttons)
                {
                    if (GamePad.Pressed(button))
                        return true;
                }
            }
            return false;
        }

        public static bool IsReleased(GameAction action)
        {
            if (KeyMapping.TryGetValue(action, out var keys))
            {
                foreach (var key in keys)
                {
                    if (Keyboard.IsKeyReleased(key))
                        return true;
                }

            }
            return false;
        }
    }
}

