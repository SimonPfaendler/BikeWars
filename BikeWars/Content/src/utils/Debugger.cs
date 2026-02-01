using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using System.Collections.Generic;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.components;
using System;
// ============================================================
// Debugger.cs
//
// Description:
// A simple in-game debugger utility to display debug information such as player position,player velocity, and bounds.
// It can be toggled on and off with DEBUG_TOGGLE(P) action.
// ============================================================
namespace BikeWars.Utilities
{
    public sealed class Debugger
    {
        private readonly Player _player;
        private float _fps;
        private bool _isVisible = true;

        private List<CharacterBase> _characters;

        public Debugger(Player player)
        {
            _player = player;
            _characters = new List<CharacterBase>();
        }

        public void Update(GameTime gameTime, List<CharacterBase> characters)
        {
            if (InputHandler.IsPressed(GameAction.DEBUG_TOGGLE))
            {
                _isVisible = !_isVisible;
            }
            _characters = characters;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt > 0f)
            {
                float instantaneousFps = 1f / dt;
                _fps = _fps <= 0f ? instantaneousFps : MathHelper.Lerp(_fps, instantaneousFps, 0.1f); // light smoothing to avoid jitter
            }
        }
        public void Draw(SpriteBatch spriteBatch, Viewport viewport)
        {
            if (!_isVisible) return;

            // Display player position, velocity and bounds, Sprint status
            // You can add more debug information as needed e.g. collider info, Bounds, FPS, etc.
            string debugInfo = $"FPS: {(int)_fps}\n" +
                               $"Characters in Game: {_characters.Count}\n" +
                               $"Player Position: X: {(int)_player.Transform.Position.X} Y: {(int)_player.Transform.Position.Y}\n" +
                               $"Player Velocity: {_player.CurrentSpeed * _player.TerrainSpeedMultiplier}\n" +
                               $"Player Bounds: {_player.Transform.Size}\n" +
                               $"Player Sprint Cooldown: {(int)_player.CooldownTimer()}\n" +
                               $"Player Health: {_player.Attributes.Health}" +
                               (_player.CurrentBike != null ? $"\nBike Health: {_player.CurrentBike.Attributes.Health}" : "");


            Vector2 textSize = UIAssets.DefaultFont.MeasureString(debugInfo);

            const float padding = 16f;
            float x = (viewport.Width - textSize.X) * 0.5f;
            float y = viewport.Height - textSize.Y - padding;

            // clamp so text stays on screen for very small viewports
            x = MathF.Max(padding, x);
            y = MathF.Max(padding, y);

            spriteBatch.DrawString(UIAssets.DefaultFont, debugInfo, new Vector2(x, y), Color.White);
        }
    }
}