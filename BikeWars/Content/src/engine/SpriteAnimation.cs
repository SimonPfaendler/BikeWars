using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.engine
{
    /// <summary>
    /// Handles sprite animation playback from a texture atlas.
    /// </summary>
    public class SpriteAnimation
    {
        private readonly Texture2D _sheet;
        private readonly List<Rectangle> _frames;
        private readonly float _secondsPerFrame;
        private int _frameIndex;
        private float _timer;

        public SpriteAnimation(Texture2D sheet, List<Rectangle> frames, float secondsPerFrame)
        {
            _sheet = sheet;
            _frames = frames;
            _secondsPerFrame = secondsPerFrame;
            _frameIndex = 0;
            _timer = 0f;
        }

        public void Update(GameTime gameTime, bool isMoving)
        {
            if (isMoving)
            {
                _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_timer >= _secondsPerFrame)
                {
                    _timer -= _secondsPerFrame;
                    _frameIndex = (_frameIndex + 1) % _frames.Count;
                }
            }
            else
            {
                _frameIndex = 0;
                _timer = 0f;
            }
        }

        public Rectangle GetCurrentFrame()
        {
            return _frames[_frameIndex];
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Point size, float rotation, Color? color = null, SpriteEffects effects = SpriteEffects.None)
        {
            Draw(spriteBatch, position, size, rotation, Vector2.One, color, effects);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Point size, float rotation, Vector2 scale, Color? color = null, SpriteEffects effects = SpriteEffects.None)
        {
            Rectangle source = _frames[_frameIndex];
            
            // Apply scale to dimensions
            float width = size.X * scale.X;
            float height = size.Y * scale.Y;
            
            Rectangle dest = new Rectangle(
                (int)MathF.Round(position.X),
                (int)MathF.Round(position.Y),
                (int)width,
                (int)height
            );
            
            // Adjust origin to center for proper scaling
            Vector2 origin = new Vector2(source.Width / 2f, source.Height / 2f);
            
            spriteBatch.Draw(_sheet, dest, source, color ?? Color.White, rotation: rotation, origin, effects, layerDepth:0f);
        }

        public SpriteAnimation Clone()
        {
            return new SpriteAnimation(_sheet, _frames, _secondsPerFrame);
        }
    }
}
