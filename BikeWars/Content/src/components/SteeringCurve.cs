using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.components
{
    /// <summary>
    /// calculates a steering multiplier based on the current speed
    ///
    /// HOW TO TUNE: (balancing)
    /// Adjust the Vector2 keypoints in the constructor.
    /// X = Speed (0 to ~400)
    /// Y = Steering Multiplier (1.0 = Full Steering, 0.2 = Stiff Steering)
    /// </summary>
    public class SteeringCurve
    {
        private List<Vector2> _keypoints;
        private int _lastSegmentIndex = 0;
        private Vector2 _virtualStart;
        private Vector2 _virtualEnd;

        // max speed should be around 385 with the boost, 220 on Street
        // here you can change the steering:
        public SteeringCurve()
        {
            SetKeypoints(new List<Vector2>
            {
                new Vector2(0f, 1.0f),    // 0 speed: Full steering
                new Vector2(60f, 0.8f),   // Moderate speed: Quick drop-off
                new Vector2(150f, 0.6f),  // High speed: Very stiff
                new Vector2(221f, 0.2f)  // Boost: Extremely stiff
            });
        }

        public SteeringCurve(List<Vector2> keypoints)
        {
            SetKeypoints(keypoints);
        }

        public void SetKeypoints(List<Vector2> keypoints)
        {
            _keypoints = keypoints;
            if (_keypoints == null || _keypoints.Count < 2)
            {
                // Fallback
                _keypoints = new List<Vector2> { new Vector2(0, 1), new Vector2(1000, 0.2f) };
            }

            // sort by speed (X)
            _keypoints.Sort((a, b) => a.X.CompareTo(b.X));

            // Precompute virtual points
            _virtualStart = _keypoints[0] - (_keypoints[1] - _keypoints[0]);
            _virtualEnd = _keypoints[^1] + (_keypoints[^1] - _keypoints[^2]);

            _lastSegmentIndex = 0;
        }

        public float Evaluate(float speed)
        {
            if (speed <= _keypoints[0].X) return _keypoints[0].Y;
            if (speed >= _keypoints[^1].X) return _keypoints[^1].Y;

            int i = _lastSegmentIndex;

            // Check if we need to move forward or backward
            if (speed > _keypoints[i + 1].X)
            {
                // Search forward
                while (i < _keypoints.Count - 2 && speed > _keypoints[i + 1].X)
                {
                    i++;
                }
            }
            else if (speed < _keypoints[i].X)
            {
                // Search backward
                while (i > 0 && speed < _keypoints[i].X)
                {
                    i--;
                }
            }

            _lastSegmentIndex = i;

            // Catmull-Rom
            // P0, P1, P2, P3
            Vector2 p1 = _keypoints[i];
            Vector2 p2 = _keypoints[i + 1];

            Vector2 p0 = (i == 0) ? _virtualStart : _keypoints[i - 1];
            Vector2 p3 = (i == _keypoints.Count - 2) ? _virtualEnd : _keypoints[i + 2];

            // T = fraction between P1 and P2
            float t = (speed - p1.X) / (p2.X - p1.X);

            float result = Vector2.CatmullRom(p0, p1, p2, p3, t).Y;

            // Clamp
            return MathHelper.Clamp(result, 0f, 1.5f);
        }
    }
}
