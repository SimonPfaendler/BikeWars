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
        private float _scaleAnim = 0f;

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
                _tint = Color.Red;
            }
            else
            {
                // Simple color variation
                int variant = _rnd.Next(0, 4);
                switch(variant)
                {
                    case 0: _tint = Color.Orange; break;
                    case 1: _tint = Color.Yellow; break;
                    case 2: _tint = Color.DarkOrange; break;
                    case 3: _tint = Color.Gold; break;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Pop-in animation
            _scaleAnim = MathHelper.Lerp(_scaleAnim, 2.0f, 20f * dt);
            if (_scaleAnim > 1.2f && Lifetime < MaxLifetime - 0.05f)
            {
                _scaleAnim = MathHelper.Lerp(_scaleAnim, 1.0f, 10f * dt); // Elastic snap back
            }

            // Apply Velocity
            Position += Velocity * dt;
            Velocity *= 0.98f; 

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

            float baseScale = IsCrit ? 2.5f : 1.75f;
            
            // Calculate Squash and Stretch

            float stretchFactor = _scaleAnim - 1.0f; 
            float scaleX = _scaleAnim * (1.0f - stretchFactor * 0.5f);
            float scaleY = _scaleAnim * (1.0f + stretchFactor * 0.5f);

            Vector2 finalScale = new Vector2(scaleX, scaleY) * baseScale;

            string text = Value.ToString();
            Vector2 origin = font.MeasureString(text) / 2f;

            // Draw with simple drop shadow/outline for readability
            spriteBatch.DrawString(font, text, Position + new Vector2(2, 2), Color.Black * alpha, 0f, origin, finalScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, Position, color, 0f, origin, finalScale, SpriteEffects.None, 0f);
        }
    }
}
