using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;

namespace BikeWars.Content.entities.npcharacters
{
    public class Car
    {
        public const int DamageToBike = 6;
        public const int DamageToCharacter = 4;
        
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
        
        // prevents the car from choosing a new direction multiple times in the same tile
        private Point _lastDecisionTile = new Point(int.MinValue, int.MinValue);
        
        // forward road check
        private const int CheckSamplesAhead = 3;
        private float AheadDistancePx => _collision._cellSize * 1.0f;
        
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
            
            // Immediately snap to lane center on spawn
            SnapToLaneCenter();
            UpdateCollider();
        }

        public void Update(GameTime gameTime)
        {
            if (IsDead)
                return;
            
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // snap to center before choosing direction
            SnapToLaneCenter();
            
            ChooseDirectionIfNeeded(dt);

            // Movement
            Vector2 moveDir = new Vector2(_dir.X, _dir.Y);
            if (moveDir.LengthSquared() > 0) moveDir.Normalize();
            
            Transform.Position += moveDir * Speed * dt;
            UpdateCollider();
            
            // Off-road check
            if (_collision.IsRoadWorld(Transform.Position))
            {
                _offRoadTimer = 0f;
            }
            else
            {
                _offRoadTimer += dt;
                if (_offRoadTimer >= DespawnGrace)
                {
                    IsDead = true;
                }
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

            // Normalize direction
            if (_dir.X != 0) _dir = new Point(Math.Sign(_dir.X), 0);
            else if (_dir.Y != 0) _dir = new Point(0, Math.Sign(_dir.Y));

            Point front = _dir;
            Point left  = new Point(-_dir.Y, _dir.X);
            Point right = new Point(_dir.Y, -_dir.X);

            bool validFront = CanContinueStraight(front);   
            bool validLeft  = IsRealIntersection(left);
            bool validRight = IsRealIntersection(right);
            
            // if there is a dead end the car must turn
            if (!validFront)
            {
                if (validLeft && validRight)
                    _dir = (_rng.Next(2) == 0) ? left : right;
                else if (validLeft)
                    _dir = left;
                else if (validRight)
                    _dir = right;
                return; 
            }
            
            // intersection logic
            if ((validLeft || validRight) && _rng.Next(100) < 1)
            {
                var turns = new System.Collections.Generic.List<Point>();
                if (validLeft) turns.Add(left);
                if (validRight) turns.Add(right);
                
                if (turns.Count > 0)
                    _dir = turns[_rng.Next(turns.Count)];
            }
        }
        
        // check if road continues straight ahead 
        private bool CanContinueStraight(Point dir)
        {
            if (dir == Point.Zero) return false;

            Vector2 fwd = new Vector2(dir.X, dir.Y);
            fwd.Normalize();

            // check just a few points directly ahead in the cars lane
            for (int i = 1; i <= CheckSamplesAhead; i++)
            {
                Vector2 checkPos = Transform.Position + fwd * (AheadDistancePx * i / CheckSamplesAhead);
                
                if (!_collision.IsRoadWorld(checkPos))
                    return false;
            }
            
            return true;
        }
        
        // check if there's a real perpendicular intersection
        private bool IsRealIntersection(Point sideDir)
        {
            if (sideDir == Point.Zero) return false;
            
            Vector2 perpVec = new Vector2(sideDir.X, sideDir.Y);
            perpVec.Normalize();
            
            // check if there's road in the perpendicular direction
            float checkDist = _collision._cellSize * 1.5f;
            Vector2 checkPos = Transform.Position + perpVec * checkDist;
            
            // must have road at that perpendicular position
            if (!_collision.IsRoadWorld(checkPos))
                return false;
            
            // check a bit further to confirm it's a real road, not just edge
            Vector2 farCheck = Transform.Position + perpVec * (checkDist * 1.5f);
            return _collision.IsRoadWorld(farCheck);
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