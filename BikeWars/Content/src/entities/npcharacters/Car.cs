using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.managers;
using BikeWars.Content.engine.interfaces;
using System.Collections.Generic;
using BikeWars.Content.utils;

namespace BikeWars.Content.entities.npcharacters
{
    public class Car
    {
        private readonly CollisionManager _collision;
        public Random _rng;

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

        public string SideKey;
        public string UpKey;

        private readonly Rectangle _srcSide;
        private readonly Rectangle _srcUp;

        // prevents the car from choosing a new direction multiple times in the same tile
        private Point _lastDecisionTile = new Point(int.MinValue, int.MinValue);

        public Car(Vector2 startWorldCenter, Point startDir, CollisionManager collision, Random rng, string sideKey, string upKey)
        {
            _collision = collision;
            _rng = rng;

            SideKey = sideKey;
            UpKey = upKey;

            _srcSide = SpriteFrameDictionary.GetFrames(sideKey)[0];
            _srcUp   = SpriteFrameDictionary.GetFrames(upKey)[0];

            Transform = new Transform(startWorldCenter, new Point(150, 150));

            Collider = new BoxCollider(
                Transform.Position - new Vector2(75, 75),
                150,
                150,
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
            if (IsDead) return;

            SnapToLaneCenter();

            // check for cars ahead on a timer
            _aheadCheckTimer += dt;
            if (_aheadCheckTimer >= AheadCheckInterval)
            {
                _aheadCheckTimer = 0f;
                CheckForCarsAhead();
            }

            SeparateFromNearbyCars(dt);

            Vector2 moveDir = new Vector2(_dir.X, _dir.Y);
            if (moveDir.LengthSquared() > 0) moveDir.Normalize();

            // look slightly ahead to check if the car is about to drive into a dead end
            Vector2 aheadPos = Transform.Position + moveDir * (_collision._cellSize * 0.55f);

            // Despawn the car if it keeps driving into a dead end
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

            // off-road despawn
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

            // sample a few points ahead to detect cars
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

               // a car checks if there is another car in front of it
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

            // adjust speed based on car ahead
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
                // no car ahead, go full speed
                _currentSpeed = Speed;
            }
        }

        // Push cars apart if they're overlapping or too close
        private void SeparateFromNearbyCars(float dt)
        {
            _nearbyCars.Clear();
            _collision.DynamicHash.QueryNearby(Transform.Position, 2, _nearbyCars);

            Vector2 separationForce = Vector2.Zero;
            int nearbyCount = 0;

            foreach (var col in _nearbyCars)
            {
                if (col.Owner == this) continue;
                if (col.Owner is Car otherCar && otherCar.IsDead) continue;

                if (col.Layer == CollisionLayer.CAR && col.Intersects(Collider))
                {
                    // calculate direction away from other car
                    Vector2 toCar = Transform.Position - ((Car)col.Owner).Transform.Position;
                    float distance = toCar.Length();

                    // If too close, push away
                    if (distance < 150f && distance > 0.1f)
                    {
                        toCar.Normalize();
                        // Stronger push when closer
                        float pushStrength = (150f - distance) / 150f;
                        separationForce += toCar * pushStrength;
                        nearbyCount++;
                    }
                }
            }

            // Apply the separation force
            if (nearbyCount > 0)
            {
                separationForce /= nearbyCount; // Average the force
                Transform.Position += separationForce * 150f * dt; // Push speed
            }
        }

        // snap to exact lane center
        private void SnapToLaneCenter()
        {
            Point gridPos = _collision.WorldToGrid(Transform.Position);
            float cz = _collision._cellSize;
            Vector2 center = new Vector2(gridPos.X * cz + cz / 2f, gridPos.Y * cz + cz / 2f);

            // if moving horizontally, lock y to center
            if (_dir.X != 0)
            {
                Transform.Position = new Vector2(Transform.Position.X, center.Y);
            }
            // if moving vertically, lock x to center
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

            // snap once per tile to remove drift
            SnapToTileCenter();
            UpdateCollider();

            // preferred traffic direction for this tile
            Point preferred = GetPreferredDirForTile(curTile);

            // if traffic rule says "no valid forward direction", despawn
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

            // find possible turn directions, but don't allow reversing
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
                // if the only possible move is reverse, despawn the car
                IsDead = true;
            }
        }

        // center car on tile
        private void SnapToTileCenter()
        {
            Point g = _collision.WorldToGrid(Transform.Position);
            float cs = _collision._cellSize;

            Transform.Position = new Vector2(
                g.X * cs + cs / 2f,
                g.Y * cs + cs / 2f
            );
        }

        // returns true if moving in this direction leads to another road tile
        private bool IsRoadNeighbor(Point tile, Point dir)
        {
            if (dir == Point.Zero) return false;
            Point n = new Point(tile.X + dir.X, tile.Y + dir.Y);
            return _collision.IsRoadTile(n);
        }

        // returns the intended driving direction for this road tile
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

        public void Draw(SpriteBatch sb)
        {
            if (IsDead) return;

            Texture2D atlas = SpriteManager.GetCharacterAtlas();

            Rectangle src;
            SpriteEffects fx = SpriteEffects.None;

            if (_dir.X != 0)
            {
                src = _srcSide;
                if (_dir.X > 0) fx = SpriteEffects.FlipHorizontally; // right
            }
            else
            {
                src = _srcUp;
                if (_dir.Y < 0) fx = SpriteEffects.FlipVertically;   // up
            }

            // scale down to tile size
            float targetPx = 150f; // try 40f if you want bigger
            float scale = targetPx / MathF.Max(src.Width, src.Height);

            sb.Draw(
                atlas,
                Transform.Position,
                src,
                Color.White,
                0f,
                new Vector2(src.Width / 2f, src.Height / 2f),
                scale,
                fx,
                0f
            );
        }
    }
}