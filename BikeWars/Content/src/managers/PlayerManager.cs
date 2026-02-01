using System;
using System.Collections.Generic;
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
        private static readonly Random _rng = new();

        // Define possible starting points
        private static readonly List<Vector2> _startPositions = new()
        {
            new Vector2(3511, 1313),
            new Vector2(4799, 7618),
            new Vector2(8576, 8758),
            new Vector2(9045, 5198),
            new Vector2(9960, 2091),
            new Vector2(5279, 4816),
            new Vector2(1390, 9856)
        };

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

            // Pick random starting spots
            Vector2 p1Start = PickStartPosition();
            Vector2 p2Start = p1Start + new Vector2(200, 0);

            // Player 1 - Keyboard
            var inputP1 = new KeyboardPlayerInput(Camera);
            Player1 = new Player(p1Start, 15, new Point(32,32), audioService, inputP1, isTechDemo);

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
            Player2 = new Player(p2Start, 12, new Point(32, 32), audioService, inputP2, isTechDemo, "Character2");

            if (isTechDemo)
            {
                Player2.IsGodMode = true;
            }
        }

        public void ReinitializeCamera(Viewport viewport, Rectangle worldBounds)
        {
            Camera = new Camera2D(
                viewport.Width,
                viewport.Height,
                worldBounds
            );

            if (Player1 != null)
            {
                Player1.SetInput(new KeyboardPlayerInput(Camera));
            }

            // Player 2 uses controller; no input remap needed, but keep reference consistent
            if (Player2 != null)
            {
                Player2.UpdateCollider();
            }
        }

        private static Vector2 PickStartPosition()
        {
            if (_startPositions.Count == 0)
            {
                return Vector2.Zero;
            }
            return _startPositions[_rng.Next(_startPositions.Count)];
        }
    }
}
