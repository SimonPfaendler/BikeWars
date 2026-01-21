using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
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
        private readonly SpriteFont _font;
        private readonly Player _player;
        private float _fps;
        private bool _isVisible = true;

        public Debugger(SpriteFont font, Player player)
        {
            _font = font;
            _player = player;
        }

        public void Update(GameTime gameTime)
        {
            if (InputHandler.IsPressed(GameAction.DEBUG_TOGGLE))
            {
                _isVisible = !_isVisible;
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (dt > 0f)
            {
                float instantaneousFps = 1f / dt;
                _fps = _fps <= 0f ? instantaneousFps : MathHelper.Lerp(_fps, instantaneousFps, 0.1f); // light smoothing to avoid jitter
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible) return;

            // Display player position, velocity and bounds, Sprint status
            // You can add more debug information as needed e.g. collider info, Bounds, FPS, etc.
            string debugInfo = $"FPS: {(int)_fps}\n" +
                               $"Player Position: X: {(int)_player.Transform.Position.X} Y: {(int)_player.Transform.Position.Y}\n" +
                               $"Player Velocity: {_player.CurrentSpeed * _player.TerrainSpeedMultiplier}\n" +
                               $"Player Bounds: {_player.Transform.Size}\n" +
                               $"Player Sprint Cooldown: {(int)_player.CooldownTimer()}\n" +
                               $"Player Health: {_player.Attributes.Health}" +
                               (_player.CurrentBike != null ? $"\nBike Health: {_player.CurrentBike.Attributes.Health}" : "");


            spriteBatch.DrawString(_font, debugInfo, new Vector2(10, 600), Color.White);
        }
    }
}