#nullable enable
using System;
using System.Linq;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Entities.Characters;
using BikeWars.Content.entities.interfaces;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Autofac.Features.GeneratedFactories;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.components;
using BikeWars.Content.entities.npcharacters;
using BikeWars.Utilities;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.managers
{
    public class SpawnManager
    {
        private readonly GameObjectManager _gameObjectManager;
        private readonly CollisionManager _collisionManager;
        private readonly AudioService _audioService;
        private readonly PathFinding _pathFinding;
        private double _totalTime;
        private double _timeSinceLastSpawn;
        private double _timeSinceLastSwarm;
        private double _timeSinceLastCircle;
        private double _timeSinceLastSlowEnemyLinear;
        private const double SWARM_INTERVAL = 33.0;
        private const double CIRCLE_SPAWN_INTERVAL = 45.0;
        //private const double SLOW_ENEMY_SPAWN_INTERVAL = 60.0;
        private readonly List<ICollider> _spawnQueryBuffer = new(32);

        private const double GAME_DURATION = 5 * 60; // 5 minutes in seconds
        private const double START_SPAWN_INTERVAL = 3; // Start with 4 seconds
        private const double END_SPAWN_INTERVAL = 0.2;   // End with 0.5 seconds
        private double _spawnInterval;
        private const float MIN_SPAWN_RADIUS = 450f;
        private const float MAX_SPAWN_RADIUS = 900f;
        private readonly WorldAudioManager _worldAudioManager;

        public event Action RaverGroupDied;

        // Tram Logic
        private double _timeSinceLastTram;
        private const double TRAM_SPAWN_INTERVAL = 28.0; // Every 15 seconds

        // raver logic
        private List<RaveGroup> _raveGroups = new List<RaveGroup>();


        // car logic
        private double _timeSinceLastCar;
        private const double CAR_SPAWN_START = 1.5;
        private const double CAR_SPAWN_END   = 0.7;

        private readonly RepathScheduler _repathScheduler;
        public SpawnManager(GameObjectManager gameObjectManager, CollisionManager collisionManager, AudioService audioService, PathFinding pathFinding, RepathScheduler repathScheduler, WorldAudioManager worldAudioManager)
        {
            _gameObjectManager = gameObjectManager;
            _collisionManager = collisionManager;
            _audioService = audioService;
            _spawnInterval = START_SPAWN_INTERVAL;
            _pathFinding = pathFinding;
            _repathScheduler = repathScheduler;
            _worldAudioManager = worldAudioManager;
        }

        public void Update(GameTime gameTime)
        {
            double elapsed = gameTime.ElapsedGameTime.TotalSeconds;
            _totalTime += elapsed;
            _timeSinceLastSpawn += elapsed;
            _timeSinceLastSwarm += elapsed;
            _timeSinceLastCircle += elapsed;
            _timeSinceLastTram += elapsed;
            _timeSinceLastCar += elapsed;
            _timeSinceLastSlowEnemyLinear += elapsed;

            // Update spawn interval based on progression
            // Lerp from start interval to end interval based on time fraction
            double progress = Math.Clamp(_totalTime / GAME_DURATION, 0, 1);
            int playerCount = 0;
            if (_gameObjectManager.Player1 != null && !_gameObjectManager.Player1.IsDead)
                playerCount += 1;
            if (_gameObjectManager.Player2 != null && !_gameObjectManager.Player2.IsDead)
                playerCount += 1;
            _spawnInterval = START_SPAWN_INTERVAL + (END_SPAWN_INTERVAL - START_SPAWN_INTERVAL) * progress;
            if (playerCount == 2)
            {
                _spawnInterval *= 0.7; 
            }
            double carInterval = CAR_SPAWN_START + (CAR_SPAWN_END - CAR_SPAWN_START) * progress;
            double tramChancePerSecond = 0.05 + 0.7 * progress;
            double spawnProbability = tramChancePerSecond * elapsed;

            if (RandomUtil.NextDouble() < spawnProbability && _totalTime > 90)
            {
                SpawnTram();
            }
            if (_timeSinceLastSpawn >= _spawnInterval)
            {
                SpawnEnemy(progress);
                _timeSinceLastSpawn = 0;
            }

            if (_totalTime >= 60 && _timeSinceLastSwarm >= SWARM_INTERVAL)
            {
                SpawnSwarm(progress);
                _timeSinceLastSwarm = 0;
            }

            if (_timeSinceLastCircle >= CIRCLE_SPAWN_INTERVAL)
            {
                SpawnCircle(progress);
                _timeSinceLastCircle = 0;
            }

            if (_timeSinceLastTram >= TRAM_SPAWN_INTERVAL)
            {
                SpawnTram();
                _timeSinceLastTram = 0;
            }

            for (int i = _raveGroups.Count - 1; i >= 0; i--)
            {
                _raveGroups[i].Update(gameTime);

                // remove finished/dispersed groups
                if (!_raveGroups[i].IsActive)
                    _raveGroups.RemoveAt(i);
            }

            if (_timeSinceLastCar >= carInterval)
            {
                SpawnCar(progress);
                _timeSinceLastCar = 0;
            }

            //if (_totalTime >= 90 && _timeSinceLastSlowEnemyLinear >= SLOW_ENEMY_SPAWN_INTERVAL)
            //{
            //    SpawnSlowEnemyLiniear(12, progress);
            //    _timeSinceLastSlowEnemyLinear = 0;
            //}
        }

        public void SpawnSlowEnemyLiniear(int count, double progress)
        {
            count += 10 * (int)Math.Round(progress) * count;
            float difficultyMultiplier = 1.0f + (1.0f * (float)progress);
            float speedMultiplier = 0.5f;

            if (_gameObjectManager.Player1 == null) return;
            Vector2 center = _gameObjectManager.Player1.Transform.Position;

            float spacing = 50f; // Spacing between enemies
            Vector2 startPos;
            Vector2 step;

            if (center.X <= 5600 && center.Y <= 5600) // upper left: line to the left of the player, vertical
            {
                startPos = center + new Vector2(-400f, -((count - 1) * spacing) / 2f);
                step = new Vector2(0, spacing);
            }
            else if (center.X <= 5600 && center.Y > 5600) // lower left: line below the player, horizontal
            {
                startPos = center + new Vector2(-((count - 1) * spacing) / 2f, 400f);
                step = new Vector2(spacing, 0);
            }
            else if (center.X > 5600 && center.Y > 5600) // lower right: line to the right of the player, vertical
            {
                startPos = center + new Vector2(400f, -((count - 1) * spacing) / 2f);
                step = new Vector2(0, spacing);
            }
            else // upper right: line above the player, horizontal
            {
                startPos = center + new Vector2(-((count - 1) * spacing) / 2f, -400f);
                step = new Vector2(spacing, 0);
            }

            for (int i = 0; i < count; i++)
            {
                Vector2 pos = startPos + step * i;

                if (!IsValidSpawnPosition(pos)) continue;

                CharacterBase enemy = new Hobo(pos, 15, _audioService, _pathFinding, _collisionManager, _repathScheduler);

                ApplyScaling(enemy, difficultyMultiplier, speedMultiplier);
                _gameObjectManager.AddCharacter(enemy);
            }
        }

        public void SpawnTram(float spawnRadius = 5000f)
        {
            List<Player> validPlayers = new List<Player>();
            if (_gameObjectManager.Player1 != null && !_gameObjectManager.Player1.IsDead) validPlayers.Add(_gameObjectManager.Player1);
            if (_gameObjectManager.Player2 != null && !_gameObjectManager.Player2.IsDead) validPlayers.Add(_gameObjectManager.Player2);

            if (validPlayers.Count == 0) return;

            Player targetPlayer = validPlayers[RandomUtil.NextInt(0, validPlayers.Count)];

            // Spawn far outside the screen
            Vector2 playerPos = targetPlayer.Transform.Position;
            float angle = (float)(RandomUtil.NextDouble() * Math.PI * 2);
            Vector2 startPos = playerPos + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * spawnRadius;

            // Target a position near the player with some randomness
            Vector2 targetOffset = new Vector2((float)(RandomUtil.NextDouble() - 0.5) * 10, (float)(RandomUtil.NextDouble() - 0.5) * 10);
            Vector2 targetPos = playerPos + targetOffset;

            Tram tram = new Tram(startPos, targetPos,  _audioService, targetPlayer);
            _gameObjectManager.AddTram(tram);
        }

        // spawn the car
        public void SpawnCar(double progress)
        {
            Vector2 spawnPos = Vector2.Zero;
            Point spawnTile = Point.Zero;
            bool found = false;

            // Try up to 100 times to find a road tile where the car can safely spawn
            for (int tries = 0; tries < 100; tries++)
            {
                spawnPos = _collisionManager.GetRandomRoadWorldCenter(Random.Shared);
                if (spawnPos == Vector2.Zero) return;

                spawnTile = _collisionManager.WorldToGrid(spawnPos);

                if (!_collisionManager.IsRoadTile(spawnTile))
                    continue;

                if (!IsCarSpawnFree(spawnPos))
                    continue;

                found = true;
                break;
            }

            if (!found) return;

            // Build valid directions
            var dirs = new[] {
                new Point(1, 0),
                new Point(-1, 0),
                new Point(0, 1),
                new Point(0, -1)
            };
            var validDirs = new List<Point>(4);

            // Find valid road directions
            foreach (var d in dirs)
            {
                Point neighbor = new Point(spawnTile.X + d.X, spawnTile.Y + d.Y);
                if (_collisionManager.IsRoadTile(neighbor))
                    validDirs.Add(d);
            }

            if (validDirs.Count == 0) return;

            // decide direction by street orientation
            bool left  = _collisionManager.IsRoadTile(new Point(spawnTile.X - 1, spawnTile.Y));
            bool right = _collisionManager.IsRoadTile(new Point(spawnTile.X + 1, spawnTile.Y));
            bool up    = _collisionManager.IsRoadTile(new Point(spawnTile.X, spawnTile.Y - 1));
            bool down  = _collisionManager.IsRoadTile(new Point(spawnTile.X, spawnTile.Y + 1));

            Point dir;

            // if horizontal road go left
            if ((left || right) && !(up || down))
                dir = new Point(-1, 0);
            // else go down
            else
                dir = new Point(0, 1);

            // if the chosen dir isn't actually valid here, fall back
            if (!validDirs.Contains(dir))
                dir = validDirs[RandomUtil.NextInt(0, validDirs.Count)];

            int type = RandomUtil.NextInt(1, 5);
            string sideKey = $"Car{type}_Side";
            string upKey   = $"Car{type}_Up";

            var car = new Car(spawnPos, dir, _collisionManager, Random.Shared, sideKey, upKey);
            _gameObjectManager.AddCar(car);
        }

        // checks if the spawn position is far enough away from other cars
        private bool IsCarSpawnFree(Vector2 spawnPos, float minDist = 10f)
        {
            foreach (var car in _gameObjectManager.Cars)
            {
                if (car == null || car.IsDead) continue;
                if (Vector2.Distance(car.Transform.Position, spawnPos) < minDist)
                    return false;
            }
            return true;
        }

        private void SpawnSwarm(double progress)
        {
            int baseCount = (int)Math.Round(20 + 20 * 3 * progress);
            int count = RandomUtil.NextInt(baseCount - 10, baseCount + 10);

            float speedMultiplier = 1f + (0.5f * (float)progress); // Start fast, get faster
            float difficultyMultiplier = 1.0f + (1.5f * (float)progress);

            // Spawn them in a cluster
            Vector2 clusterCenter = GetRandomSpawnPosition();


             for (int i = 0; i < count; i++)
             {
                 // Small random offset from cluster center for each unit
                 Vector2 offset = new Vector2((float)(RandomUtil.NextDouble() - 0.5) * 300, (float)(RandomUtil.NextDouble() - 0.5) * 300);
                 Vector2 spawnPos = clusterCenter + offset;

                 if (!IsValidSpawnPosition(spawnPos))
                 {
                     spawnPos = clusterCenter;
                 }

                //  var hobo = new Hobo(spawnPos, new Point(24, 24), _audioService, _pathFinding,
                 var hobo = new Hobo(spawnPos, 12, _audioService, _pathFinding,
                     _collisionManager, _repathScheduler);
                 ApplyScaling(hobo, difficultyMultiplier, speedMultiplier); // Apply extra speed
                 _gameObjectManager.AddCharacter(hobo);
             }
        }

        private void SpawnEnemy(double progress)
        {
            // Determine type of enemy
            // As time progresses, higher chance for stronger enemies (BikeThief vs Hobo)


            Vector2 spawnPos = GetRandomSpawnPosition();

            // Difficulty scaling
            // Health and Damage multiplier: 1.0 to 3.0 over 15 mins
            float difficultyMultiplier = 1.0f + (1.5f * (float)progress);
            // Speed scaling: 1.0 to 1.5 over 15 mins
            double basespeedMultiplier = 1.0 + (1.1f * progress);
            float speedMultiplier;
            speedMultiplier = (float)RandomSpeed(basespeedMultiplier);
            // chances of each type can be changed here
            // hobo starts at 0.4 and after ends at 0.3
            // progress starts at 0.0 and is 1.0 at the end of Gametime
            // start and end percentages must be 100 in total
            double pHobo = 0.35 - 0.15 * progress; //35% -> 20%
            double pDog = 0.35 - 0.15 * progress; // 35 -> 20
            double pThief    = 0.13 + 0.02 * progress;  // 13 15
            double pDozent   = 0.02 + 0.13 * progress;  // 2  15
            double pPolice = 0.1 + 0.05 * progress;  // 10 -> 15

            double val = RandomUtil.NextDouble();

            double spawnHobo = pHobo;
            double spawnDog = spawnHobo + pDog;
            double spawnThief = spawnDog + pThief;
            double spawnPolice = spawnThief + pPolice;
            double spawnDozent = spawnPolice + pDozent;
            // theoretisch spawnKamikaze, aber faellt unter letzdem else

            if (val < spawnHobo)
            {
                var hobo = new Hobo(spawnPos, 12, _audioService, _pathFinding, _collisionManager, _repathScheduler);
                ApplyScaling(hobo, difficultyMultiplier, speedMultiplier);
                _gameObjectManager.AddCharacter(hobo);
            }
            else if (val < spawnDog)
            {
                var dog = new Dog(spawnPos, 15, _audioService, _pathFinding, _collisionManager, _repathScheduler);
                ApplyScaling(dog, difficultyMultiplier, speedMultiplier);
                dog.SetWorldAudioManager(_worldAudioManager);
                _gameObjectManager.AddCharacter(dog);
            }
            else if (val < spawnThief)
            {
                var thief = new BikeThief(spawnPos, 15, _audioService, _pathFinding, _collisionManager, _repathScheduler);
                ApplyScaling(thief, difficultyMultiplier, speedMultiplier);
                _gameObjectManager.AddCharacter(thief);
            }
            else if (val < spawnPolice)
            {
                var police = new PoliceMan(spawnPos, 20, _audioService, _pathFinding, _collisionManager, _repathScheduler);
                ApplyScaling(police, difficultyMultiplier, speedMultiplier);
                _gameObjectManager.AddCharacter(police);
            }
            else if (val < spawnDozent)
            {
                var thief = new Dozent(spawnPos, 25, _audioService, _pathFinding, _collisionManager, _repathScheduler);
                ApplyScaling(thief, difficultyMultiplier, speedMultiplier);
                _gameObjectManager.AddCharacter(thief);
            }
            else
            {
                var kamikaze = new KamikazeOpa(spawnPos, 15, _audioService, _pathFinding, _collisionManager, _gameObjectManager, _repathScheduler);
                ApplyScaling(kamikaze, difficultyMultiplier, speedMultiplier);
                _gameObjectManager.AddCharacter(kamikaze);
            }
        }

        private void ApplyScaling(CharacterBase character, float difficultyMultiplier, float speedMultiplier)
        {
            character.Attributes.MaxHealth = (int)(character.Attributes.MaxHealth * difficultyMultiplier * 1.3f);
            character.Attributes.Health = character.Attributes.MaxHealth;
            character.Attributes.AttackDamage = (int)(character.Attributes.AttackDamage * difficultyMultiplier);
            character.Speed *= speedMultiplier;
        }

        private bool IsValidSpawnPosition(Vector2 pos)
        {
            // Create a temporary collider for check
            // Use 32x32 size (enemy size)
            BoxCollider checkCollider = new BoxCollider(pos, 32, 32, CollisionLayer.CHARACTER, null);

            _spawnQueryBuffer.Clear();
            _collisionManager.StaticHash.QueryNearby(pos, 3, _spawnQueryBuffer);
            foreach (var col in _spawnQueryBuffer)
            {
                if (col.Layer == CollisionLayer.SPAWNENEMIES && col.Intersects(checkCollider))
                {
                    return true;
                }
            }
            return false;
        }

        // spawn the rave group
        private void SpawnCircle(double progress)
        {
            Player? target = _gameObjectManager.GetTargetPlayer(Vector2.Zero);
            if (target == null) return;

            int count = 30 + (int)(progress * 30); // 30..60
            float startRadius = 500f;

            // pick a size you want for ravers
            Point raverSize = new Point(32, 32);

            // optional: prevent multiple rave groups at once
            if (_raveGroups.Any(g => g.IsActive))
                return;

            // Spawn the rave group (this already calls GameObjectManager.AddCharacter internally)
            var group = RaveGroup.SpawnAroundPlayer(
                target: target,
                count: count,
                startRadius: startRadius,
                raverSize: 20,
                audioService: _audioService,
                gameObjectManager: _gameObjectManager,
                collisionManager: _collisionManager,
                shrinkSpeed: 30f,
                minRadius: 300f,
                beatInterval: 0.2f
            );

            if (group != null)
            {
                group.OnDied += HandleRaveGroupDied;
                _raveGroups.Add(group);
            }
        }

        private Vector2 GetRandomSpawnPosition()
        {
            Player? target = _gameObjectManager.GetTargetPlayer(Vector2.Zero);
            if (target == null) return Vector2.Zero;

            Vector2 playerPos = target.Transform.Position;

            for (int i = 0; i < 20; i++) // Try 20 times to find a valid position
            {
                // Makes Oppenents mainly spawn in gaze direction
                Vector2 forward = target.GazeDirection;
                if (forward == Vector2.Zero) forward = Vector2.UnitX;
                forward.Normalize();
                float baseAngle = (float)Math.Atan2(forward.Y, forward.X);
                float halfCone = MathHelper.ToRadians(20f);
                float angle = baseAngle + (float)(RandomUtil.NextDouble() * 2 - 1) * halfCone;
                // Random distance
                float distance = MIN_SPAWN_RADIUS + (float)(RandomUtil.NextDouble() * (MAX_SPAWN_RADIUS - MIN_SPAWN_RADIUS));

                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
                Vector2 pos = playerPos + offset;

                if (IsValidSpawnPosition(pos))
                {
                    return pos;
                }
            }

            // Fallback
            return playerPos + new Vector2(MIN_SPAWN_RADIUS, 0);
        }

        public void Dispose() {
        }
        public void HandleRaveGroupDied()
        {
            RaverGroupDied?.Invoke();
        }

        private double RandomSpeed(double basespeed)
        {
            double normalRange = 0.20f;
            double rareRange = 0.90f;
            double rareChance = 0.30f;
            double varchance = RandomUtil.NextDouble();
            double range = varchance < rareChance ? rareRange : normalRange;
            double factor = 1f + ((float)RandomUtil.NextDouble() * 2f - 1f) * (float)range;

            return basespeed * factor;
        }
    }
}
