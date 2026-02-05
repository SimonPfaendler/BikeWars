#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
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
        private static int _raveMusicUsers = 0;
        public bool IsActive => !_isDispersed && _ravers.Any(r => !r.IsDead);

        public event Action OnDied;

        // constructor rave group
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
            Player target,
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
            if (target == null || target.IsDead || target.Attributes.Health <= 0)
                return null;

            // ensure at least one raver so circle math never divides by zero
            if (count < 1)
                count = 1;

            Vector2 circleCenter = target.Transform.Position;

            // 4 raver variants
            (string Left, string Right)[] variants =
            {
                ("Raver03_LeftUp", "Raver03_RightUp"),
                ("Raver02_LeftUp", "Raver02_RightUp"),
                ("Raver04_LeftUp", "Raver04_RightUp"),
                ("Raver06_LeftUp", "Raver06_RightUp"),
            };

            var ravers = new List<Raver>(count);

            // spawn ravers evenly spaced around the circle
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * (i / (float)count);

                float cos = (float)Math.Cos(angle);
                float sin = (float)Math.Sin(angle);

                // builds the world position for a raver
                Vector2 pos = circleCenter + new Vector2(cos, sin) * startRadius;

                var variant = variants[i % variants.Length];

                var r = new Raver(pos, raverSize, audioService, variant.Left,
                    variant.Right);

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

            group.StartRaveMusic();
            return group;
        }

        private void PruneMissingRavers()
        {
            // If something else removed ravers from the world (off-screen despawn),
            // we must notice it and clean up (especially music).
            for (int i = _ravers.Count - 1; i >= 0; i--)
            {
                var r = _ravers[i];
                if (r == null)
                {
                    _ravers.RemoveAt(i);
                    continue;
                }

                // If the raver is no longer registered in the world, treat it as gone.
                // (This is what happens when an "off-screen despawn" system removes it.)
                if (!_gameObjectManager.Characters.Contains(r))
                {
                    // optional: mark dead to be consistent
                    r.Attributes.Health = 0;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (_isDispersed)
                return;

            PruneMissingRavers();

            // the circle shouldnt shrink every frame
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float nearestDist = GetNearestAlivePlayerDistance(_circleCenter);

            if (nearestDist < float.PositiveInfinity)
                UpdateMusicProximityByDistance(nearestDist);
            else
                StopRaveMusic();

            // shrink the circle
            _radius = Math.Max(_minRadius,  _radius - _shrinkSpeed * dt);

            // the raver animation shouldn't change every frame
            UpdateBeat(dt);

            // Keep ravers arranged around the player
            UpdateRaverPositions();

            // check if 3 adjacent ravers are dead
            if (CheckBreakCondition())
                Disperse();
        }

        // returns distance of the nearest alive player to the circle center
        private float GetNearestAlivePlayerDistance(Vector2 center)
        {
            float best = float.PositiveInfinity;

            var p1 = _gameObjectManager.Player1;
            if (p1 != null && !p1.IsDead)
            {
                float d = Vector2.Distance(p1.Transform.Position, center);
                if (d < best) best = d;
            }

            var p2 = _gameObjectManager.Player2;
            if (p2 != null && !p2.IsDead)
            {
                float d = Vector2.Distance(p2.Transform.Position, center);
                if (d < best) best = d;
            }

            return best;
        }

        // starts or stops rave music depending on nearest-player distance
        private void UpdateMusicProximityByDistance(float dist)
        {
            if (_raveMusicActive && dist > _musicExitRadius)
                StopRaveMusic();
            else if (!_raveMusicActive && dist < _musicEnterRadius)
                StartRaveMusic();
        }

        private void StartRaveMusic()
        {
            if (_raveMusicActive)
                return;

            _raveMusicActive = true;
            _raveMusicUsers++;

            // Only the first user actually starts the loop
            if (_raveMusicUsers == 1)
            {
                _audioService.Music.Pause();
                _audioService.Sounds.PlayLoop(AudioAssets.RaverSound);
            }
        }

        private void StopRaveMusic()
        {
            if (!_raveMusicActive)
                return;

            _raveMusicActive = false;
            _raveMusicUsers = Math.Max(0, _raveMusicUsers - 1);

            // Only stop when nobody needs it anymore
            if (_raveMusicUsers == 0)
            {
                _audioService.Sounds.StopLoop(AudioAssets.RaverSound);
                _audioService.Music.Resume();
            }
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

            // distance in angle between each raver on the circle
            float step = MathHelper.TwoPi / n;

            for (int i = 0; i < n; i++)
            {
                var r = _ravers[i];
                if (r == null) continue;

                if (r.IsDead)
                    continue;

                // checks whether raver is even or not
                bool even = (i % 2 == 0);
                bool lookLeft;

                if (_beatFlip)
                    lookLeft = !even;
                else
                    lookLeft = even;

                // flips the raver
                r.SetFacingLeft(lookLeft);

                // computes the angle on the circle for raver
                float angle = i * step;

                float cos = (float)Math.Cos(angle);
                float sin = (float)Math.Sin(angle);

                // Calculates where the raver should be
                Vector2 desired = _circleCenter + new Vector2(cos, sin) * _radius;

                // Try to keep it walkable, if blocked, push outward along radial direction
                if (TryFindWalkablePos(desired, _circleCenter, out Vector2 resolved))
                {
                    r.Transform.Position = resolved;
                    r.UpdateCollider(CollisionLayer.CHARACTER);
                }
                else
                {
                    // If no walkable tile was found nearby, keep current position
                    r.UpdateCollider(CollisionLayer.CHARACTER);
                }
            }
        }

        // tries to find a place where a raver is allowed to stand
        private bool TryFindWalkablePos(Vector2 desired, Vector2 center, out Vector2 resolved)
        {
            resolved = desired;

            // If desired is already walkable, done
            Point g = _collisionManager.WorldToGrid(desired);

            int gridW = _collisionManager.PathGrid.GetLength(0);
            int gridH = _collisionManager.PathGrid.GetLength(1);

            // Check if the grid cell inside the map and if the cell is walkable
            if (g.X >= 0 && g.X < gridW && g.Y >= 0 && g.Y < gridH &&
                _collisionManager.PathGrid[g.X, g.Y].Walkable)
            {
                return true;
            }

            // Otherwise, push outward along the radius direction to find the nearest walkable tile
            Vector2 dir = desired - center;

            // If the direction is too small pick a default dir
            if (dir.LengthSquared() < 0.0001f)
                dir = new Vector2(1, 0);
            else
                dir.Normalize();

            const float step = 16f;
            const int maxNrOfSteps = 10;

            // step outward from the blocked position until we find a walkable tile
            for (int i = 1; i <= maxNrOfSteps; i++)
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
            OnDied?.Invoke();
        }

        public void ForceDisperse()
        {
            if (_isDispersed) return;
            Disperse();
        }
    }
}
