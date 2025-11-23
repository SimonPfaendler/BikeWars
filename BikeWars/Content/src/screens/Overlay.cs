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
        
        // to draw the life lines and inventory
        private Texture2D _pixel;
        
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
            _pixel = new Texture2D(device, 1, 1);
            _pixel.SetData(new[] { Color.White });
            
            // calculation to put the timer in the middle upper part of the screen
            _timerPosition = new Vector2(device.Viewport.Width / 2f, 40f);
        }

        public void DrawOnScreen(SpriteBatch spriteBatch, GameTime frameTime)
        {
            // timer
            DrawTimer(spriteBatch, frameTime);
        }

        public void DrawOnWorld(SpriteBatch spriteBatch, Player player)
        {
            // player + bike lifelines
            DrawLifeLines( spriteBatch, player);
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
            
            // transform 30 min in seconds
            int totalSeconds = (int)Math.Max(0, (30 * 60 - effectiveTime.TotalSeconds));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            
            // draws the counter on screen
            spriteBatch.DrawString(_counterFont, $"Time: {minutes:00}:{seconds:00}", _timerPosition, Color.White);
        }

        private void DrawLifeLines(SpriteBatch spriteBatch, Player player)
        {
            // bar layout
            int barWidth  = 30;   
            int barHeight = 3;  
            int gap = 3; 
            int barPosition = 5;   // how far under the player will the bar be
            
            // get the player’s position from Transform
            Vector2 characterPos = player.Transform.Position;
            
            // center the lifelines
            float centerX = player.Transform.Position.X + player.Transform.Size.X / 2f + 5f;

            float bottomY = player.Transform.Position.Y + player.Transform.Size.Y;
            
            // get the position of the players lifeline
            Vector2 playerBarPos = new Vector2(
                centerX - barWidth / 2f,
                bottomY + barPosition
            );
            
            //player lifeline
            // background of the lifeline (black)
            Rectangle bgRect = new Rectangle((int)playerBarPos.X, (int)playerBarPos.Y, barWidth, barHeight);
            spriteBatch.Draw(_pixel, bgRect, Color.Black);
            
            // red "lost health" bar (no math yet)
            int redWidth = (int)(barWidth * 0.65f); 
            Rectangle redRect = new Rectangle((int)playerBarPos.X, (int)playerBarPos.Y, redWidth, barHeight);
            spriteBatch.Draw(_pixel, redRect, Color.Red);
            
            // bike lifeline
            Vector2 bikeBarPos = new Vector2(playerBarPos.X, playerBarPos.Y + barHeight + gap);
            Rectangle bikeRect = new Rectangle((int)bikeBarPos.X, (int)bikeBarPos.Y, barWidth, barHeight);
            spriteBatch.Draw(_pixel, bikeRect, Color.Red);
        }
    }
    
}