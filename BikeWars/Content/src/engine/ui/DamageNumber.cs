using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.engine.ui
{
    public class DamageNumber
    {
        public Vector2 Position { get; private set; }
        public int Value { get; private set; }
        public float Lifetime { get; private set; }
        public bool IsExpired { get; private set; } = false;
        public bool IsCrit { get; private set; }
        public Vector2 Velocity { get; private set; }
        private Color _tint;
        private static Random _rnd = new Random();

        private const float MaxLifetime = 0.8f;

        public DamageNumber(Vector2 position, int value, bool isCrit, Vector2 velocity)
        {
            Position = position;
            Value = value;
            Lifetime = MaxLifetime;
            IsCrit = isCrit;
            Velocity = velocity;

            // randomize color
            if (IsCrit)
            {
                _tint = Color.Yellow;
            }
            else
            {
                // Simple color variation
                int variant = _rnd.Next(0, 3);
                switch(variant)
                {
                    case 0: _tint = Color.Orange; break;
                    case 1: _tint = Color.OrangeRed; break;
                    case 2: _tint = Color.DarkOrange; break;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Apply Velocity
            Position += Velocity * dt;
            
            // Optional: Friction or Gravity?
            // Let's add slight "friction" so they pop out fast and slow down
            Velocity *= 0.95f; 

            // Decay
            Lifetime -= dt;
            if (Lifetime <= 0f)
            {
                IsExpired = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (font == null) return;

            float alpha = Lifetime / MaxLifetime;
            Color color = _tint * alpha;

            float scale = IsCrit ? 2.5f : 1.75f;

            string text = Value.ToString();
            Vector2 origin = font.MeasureString(text) / 2f;

            // Draw with simple drop shadow/outline for readability
            spriteBatch.DrawString(font, text, Position + new Vector2(2, 2), Color.Black * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, Position, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
