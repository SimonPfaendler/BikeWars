using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace BikeWars.Content.engine
{
    public class SoundHandler
    {
        // private KeyboardState _keyboardState { get; set; }
        public static readonly Dictionary<Action, Keys> KeyMapping = new()
        {
            {Action.MOVE_LEFT, Keys.A},
            {Action.MOVE_RIGHT, Keys.D},
            {Action.MOVE_UP, Keys.W},
            {Action.MOVE_DOWN, Keys.S},
        };

        public SoundHandler()
        {
            // _keyboardState = Keyboard.GetState();
        }

        // public KeyboardState KeyboardState { get => _keyboardState; }
        
    }
}
