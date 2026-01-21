using Microsoft.Xna.Framework;
using BikeWars.Content.engine;
using BikeWars.Content.engine.input;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.managers
{
    public enum GameMode
    {
        SinglePlayer,
        MultiPlayer
    }

    public class PlayerManager
    {
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public Camera2D Camera { get; private set; }

        public PlayerManager(Viewport viewport, GameMode mode, Rectangle worldBounds, AudioService audioService, bool isTechDemo)
        {
            Camera = new Camera2D(
                viewport.Width,
                viewport.Height,
                worldBounds
            );

            // Player 1 - Keyboard
            var inputP1 = new KeyboardPlayerInput(Camera);
            Player1 = new Player(new Vector2(worldBounds.Width / 2, worldBounds.Height / 2), new Point(24, 24), new Point(32,32), audioService, inputP1);

            if (isTechDemo)
            {
                Player1.IsGodMode = true;
            }

            // Player 2 - Gamepad
            if (mode != GameMode.MultiPlayer)
            {
                return;
            }
            // Assign Player 2 to the second controller. Player 1 starts on Keyboard but can switch to Pad 1.
            var inputP2 = new GamepadPlayerInput(PlayerIndex.Two);
            Player2 = new Player(new Vector2(worldBounds.Width / 2 + 50, worldBounds.Height / 2), new Point(24, 24), new Point(32, 32), audioService, inputP2, "Character2");

            if (isTechDemo)
            {
                Player2.IsGodMode = true;
            }
        }
    }
}
