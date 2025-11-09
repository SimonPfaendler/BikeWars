using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;


// I think we need to improve this one. Because that is not enough imo. But ok first. Maybe we make it static. To use it everywhere we need it.
namespace BikeWars.Content.engine
{
    public enum GameAction
    {
        MOVE_LEFT, MOVE_RIGHT, MOVE_UP, MOVE_DOWN
    }

    public class InputHandler
    {
        // private KeyboardState _keyboardState { get; set; }
        public static readonly Dictionary<GameAction, Keys> KeyMapping = new()
        {
            {GameAction.MOVE_LEFT, Keys.A},
            {GameAction.MOVE_RIGHT, Keys.D},
            {GameAction.MOVE_UP, Keys.W},
            {GameAction.MOVE_DOWN, Keys.S},
        };

        public InputHandler()
        {
            // _keyboardState = Keyboard.GetState();
        }

        // public KeyboardState KeyboardState { get => _keyboardState; }
        public bool PressingAction(GameAction action)
        {
            return Keyboard.GetState().IsKeyDown(KeyMapping[action]);
        }
    }
}
