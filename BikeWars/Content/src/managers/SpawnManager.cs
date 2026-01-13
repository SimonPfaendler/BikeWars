using System;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Entities.Characters;
using BikeWars.Content.entities.interfaces;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using BikeWars.Content.engine.interfaces;

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
        private const double SWARM_INTERVAL = 65.0;
        private const double CIRCLE_SPAWN_INTERVAL = 100.0;

        private readonly List<ICollider> _spawnQueryBuffer = new(32);

        private const double GAME_DURATION = 15 * 60; // 15 minutes in seconds
        private const double START_SPAWN_INTERVAL = 4; // Start with 4 seconds
        private const double END_SPAWN_INTERVAL = 0.5;   // End with 0.5 seconds
        private double _spawnInterval;
        private const float MIN_SPAWN_RADIUS = 300f;
        private const float MAX_SPAWN_RADIUS = 700f;

        private readonly Random _random;
        private readonly RepathScheduler _repathScheduler;

        public SpawnManager(GameObjectManager gameObjectManager, CollisionManager collisionManager, AudioService audioService, PathFinding pathFinding, RepathScheduler repathScheduler)
        {
            _gameObjectManager = gameObjectManager;
            _collisionManager = collisionManager;
            _audioService = audioService;
            _random = new Random();
            _spawnInterval = START_SPAWN_INTERVAL;
            _pathFinding = pathFinding;
            _repathScheduler = repathScheduler;
        }

        public void Update(GameTime gameTime)
        {
            double elapsed = gameTime.ElapsedGameTime.TotalSeconds;
            _totalTime += elapsed;
            _timeSinceLastSpawn += elapsed;
            _timeSinceLastSwarm += elapsed;
            _timeSinceLastCircle += elapsed;

            // Update spawn interval based on progression
            // Lerp from start interval to end interval based on time fraction
            double progress = Math.Clamp(_totalTime / GAME_DURATION, 0, 1);
            _spawnInterval = START_SPAWN_INTERVAL + (END_SPAWN_INTERVAL - START_SPAWN_INTERVAL) * progress;

            if (_timeSinceLastSpawn >= _spawnInterval)
            {
                SpawnEnemy(progress);
                _timeSinceLastSpawn = 0;
            }

            if (_timeSinceLastSwarm >= SWARM_INTERVAL)
            {

                SpawnSwarm(progress);
                _timeSinceLastSwarm = 0;
            }

            if (_timeSinceLastCircle >= CIRCLE_SPAWN_INTERVAL)
            {
                SpawnCircle(progress);
                _timeSinceLastCircle = 0;
            }
        }

        private void SpawnSwarm(double progress)
        {

            // Spawn 10-15 Hobos
            int count = _random.Next(10, 16);


            float speedMultiplier = 1.5f + (0.5f * (float)progress); // Start fast, get faster
            float difficultyMultiplier = 1.0f + (2.0f * (float)progress);

            // Spawn them in a cluster
            Vector2 clusterCenter = GetRandomSpawnPosition();


             for (int i = 0; i < count; i++)
             {
                 // Small random offset from cluster center for each unit
                 Vector2 offset = new Vector2((float)(_random.NextDouble() - 0.5) * 100, (float)(_random.NextDouble() - 0.5) * 100);
                 Vector2 spawnPos = clusterCenter + offset;

                 if (!IsValidSpawnPosition(spawnPos))
                 {
                     spawnPos = clusterCenter;
                 }

                 var hobo = new Hobo(spawnPos, new Point(32, 32), _audioService, _pathFinding,
                     _collisionManager, _repathScheduler);
                 ApplyScaling(hobo, difficultyMultiplier, speedMultiplier); // Apply extra speed
                 _gameObjectManager.AddCharacter(hobo);
             }


        }

        private void SpawnEnemy(double progress)
        {
            // Determine type of enemy
            // As time progresses, higher chance for stronger enemies (BikeThief vs Hobo)

            bool spawnHobo = _random.NextDouble() > (0.2 + 0.3 * progress); // Chance of BikeThief/ Dog increases from 20% to 50%

            Vector2 spawnPos = GetRandomSpawnPosition();

            // Difficulty scaling
            // Health and Damage multiplier: 1.0 to 3.0 over 15 mins
            float difficultyMultiplier = 1.0f + (2.0f * (float)progress);
            // Speed scaling: 1.0 to 1.5 over 15 mins
            float speedMultiplier = 1.0f + (0.5f * (float)progress);

            if (spawnHobo)
            {
                var hobo = new Hobo(spawnPos, new Point(32, 32), _audioService, _pathFinding, _collisionManager,  _repathScheduler);
                ApplyScaling(hobo, difficultyMultiplier, speedMultiplier);
                _gameObjectManager.AddCharacter(hobo);
            }
            else
            {
                // Remaining probability split between Dog, Thief, and Kamikaze
                double val = _random.NextDouble();
                // Dog: 40% of remaining
                // Thief: 40% of remaining
                // Kamikaze: 20% of remaining

                if (val < 0.4)
                {
                    var dog = new Dog(spawnPos, new Point(32, 32), _audioService, _pathFinding, _collisionManager, _repathScheduler);
                    ApplyScaling(dog, difficultyMultiplier, speedMultiplier);
                    _gameObjectManager.AddCharacter(dog);
                }
                else if (val < 0.8)
                {
                    var thief = new BikeThief(spawnPos, new Point(32, 32), _audioService, _pathFinding, _collisionManager, _repathScheduler);
                    ApplyScaling(thief, difficultyMultiplier, speedMultiplier);
                    _gameObjectManager.AddCharacter(thief);
                }
                else
                {
                    var kamikaze = new KamikazeOpa(spawnPos, new Point(32, 32), _audioService, _pathFinding, _collisionManager, _gameObjectManager, _repathScheduler);
                    ApplyScaling(kamikaze, difficultyMultiplier, speedMultiplier);
                    _gameObjectManager.AddCharacter(kamikaze);
                }
            }
        }

        private void ApplyScaling(CharacterBase character, float difficultyMultiplier, float speedMultiplier)
        {
            character.Attributes.MaxHealth = (int)(character.Attributes.MaxHealth * difficultyMultiplier);
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

        private void SpawnCircle(double progress)
        {
            int count = 12 + (int)(progress * 10); // 12 to 22 enemies
            float radius = 300f;
            float angleStep = (float)(Math.PI * 2 / count);
            float difficultyMultiplier = 1.0f + (2.0f * (float)progress);
            float speedMultiplier = 1.0f + (0.5f * (float)progress);

            if (_gameObjectManager.Player1 == null) return;
            Vector2 center = _gameObjectManager.Player1.Transform.Position;

            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep;
                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                if (!IsValidSpawnPosition(pos)) continue;

                // Alternate between Hobo and BikeThief
                CharacterBase enemy = new Hobo(pos, new Point(32, 32), _audioService, _pathFinding, _collisionManager, _repathScheduler);

                ApplyScaling(enemy, difficultyMultiplier, speedMultiplier);
                _gameObjectManager.AddCharacter(enemy);
            }
        }

        private Vector2 GetRandomSpawnPosition()
        {
            if (_gameObjectManager.Player1 == null) return Vector2.Zero;

            Vector2 playerPos = _gameObjectManager.Player1.Transform.Position;

            for (int i = 0; i < 20; i++) // Try 20 times to find a valid position
            {
                // Random angle
                float angle = (float)(_random.NextDouble() * Math.PI * 2);
                // Random distance
                float distance = MIN_SPAWN_RADIUS + (float)(_random.NextDouble() * (MAX_SPAWN_RADIUS - MIN_SPAWN_RADIUS));

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
    }
}
