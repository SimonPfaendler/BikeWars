using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace BikeWars.Content.managers
{
    /// <summary>
    /// Verwaltet die Erstellung/Bereitstellung aller SpriteAnimation-Objekte für die Spielentitäten.
    /// - zentralisiert das Laden des Texture Atlas
    /// - Instanziierung der vollständigen Animationsobjekte
    /// - ruft die gespeicherten Frame-Koordinaten ab
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

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Point size, float rotation)
        {
            // aktueller Frame aus der Liste
            Rectangle source = _frames[_frameIndex];

            Rectangle dest = new Rectangle(
                (int)MathF.Round(position.X),
                (int)MathF.Round(position.Y),
                size.X,
                size.Y
            );

            spriteBatch.Draw(_sheet, dest, source, Color.White, rotation: rotation, new Vector2(source.Width / 2f, source.Height / 2f), SpriteEffects.None, layerDepth:0f);
        }
    }
}