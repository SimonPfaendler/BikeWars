using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Entities.Characters;


namespace BikeWars.Content.src.screens.Overlay
{
    public class Overlay
    {
        // to draw the counter
        private SpriteFont _counterFont;
        // timer position
        private Vector2 _timerPosition;
        
        private bool _isPaused = false;
        private TimeSpan _pausedTime = TimeSpan.Zero;
        private TimeSpan _timeOffset = TimeSpan.Zero;
        
        public void SetPaused(bool paused, GameTime gameTime)
        {
            if (paused && !_isPaused)
            {
                _pausedTime = gameTime.TotalGameTime;
            }
            else if (!paused && _isPaused)
            {
                _timeOffset += gameTime.TotalGameTime - _pausedTime;
            }
            _isPaused = paused;
        }
        
        public Overlay(SpriteFont counterFont, GraphicsDevice device)
        {
            _counterFont = counterFont;
            // calculation to put the timer in the middle upper part of the screen
            _timerPosition = new Vector2(device.Viewport.Width / 2f, 40f);
        }

        public void DrawOnScreen(SpriteBatch spriteBatch, GameTime frameTime)
        {
            // timer
            DrawTimer(spriteBatch, frameTime);
        }
        
        // draw the timer
        private void DrawTimer(SpriteBatch spriteBatch, GameTime frameTime)
        {
            TimeSpan effectiveTime;
        
            if (_isPaused)
            {
                effectiveTime = _pausedTime - _timeOffset;
            }
            else
            {
                effectiveTime = frameTime.TotalGameTime - _timeOffset;
            }
            
            // transform 15 min in seconds
            int totalSeconds = (int)Math.Max(0, (15 * 60 - effectiveTime.TotalSeconds));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            
            // draws the counter on screen
            spriteBatch.DrawString(_counterFont, $"Time: {minutes:00}:{seconds:00}", _timerPosition, Color.White);
        }
    }
    
}