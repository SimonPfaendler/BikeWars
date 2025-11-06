using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BikeWars.Entities.Characters;

namespace BikeWars.Utilities
{
    public sealed class Debugger
    {
        private readonly SpriteFont _font;
        private readonly Player _player;
        private bool _isVisible = true;

        private KeyboardState _prevKb;




        public Debugger(SpriteFont font, Player player)
        {
            _font = font;
            _player = player;
        }

        public void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();

            if (kb.IsKeyDown(Keys.P) && !_prevKb.IsKeyDown(Keys.P))
            {
                _isVisible = !_isVisible;
            }
            _prevKb = kb;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible) return;


            // Display player position, velocity and bounds
            // You can add more debug information as needed e.g. collider info, Bounds, FPS, etc.

            string debugInfo = $"Player Position: {_player.Transform.Position}\n" +
                               $"Player Velocity: {_player.Speed}\n" + 
                               $"Player Bounds: {_player.Transform.Size}\n";

            
            spriteBatch.DrawString(_font, debugInfo, new Vector2(10, 10), Color.White);
            
        }
    }
}