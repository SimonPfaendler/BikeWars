using System;
using System.Collections.Generic;
using System.Linq;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine;
using BikeWars.Content.managers;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.managers
{
    public class RaveGroup
    {
        private List<Raver> _ravers;
        private readonly AudioService _audioService;
        private readonly GameObjectManager _gameObjectManager;
        private readonly CollisionManager _collisionManager;
        
        private bool _isDispersed;
        
        // circle behaviour
        private float _radius;
        private readonly Vector2 _circleCenter;
        private readonly float _minRadius;
        private readonly float _shrinkSpeed;
        
        // Optional music
        // private readonly string? _musicTrackName;
        // private bool _musicStarted;

        public bool IsActive => !_isDispersed && _ravers.Any(r => !r.IsDead);

        public RaveGroup(List<Raver> ravers,
            AudioService audioService,
            GameObjectManager gameObjectManager,
            CollisionManager collisionManager,
            float startRadius,
            Vector2 circleCenter,
            float shrinkSpeed,
            float minRadius,
            string? musicTrackName)
        {
            _ravers = ravers;
            _audioService = audioService;
            _gameObjectManager = gameObjectManager;
            _collisionManager = collisionManager;
            
            _radius = startRadius;
            _circleCenter = circleCenter;
            _shrinkSpeed = shrinkSpeed;
            _minRadius = minRadius;
            
            // TODO: Play Rave Music
            // _audioService.Music.Play("RaveTrack");
            // _musicTrackName = musicTrackName;
            // StartMusicIfNeeded();
        }
        
        // Spawns a rave circle around Player1
        public static RaveGroup? SpawnAroundPlayer(
            int count,
            float startRadius,
            Point raverSize,
            AudioService audioService,
            GameObjectManager gameObjectManager,
            CollisionManager collisionManager,
            float shrinkSpeed = 20f,
            float minRadius = 60f,
            string? musicTrackName = null
        )
        {
            if (gameObjectManager.Player1 == null)
                return null;
            
            if (count < 1)
                count = 1;
            
            Vector2 circleCenter = gameObjectManager.Player1.Transform.Position;
            
            var ravers =  new List<Raver>(count);
            
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * (i / (float)count);
                
                float cos = (float)Math.Cos(angle);
                float sin = (float)Math.Sin(angle);

                Vector2 pos = circleCenter + new Vector2(cos, sin) * startRadius;

                var r = new Raver(pos, raverSize, audioService);
                ravers.Add(r);

                // Register in world so Update/Draw/Collision includes them
                gameObjectManager.AddCharacter(r);
            }

            return new RaveGroup(
                ravers,
                audioService,
                gameObjectManager,
                collisionManager,
                startRadius,
                circleCenter,
                shrinkSpeed,
                minRadius,
                musicTrackName
            );
        }

        public void Update(GameTime gameTime)
        {
            if (_isDispersed)
                return;
            
            if (_gameObjectManager.Player1 == null)
                return;
            
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            //shrink the circle
            _radius = Math.Max(_minRadius,  _radius - _shrinkSpeed * dt);
            
            // Keep ravers arranged around the player
            UpdateRaverPositions();
            
            if (CheckBreakCondition())
            {
                Disperse();
            }
        }

        private void UpdateRaverPositions()
        {
            // compute circle center from the player position
            // Vector2 center = _gameObjectManager.Player1!.Transform.Position;
            
            // IMPORTANT: Use the fixed list count for angles so adjacency is stable.
            int n = _ravers.Count;
            if (n == 0) 
                return;
            
            float step = MathHelper.TwoPi / n;

            for (int i = 0; i < n; i++)
            {
                var r = _ravers[i];
                if (r == null) continue;
                if (r.IsDead) 
                    continue; // dead ones don't need to be repositioned

                float angle = i * step;
                
                float cos = (float)Math.Cos(angle);
                float sin = (float)Math.Sin(angle);

                // The positions on the circle
                Vector2 desired = _circleCenter + new Vector2(cos, sin) * _radius;

                
                // Try to keep it walkable: if blocked, push outward along radial direction
                if (TryResolveWalkableRadially(desired, _circleCenter, out Vector2 resolved))
                {
                    r.LastTransform = new Transform(r.Transform.Position, r.Transform.Size);
                    r.Transform.Position = resolved;
                    r.UpdateCollider();
                }
                else
                {
                    // If no walkable tile was found nearby, keep current position
                    // (so we never teleport into a wall/tree and never jitter randomly)
                    r.UpdateCollider();
                }
            }
        }
        
        private bool TryResolveWalkableRadially(Vector2 desired, Vector2 center, out Vector2 resolved)
        {
            resolved = desired;

            // If desired is already walkable -> done
            Point g = _collisionManager.WorldToGrid(desired);

            int gridW = _collisionManager.PathGrid.GetLength(0);
            int gridH = _collisionManager.PathGrid.GetLength(1);

            if (g.X >= 0 && g.X < gridW && g.Y >= 0 && g.Y < gridH &&
                _collisionManager.PathGrid[g.X, g.Y].Walkable)
            {
                return true;
            }

            // Otherwise: push outward along the radius direction to find the nearest walkable tile
            Vector2 dir = desired - center;
            if (dir.LengthSquared() < 0.0001f)
                dir = new Vector2(1, 0);
            else
                dir.Normalize();

            const float step = 16f;        // one tile step (matches your CELL_SIZE)
            const int maxSteps = 10;       // search up to 10 tiles outward

            for (int i = 1; i <= maxSteps; i++)
            {
                Vector2 candidate = desired + dir * (step * i);
                Point cg = _collisionManager.WorldToGrid(candidate);

                if (cg.X < 0 || cg.X >= gridW || cg.Y < 0 || cg.Y >= gridH)
                    continue;

                if (_collisionManager.PathGrid[cg.X, cg.Y].Walkable)
                {
                    resolved = candidate;
                    return true;
                }
            }

            // No valid spot found -> keep them where they are
            return false;
        }

        private bool CheckBreakCondition()
        {
            // check if 3 adjacent ravers are dead
            int n =  _ravers.Count;

            if (n < 3)
                return true;
            
            // selects three neighboring ravers in a circular list
            for (int i = 0; i < n; i++)
            {
                var r1 =  _ravers[i];
                var r2 = _ravers[(i + 1) % n];
                var r3 = _ravers[(i + 2) % n];

                if (r1 != null && r2 != null && r3 != null &&
                    r1.IsDead && r2.IsDead && r3.IsDead)
                {
                    return true;
                }
            }
            return false;
        }

        private void Disperse()
        {
            _isDispersed = true;
            
            // kill/despawn all remaining ravers
            foreach (var r in _ravers)
            {
                if (r == null) continue;

                r.Attributes.Health = 0;

                // Remove from the world collection (GameObjectManager exposes Characters)
                _gameObjectManager.Characters.Remove(r);
            }

            _ravers.Clear();
            // Stop music if needed
            // _audioService.Music.Stop();
        }
        
        // private void StartMusicIfNeeded()
        // {
        //     if (_musicStarted) return;
        //     if (string.IsNullOrWhiteSpace(_musicTfrackName)) return;
        //     if (_audioService == null) return;
        //
        //     // TODO: adapt to your AudioService API
        //     // Example:
        //     // _audioService.Music.Play(_musicTrackName);
        //
        //     _musicStarted = true;
        // }
        //
        // private void StopMusicIfNeeded()
        // {
        //     if (!_musicStarted) return;
        //     if (string.IsNullOrWhiteSpace(_musicTrackName)) return;
        //     if (_audioService == null) return;
        //
        //     // TODO: adapt to your AudioService API
        //     // Example:
        //     // _audioService.Music.Stop();
        //
        //     _musicStarted = false;
        // }
    }
}

