using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;


// I think we need to improve this one. Because that is not enough imo. But ok first. Maybe we make it static. To use it everywhere we need it.
namespace BikeWars.Content.engine
{
    public enum Action
    {
        MOVE_LEFT, MOVE_RIGHT, MOVE_UP, MOVE_DOWN, SAVE, RESET, LOAD
    }

    public class InputHandler
    {
        // private KeyboardState _keyboardState { get; set; }
        public static readonly Dictionary<Action, Keys> KeyMapping = new()
        {
            {Action.MOVE_LEFT, Keys.A},
            {Action.MOVE_RIGHT, Keys.D},
            {Action.MOVE_UP, Keys.W},
            {Action.MOVE_DOWN, Keys.S},
            {Action.SAVE, Keys.T},
            {Action.LOAD, Keys.L},
            {Action.RESET, Keys.R},
        };

        public InputHandler()
        {
            // _keyboardState = Keyboard.GetState();
        }

        // public KeyboardState KeyboardState { get => _keyboardState; }
        public bool PressingAction(Action action)
        {
            return Keyboard.GetState().IsKeyDown(KeyMapping[action]);
        }
    }
}
