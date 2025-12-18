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
        // Gameplay
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
        SWITCH,
        DEBUG_HEAL,
        TECH_DEMO,
        DEBUG_HITBOXES,
        SWITCH_WEAPON,
        INVENTORY_1,
        INVENTORY_2,
        INVENTORY_3,
        INVENTORY_4,
        INVENTORY_5,
        MODE_SWITCH,
        
        // UI
        UI_UP,
        UI_DOWN,
        UI_LEFT,
        UI_RIGHT,
        UI_CONFIRM,
        UI_BACK,
        INVENTORY_NEXT,
        INVENTORY_PREV,
        INVENTORY_USE
    }

    public enum MouseButton
    {
        Left,
        Right,
        Middle
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
        public int ScrollDelta => _current.ScrollWheelValue - _previous.ScrollWheelValue;

        // Generic button handling
        public bool Held(MouseButton button) => button switch
        {
            MouseButton.Left   => _current.LeftButton   == ButtonState.Pressed,
            MouseButton.Right  => _current.RightButton  == ButtonState.Pressed,
            MouseButton.Middle => _current.MiddleButton == ButtonState.Pressed,
            _ => false
        };

        public bool Pressed(MouseButton button) => button switch
        {
            MouseButton.Left   => _current.LeftButton   == ButtonState.Pressed && _previous.LeftButton   == ButtonState.Released,
            MouseButton.Right  => _current.RightButton  == ButtonState.Pressed && _previous.RightButton  == ButtonState.Released,
            MouseButton.Middle => _current.MiddleButton == ButtonState.Pressed && _previous.MiddleButton == ButtonState.Released,
            _ => false
        };

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
            { GameAction.DEBUG_TOGGLE, new[] { Keys.D6 } },
            { GameAction.TOGGLE_CAMERA,new [] {Keys.C } },
            { GameAction.ESC, new[] {Keys.Escape } },
            { GameAction.SPRINT, new[] { Keys.LeftShift, Keys.RightShift } },
            { GameAction.PAUSE, new[] { Keys.Escape, Keys.P } },
            { GameAction.SHOOT, new Keys[0] },
            { GameAction.INTERACT, new[] {Keys.Q } },
            { GameAction.SWITCH, new[] {Keys.X } },
            { GameAction.DEBUG_HEAL, new[] {Keys.M } },
            { GameAction.TECH_DEMO, new[] {Keys.B}},
            { GameAction.DEBUG_HITBOXES, new[] {Keys.Z} },
            { GameAction.SWITCH_WEAPON, new[] {Keys.Tab} },
            { GameAction.INVENTORY_1, new[] { Keys.D1 } },
            { GameAction.INVENTORY_2, new[] { Keys.D2 } },
            { GameAction.INVENTORY_3, new[] { Keys.D3 } },
            { GameAction.INVENTORY_4, new[] { Keys.D4 } },
            { GameAction.INVENTORY_5, new[] { Keys.D5 } },
            { GameAction.UI_UP, new[] { Keys.W, Keys.Up } },
            { GameAction.UI_DOWN, new[] { Keys.S, Keys.Down } },
            { GameAction.UI_CONFIRM, new[] { Keys.Enter, Keys.Space } },
            { GameAction.MODE_SWITCH, new[] { Keys.J} },
            
        };
        public static Dictionary<GameAction, MouseButton[]> MouseMapping { get; } = new()
        {
            { GameAction.SHOOT, new[] { MouseButton.Left } }
        };

        public static Dictionary<GameAction, Buttons[]> GamepadMap { get; } = new()
        {   
            // UI
            { GameAction.UI_UP,    new[] { Buttons.DPadUp } },
            { GameAction.UI_DOWN,  new[] { Buttons.DPadDown } },
            { GameAction.UI_LEFT,  new[] { Buttons.DPadLeft } },
            { GameAction.UI_RIGHT, new[] { Buttons.DPadRight } },

            { GameAction.UI_CONFIRM, new[] { Buttons.A } },
            { GameAction.UI_BACK,    new[] { Buttons.B } },
            { GameAction.INVENTORY_PREV, new[] { Buttons.LeftShoulder } },
            { GameAction.INVENTORY_NEXT, new[] { Buttons.RightShoulder } },
            { GameAction.INVENTORY_USE,  new[] { Buttons.A } },
            
            // Gameplay
            { GameAction.SAVE, new[]  {Buttons.DPadUp} },
            { GameAction.LOAD, new[] {Buttons.DPadRight} },
            { GameAction.RESET, new[] {Buttons.DPadLeft} },
            //{ GameAction.DEBUG_TOGGLE, new[] {Buttons.DPadDown} },  no need for debug in controller we dont have much buttons
            { GameAction.TOGGLE_CAMERA,new [] {Buttons.DPadDown} },
            //{ GameAction.ESC, new[] { Buttons.B} },
            { GameAction.SPRINT, new[] { Buttons.LeftTrigger} },
            { GameAction.PAUSE, new[] { Buttons.Start} },
            { GameAction.SHOOT, new[] { Buttons.RightTrigger } },
            { GameAction.INTERACT, new[] { Buttons.A } },
            { GameAction.SWITCH_WEAPON, new[] {Buttons.Y} },
            { GameAction.SWITCH, new[] {Buttons.X } }
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
            if (MouseMapping.TryGetValue(action, out var mouseButtons))
            {
                foreach (var mb in mouseButtons)
                {
                    if (Mouse.Held(mb))
                        return true;
                }
            }
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

            if (MouseMapping.TryGetValue(action, out var mouseButtons))
            {
                foreach (var mb in mouseButtons)
                {
                    if (Mouse.Pressed(mb))
                        return true;
                }
            }

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
        public static Vector2 MakeMouseWorldPosByCamera(Camera2D camera)
        {
            // Invert the camera matrix to go from Screen -> World
            return Vector2.Transform(
                new Vector2(Mouse.X, Mouse.Y),
                Matrix.Invert(camera.GetTransform())
            );
        }
    }
}

