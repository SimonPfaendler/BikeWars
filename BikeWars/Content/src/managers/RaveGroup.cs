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
        // minimum radius the circle can shrink to
        private readonly float _minRadius;
        private readonly float _shrinkSpeed;
        
        // alternating and flipping behaviour
        private float _beatTimer = 0f;
        private readonly float _beatInterval;
        private bool _beatFlip = false;
        
        // music proximity behaviour
        private bool _raveMusicActive = false;

        // radius at which music starts when the player enters
        private readonly float _musicEnterRadius;
        // radius at which music stops when the player leaves
        private readonly float _musicExitRadius;
        
        public bool IsActive => !_isDispersed && _ravers.Any(r => !r.IsDead);

        public RaveGroup(List<Raver> ravers,
            AudioService audioService,
            GameObjectManager gameObjectManager,
            CollisionManager collisionManager,
            float startRadius,
            Vector2 circleCenter,
            float shrinkSpeed,
            float minRadius,
            float beatInterval
            )
        {
            _ravers = ravers;
            _audioService = audioService;
            _gameObjectManager = gameObjectManager;
            _collisionManager = collisionManager;

            _radius = startRadius;
            _circleCenter = circleCenter;
            _shrinkSpeed = shrinkSpeed;
            _minRadius = minRadius;
            _beatInterval = beatInterval;
            
            _musicEnterRadius = startRadius + 150f;
            _musicExitRadius  = startRadius + 180f;
        }

        // Spawns a rave circle around Player1
        public static RaveGroup? SpawnAroundPlayer(
            int count,
            float startRadius,
            float raverSize,
            AudioService audioService,
            GameObjectManager gameObjectManager,
            CollisionManager collisionManager,
            float shrinkSpeed,
            float minRadius,
            float beatInterval
        )
        {
            if (gameObjectManager.Player1 == null)
                return null;

            audioService.Music.Pause();
            audioService.Sounds.PlayLoop(AudioAssets.RaverSound);

            if (count < 1)
                count = 1;

            Vector2 circleCenter = gameObjectManager.Player1.Transform.Position;

            var ravers =  new List<Raver>(count);
            
            // spawn ravers evenly spaced around the circle
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * (i / (float)count);

                float cos = (float)Math.Cos(angle);
                float sin = (float)Math.Sin(angle);

                Vector2 pos = circleCenter + new Vector2(cos, sin) * startRadius;

                var r = new Raver(pos, raverSize, audioService);
                
                // even-indexed ravers start facing left, odd facing right
                bool startLeft = (i % 2 == 0);
                r.SetFacingLeft(startLeft);
                
                ravers.Add(r);

                // Register in world so Update/Draw/Collision includes them
                gameObjectManager.AddCharacter(r);
            }

            var group = new RaveGroup(
                ravers,
                audioService,
                gameObjectManager,
                collisionManager,
                startRadius,
                circleCenter,
                shrinkSpeed,
                minRadius,
                beatInterval
            );
            
            group._raveMusicActive = true;

            return group;
        }


        public void Update(GameTime gameTime)
        {
            if (_isDispersed)
                return;

            if (_gameObjectManager.Player1 == null)
                return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 playerPos = _gameObjectManager.Player1.Transform.Position;

            
            UpdateMusicProximity(playerPos);

            //shrink the circle
            _radius = Math.Max(_minRadius,  _radius - _shrinkSpeed * dt);
            
            UpdateBeat(dt);

            // Keep ravers arranged around the player
            UpdateRaverPositions();

            if (CheckBreakCondition())
                Disperse();
        }

        // starts or stops rave music depending on player distance
        private void UpdateMusicProximity(Vector2 playerPos)
        {
            float dist = Vector2.Distance(playerPos, _circleCenter);

            if (_raveMusicActive && dist > _musicExitRadius)
                StopRaveMusic();
            else if (!_raveMusicActive && dist < _musicEnterRadius)
                StartRaveMusic();
        }

        private void StartRaveMusic()
        {
            if (_raveMusicActive)
                return;
            
            _audioService.Music.Pause();
            _audioService.Sounds.PlayLoop(AudioAssets.RaverSound);
            _raveMusicActive = true;
        }

        private void StopRaveMusic()
        {
            if (!_raveMusicActive)
                return;

            _audioService.Sounds.StopLoop(AudioAssets.RaverSound);
            _audioService.Music.Resume();
            _raveMusicActive = false;
        }
        
        // updates the beat timer and flips animation direction on each beat
        private void UpdateBeat(float dt)
        {
            if (_beatInterval <= 0f)
                return;
            
            _beatTimer += dt;

            while (_beatTimer >= _beatInterval)
            {
                _beatTimer -= _beatInterval;
                _beatFlip = !_beatFlip;
            }
        }

        private void UpdateRaverPositions()
        {

            // Use the fixed list count for angles so adjacency is stable.
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
                
                bool even = (i % 2 == 0);
                bool lookLeft;

                if (_beatFlip)
                    lookLeft = !even;
                else
                    lookLeft = even;
                
                r.SetFacingLeft(lookLeft);

                float angle = i * step;

                float cos = (float)Math.Cos(angle);
                float sin = (float)Math.Sin(angle);

                // The positions on the circle
                Vector2 desired = _circleCenter + new Vector2(cos, sin) * _radius;
                
                // Try to keep it walkable: if blocked, push outward along radial direction
                if (TryResolveWalkableRadially(desired, _circleCenter, out Vector2 resolved))
                {
                    // r.LastTransform = new Transform(r.Transform.Position, r.Transform.Size);
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

            const float step = 16f;
            const int maxSteps = 10;

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
        
        // check if 3 adjacent ravers are dead
        private bool CheckBreakCondition()
        {
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
            StopRaveMusic();

            _ravers.Clear();
        }

    }
}

