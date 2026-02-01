using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using BikeWars.Content.engine.interfaces;
using System.Collections.Generic;

namespace BikeWars.Content.entities.npcharacters
{
    public class Car
    {
        public int Hp = 5;
        
        private readonly CollisionManager _collision;
        private readonly Random _rng;
        
        public Transform Transform { get; private set; }
        public BoxCollider Collider { get; private set; }
        
        private Point _dir;
        
        public bool IsDead { get; private set; }
        
        private const float Speed = 100f;              
        private const float DespawnGrace = 4f;
        private float _offRoadTimer = 0f;
        
        private float _deadEndTimer = 0f;
        private const float DeadEndGrace = 0.15f;
        
        private const float LookAheadDistance = 80f; 
        private const float MinFollowDistance = 50f;
        const float MinAvoidSpeed = 20f;
        private float _currentSpeed = Speed; 
        
        private float _aheadCheckTimer = 0f;
        private const float AheadCheckInterval = 0.5f;
        
        private readonly List<ICollider> _nearbyCars = new(32);
        private readonly BoxCollider _checkBox = new BoxCollider(Vector2.Zero, 32, 32, CollisionLayer.CAR, null);
        
        // prevents the car from choosing a new direction multiple times in the same tile
        private Point _lastDecisionTile = new Point(int.MinValue, int.MinValue);
        
        public Car(Vector2 startWorldCenter, Point startDir, CollisionManager collision, Random rng)
        {
            _collision = collision;
            _rng = rng;

            Transform = new Transform(startWorldCenter, new Point(32, 32));
            
            Collider = new BoxCollider(
                Transform.Position - new Vector2(16, 16),
                32,
                32,
                CollisionLayer.CAR,
                this
            );

            _dir = startDir;
            
            // immediately snap to lane center on spawn
            SnapToLaneCenter();
            UpdateCollider();
        }

        public void Update(GameTime gameTime)
        {
            if (IsDead) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            ChooseDirectionIfNeeded(dt);
            if (IsDead) return;          // IMPORTANT: ChooseDirection can set IsDead

            SnapToLaneCenter();
            
            _aheadCheckTimer += dt;
            if (_aheadCheckTimer >= AheadCheckInterval)
            {
                _aheadCheckTimer = 0f;
                CheckForCarsAhead();
            }

            Vector2 moveDir = new Vector2(_dir.X, _dir.Y);
            if (moveDir.LengthSquared() > 0) moveDir.Normalize();

            // dead-end probe (prevents end bouncing & stuck)
            Vector2 aheadPos = Transform.Position + moveDir * (_collision._cellSize * 0.55f);

            if (!_collision.IsRoadWorld(aheadPos))
            {
                _deadEndTimer += dt;
                if (_deadEndTimer >= DeadEndGrace)
                {
                    IsDead = true;
                    return;
                }
            }
            else
            {
                _deadEndTimer = 0f;
            }

            Transform.Position += moveDir * _currentSpeed * dt;
            UpdateCollider();

            // off-road despawn (keep)
            if (_collision.IsRoadWorld(Transform.Position))
            {
                _offRoadTimer = 0f;
            }
            else
            {
                _offRoadTimer += dt;
                if (_offRoadTimer >= DespawnGrace)
                    IsDead = true;
            }
        }
        
        // Check if there's a car ahead and adjust speed accordingly
        private void CheckForCarsAhead()
        {
            Vector2 fwd = new Vector2(_dir.X, _dir.Y);
            if (fwd.LengthSquared() == 0)
            {
              _currentSpeed = Speed;
              return;
            }

            fwd.Normalize();

            // Sample a few points ahead to detect cars
            float closestCarDistance = float.MaxValue;
            bool carDetected = false;

            for (float dist = 10f; dist <= LookAheadDistance; dist += 15f)
            {
               Vector2 checkPos = Transform.Position + fwd * dist;
               
               // move reusable check box in front of the car
               _checkBox.Position = checkPos - new Vector2(16, 16);

                // reuse list instead of allocating a new one
               _nearbyCars.Clear();
               _collision.DynamicHash.QueryNearby(checkPos, 1, _nearbyCars);

               foreach (var col in _nearbyCars)
               {
                   if (col.Owner == this) continue;
                   if (col.Owner is Car otherCar && otherCar.IsDead) continue;

                   if (col.Layer == CollisionLayer.CAR && col.Intersects(_checkBox))
                   {
                       carDetected = true;
                       closestCarDistance = Math.Min(closestCarDistance, dist);
                       break;
                   }
               }
               if (carDetected && dist < MinFollowDistance)
                    break;
            }

            // Adjust speed based on car ahead
            if (carDetected)
            {

                float speedFactor = (closestCarDistance - MinFollowDistance) /
                                    (LookAheadDistance - MinFollowDistance);

                speedFactor = Math.Clamp(speedFactor, 0f, 1f);

                // never fully stop; always keep a tiny bit of motion
                _currentSpeed = Math.Max(MinAvoidSpeed, Speed * speedFactor);
            }
            else
            {
                // No car ahead, go full speed
                _currentSpeed = Speed;
            }
        }
        
        // snap to exact lane center
        private void SnapToLaneCenter()
        {
            Point gridPos = _collision.WorldToGrid(Transform.Position);
            float cz = _collision._cellSize;
            Vector2 center = new Vector2(gridPos.X * cz + cz / 2f, gridPos.Y * cz + cz / 2f);
            
            // If moving horizontally, lock y to center
            if (_dir.X != 0) 
            {
                Transform.Position = new Vector2(Transform.Position.X, center.Y);
            }
            // If moving vertically, lock x to center
            else if (_dir.Y != 0)
            {
                Transform.Position = new Vector2(center.X, Transform.Position.Y);
            }
        }
        
        // chooses when and where the car should turn
        private void ChooseDirectionIfNeeded(float dt)
        {
            // only decide when the car enters a new tile
            Point curTile = _collision.WorldToGrid(Transform.Position);
            if (curTile == _lastDecisionTile) return;
            _lastDecisionTile = curTile;

            // snap ONCE per tile to remove drift
            SnapToTileCenter();
            UpdateCollider();

            // preferred traffic direction for this tile
            Point preferred = GetPreferredDirForTile(curTile);

            // If traffic rule says "no valid forward direction", despawn
            if (preferred == Point.Zero)
            {
                IsDead = true;
                return;
            }

            // If preferred direction is valid, take it
            if (IsRoadNeighbor(curTile, preferred))
            {
                _dir = preferred;
                return;
            }

            // Try to keep going straight (only if not reversing)
            if (_dir != Point.Zero && IsRoadNeighbor(curTile, _dir))
            {
                _dir = _dir;
                return;
            }

            // Fallback: choose any valid direction EXCEPT reversing
            var dirs = new[] {
                new Point(1, 0),
                new Point(-1, 0),
                new Point(0, 1),
                new Point(0, -1)
            };

            Point opposite = new Point(-_dir.X, -_dir.Y);
            var options = new List<Point>(4);

            foreach (var d in dirs)
            {
                if (d == opposite) continue;   
                if (IsRoadNeighbor(curTile, d))
                    options.Add(d);
            }

            if (options.Count > 0)
            {
                _dir = options[_rng.Next(options.Count)];
            }
            else
            {
                // Only possible move would be reversing → despawn instead
                IsDead = true;
            }
        }
        
        private void SnapToTileCenter()
        {
            Point g = _collision.WorldToGrid(Transform.Position);
            float cs = _collision._cellSize;

            Transform.Position = new Vector2(
                g.X * cs + cs / 2f,
                g.Y * cs + cs / 2f
            );
        }
        
        private bool IsRoadNeighbor(Point tile, Point dir)
        {
            if (dir == Point.Zero) return false;
            Point n = new Point(tile.X + dir.X, tile.Y + dir.Y);
            return _collision.IsRoadTile(n);
        }
        
        private Point GetPreferredDirForTile(Point tile)
        {
            bool left  = _collision.IsRoadTile(new Point(tile.X - 1, tile.Y));
            bool right = _collision.IsRoadTile(new Point(tile.X + 1, tile.Y));
            bool up    = _collision.IsRoadTile(new Point(tile.X, tile.Y - 1));
            bool down  = _collision.IsRoadTile(new Point(tile.X, tile.Y + 1));

            bool horizontal = left || right;
            bool vertical   = up || down;

            // vertical (or intersection) traffic wants DOWN
            if (vertical && down) return new Point(0, 1);

            // horizontal traffic wants LEFT
            if (horizontal && left) return new Point(-1, 0);

            // if we can't go in our traffic direction, return zero -> caller will despawn/fallback
            return Point.Zero;
        }
        
        public void UpdateCollider()
        {
            Collider.Position = Transform.Position - new Vector2(Collider.Width / 2f, Collider.Height / 2f);
        }
        
        public void Draw(SpriteBatch sb, Texture2D pixel)
        {
            if (IsDead) return;
            
            var rect = new Rectangle(
                (int)Collider.Position.X,
                (int)Collider.Position.Y,
                Collider.Width,
                Collider.Height
            );

            sb.Draw(pixel, rect, Color.Orange);
        }
    }
}