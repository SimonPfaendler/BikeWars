using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine
{
    // keeps track of temporary bell fear pulses and removes them once they expire.
    public static class BellFearSystem
    {
        private struct Pulse
        {
            public Vector2 Center;
            public float TimeLeft;
            public float Radius;
        }

        private static readonly List<Pulse> _pulses = new();
        
        // updates all active bell pulses and removes any that have expired.
        public static void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = _pulses.Count - 1; i >= 0; i--)
            {
                var p = _pulses[i];
                p.TimeLeft -= dt;

                if (p.TimeLeft <= 0f)
                {
                    _pulses.RemoveAt(i);
                    continue;
                }

                _pulses[i] = p;
            }
        }
        
    }
}