using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using BikeWars.Content.entities.special_attacks;

namespace BikeWars.Entities.Characters
{
    public class KamikazeOpa : CharacterBase, IWorldAudioAware
    {
        private readonly SpriteAnimation _idleAnimation;
        private readonly SpriteAnimation _walkLeftAnimation;
        private readonly SpriteAnimation _walkRightAnimation;
        private readonly SpriteAnimation _walkUpAnimation;
        private readonly SpriteAnimation _walkDownAnimation;
        private SpriteAnimation _currentAnimation;

        // Constants for Kamikaze behavior
        private const float TriggerDistanceSq = 2500f; // 50^2
        private const float ExplosionRadiusSq = 25000f; // 150*150
        private const int ExplosionDamage = 50;
        private const int SelfDestructDamage = 999;
        private const float ExplosionCenteringOffset = 96f; // 192 / 2

        private readonly GameObjectManager _gameObjectManager;
        private readonly RepathScheduler _repathScheduler;

        protected override string WalkingSound => AudioAssets.Walking;

        public KamikazeOpa(Vector2 start, Point size, AudioService audio, PathFinding pathFinding,
            CollisionManager collisionManager, GameObjectManager gameObjectManager, RepathScheduler repathScheduler)
        {
            _audio = audio;
            _gameObjectManager = gameObjectManager;
            _repathScheduler = repathScheduler;

            // High speed, low health
            Attributes = new CharacterAttributes(this, 10, 0, 0, 0f, false);
            Transform = new Transform(start, size);
            // LastTransform = new Transform(start, size);
            RenderTransform = new Transform(start, new Point(32, 32));
            Speed = 200f; // Very fast

            Movement = new EnemyMovement(canMove: true, isMoving: false, pathFinding: pathFinding,
                gridMapper: collisionManager, repathScheduler: _repathScheduler);

            _walkDownAnimation = SpriteManager.GetAnimation("KamikazeOpa_BikeDown");
            _walkLeftAnimation = SpriteManager.GetAnimation("KamikazeOpa_BikeLeft");
            _walkRightAnimation = SpriteManager.GetAnimation("KamikazeOpa_BikeRight");
            _walkUpAnimation = SpriteManager.GetAnimation("KamikazeOpa_BikeUp");

            _idleAnimation = SpriteManager.GetAnimation("KamikazeOpa_BikeDown");
            _currentAnimation = _idleAnimation;

            UpdateCollider();
            SubscribeToDeath();
        }

        public override void Update(GameTime gameTime)
        {
            if (IsDead || _explosionSpawned) return;

            UpdateAttackCooldown(gameTime);
            UpdateKnockback(gameTime);
            UpdateHitFlash(gameTime);

            Movement.HandleMovement(gameTime);
            HandleSound(Movement.IsMoving);

            Vector2 direction = Movement.Direction;
            // LastTransform = new Transform(new Vector2(Transform.Position.X - direction.X, Transform.Position.Y - direction.Y), Transform.Size);

            if (Movement.IsMoving)
            {
                float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
                direction.Normalize();
                Transform.Position += direction * Speed * delta;

                if (System.Math.Abs(direction.X) > System.Math.Abs(direction.Y))
                {
                    _currentAnimation = (direction.X > 0) ? _walkRightAnimation : _walkLeftAnimation;
                }
                else
                {
                    _currentAnimation = (direction.Y > 0) ? _walkDownAnimation : _walkUpAnimation;
                }
            }
            else
            {
                _currentAnimation = _idleAnimation;
            }

            if (_currentAnimation != null)
            {
                _currentAnimation.Update(gameTime, Movement.IsMoving);
            }

            UpdateCollider();

            // Self-Destruct Trigger Logic
            if (Movement is EnemyMovement enemyMove)
            {
                float distSq = Vector2.DistanceSquared(Transform.Position, enemyMove.PlayerPosition);
                if (distSq < TriggerDistanceSq)
                {
                    TakeDamage(SelfDestructDamage);
                }
            }
        }

        private bool _explosionSpawned = false;
        private void SpawnExplosion()
        {
            if (_explosionSpawned) return;
            _explosionSpawned = true;

            // 1. Deal Damage to Player
            var player = _gameObjectManager.Player1;
            if (player != null && !player.IsDead)
            {
                float distSq = Vector2.DistanceSquared(Transform.Position, player.Transform.Position);
                if (distSq < ExplosionRadiusSq)
                {
                    player.TakeDamage(ExplosionDamage);
                }
            }

            // 2. Spawn XP / Loot (Prevent double drops)
            if (!_XpDropped)
            {
                _XpDropped = true;
                _gameObjectManager.SpawnXp(this);
            }

            // 3. Spawn Visual Explosion
            // Center the explosion on the enemy. SpriteAnimation.Draw draws centered, so we pass the center position directly.
            Vector2 explosionPos = Transform.Position;

            var explosion = new KamikazeExplosion(this, explosionPos);
            _gameObjectManager.AddAOE(explosion);

            // Screen Shake
            _gameObjectManager.RequestScreenShake(5.5f, 1f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(IsDead && _explosionSpawned) return;

            if (_currentAnimation == null) return;

            Color drawColor = (_hitFlashTimer > 0f) ? _hitColor : Color.White;
            _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size, 0f, _renderScale, drawColor);
        }

        public void SetWorldAudioManager(WorldAudioManager manager)
        {
            _worldAudioManager = manager;
        }

        private void SubscribeToDeath()
        {
            Attributes.OnDied += (c) => SpawnExplosion();
        }
    }
}
