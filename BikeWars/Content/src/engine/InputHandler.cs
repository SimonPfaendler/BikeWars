using System.Collections.Generic;
using System.Data;
using Microsoft.Xna.Framework.Input;

// ============================================================
// InputHandler.cs
// ------------------------------------------------------------
// Description:
//    Centralized input management for all keyboard and mouse input.
//    Provides easy-to-use methods for querying game actions, e.g.:
//
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
        DEBUG_TOGGLE
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
    // TODO: Add MouseInfo class for mouse input handling
    // public class MouseInfo

    public static class InputHandler
    {
        public static readonly KeyboardInfo Keyboard = new();
        // public static readonly MouseInfo Mouse = new();

        public static readonly Dictionary<GameAction, Keys> KeyMapping = new()
        {
            { GameAction.MOVE_LEFT, Keys.A },
            { GameAction.MOVE_RIGHT, Keys.D },
            { GameAction.MOVE_UP, Keys.W },
            { GameAction.MOVE_DOWN, Keys.S },
            { GameAction.SAVE, Keys.T },
            { GameAction.LOAD, Keys.L },
            { GameAction.RESET, Keys.R },
            { GameAction.DEBUG_TOGGLE, Keys.P }
        };

        public static void Update()
        {
            Keyboard.Update();
            // Mouse.Update();
        }

        public static bool IsHeld(GameAction action)
        {
            return Keyboard.IsKeyHeld(KeyMapping[action]);
        }

        public static bool IsPressed(GameAction action)
        {     
            return Keyboard.IsKeyPressed(KeyMapping[action]);
        }

        public static bool IsReleased(GameAction action)
        {
            return Keyboard.IsKeyReleased(KeyMapping[action]);
        }
    }
}

