#nullable enable

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.entities.items;
using BikeWars.Content.managers;
using BikeWars.Utilities;

namespace BikeWars.Entities.Characters
{
    public class Hobo: CharacterBase, IWorldAudioAware
    {
        private readonly SpriteAnimation _idleAnimation;
        private readonly SpriteAnimation _walkLeftAnimation;
        private readonly SpriteAnimation _walkRightAnimation;
        private readonly SpriteAnimation _walkUpAnimation;
        private readonly SpriteAnimation _walkDownAnimation;
        private readonly SpriteAnimation _throwAnimation;
        private SpriteAnimation _currentAnimation;
        private SpriteEffects _spriteEffects = SpriteEffects.None;

        private readonly PathFinding _pathFinding;
        private readonly CollisionManager _collisionManager;
        private readonly RepathScheduler _repathScheduler;

        private float _throwAnimTimer;

        protected override string WalkingSound => AudioAssets.Walking;

        private float _talkTimer = 0f;
        private const float TALK_INTERVAL = 5.0f;

        private static readonly string[] TalkSounds = {
            AudioAssets.HoboTalk1,
            AudioAssets.HoboTalk2,
        };

        public Hobo(Vector2 start, float radius, AudioService audio, PathFinding pathFinding,
            CollisionManager collisionManager, RepathScheduler repathScheduler, ITargetProvider targetProvider): base(targetProvider)
        {
            _audio = audio;
            _pathFinding = pathFinding;
            _collisionManager = collisionManager;
            _repathScheduler = repathScheduler;

            Attributes = new CharacterAttributes(this, 40, 0, 5, 2f, false);
            Transform = new Transform(start, radius);
            LastTransform = new Transform(start, radius);
            RenderTransform = new Transform(start, new Point(32, 32));
            Speed = 100f;
            Movement = new EnemyMovement(canMove: true, isMoving: false, pathFinding: _pathFinding,
                gridMapper: _collisionManager, repathScheduler: _repathScheduler);
            _idleAnimation = SpriteManager.GetAnimation("Hobo_Idle");
            _walkLeftAnimation = SpriteManager.GetAnimation("Hobo_WalkLeft");
            _walkRightAnimation = SpriteManager.GetAnimation("Hobo_WalkRight");
            _walkDownAnimation = SpriteManager.GetAnimation("Hobo_WalkDown");
            _walkUpAnimation = SpriteManager.GetAnimation("Hobo_WalkUp");
            _throwAnimation = SpriteManager.GetAnimation("Hobo_Throw");
            _currentAnimation = _idleAnimation;
            UpdateCollider(CollisionLayer.CHARACTER);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateAttackCooldown(gameTime);
            UpdateKnockback(gameTime);
            UpdateHitFlash(gameTime);

            _talkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_talkTimer >= TALK_INTERVAL)
            {
                _talkTimer = 0f;

                PlayTalkWithWorldAudio();
            }
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Sound- and Movement-Control
            if (Movement is EnemyMovement em)
            {
                em.EnemyPosition = Transform.Position;
                if (!Beer.BeerIsActive)
                {
                   Player? target = _collisionManager.GameObjectManager.GetTargetPlayer(Transform.Position);
                   if (target != null)
                   {
                        em.PlayerPosition = target.Transform.Position;
                   }
                }
                else
                {
                    em.PlayerPosition = Beer.BeerPosition;
                }
            }
            // Movement.HandleMovement(gameTime);
            // Vector2 direction = Movement.Direction;
            Vector2 direction;
            float distSq = DistanceToClosestPlayerSq();
            if (Movement is null)
            {
                return;
            }

            if (distSq > LOD1_DIST_SQ)
            {
                // LOD2 – sehr weit weg → nur selten updaten
                if (Random.Shared.NextDouble() < 0.50)
                    return;

                direction = GetSimpleChaseDirection();
            }
            else if (distSq > LOD0_DIST_SQ)
            {
                // LOD1 – mittlere Distanz → KEIN Pathfinding
                direction = GetSimpleChaseDirection();
            }
            else
            {
                // LOD0 – nahe am Spieler → volle Pathfinding AI
                Movement.HandleMovement(gameTime);
                direction = Movement.Direction;
            }
            LastTransform = new Transform(Transform.Position, Transform.Size);
            if (Movement.IsMoving)
            {
                if (direction.LengthSquared() > 0.0001f)
                {
                    direction.Normalize();
                    Transform.Position += direction * Speed * dt;
                }

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

            if (_throwAnimTimer > 0f)
            {
                _throwAnimTimer -= dt;
                _currentAnimation = _throwAnimation;
            }
            else
            {
                _spriteEffects = SpriteEffects.None; // default for non-throw animations
            }

            if (_currentAnimation != null)
            {
                bool animating = _throwAnimTimer > 0f || Movement.IsMoving;
                _currentAnimation.Update(gameTime, animating);
            }
            HandleSound(Movement.IsMoving);

            var gom = _collisionManager?.GameObjectManager;
            if (gom != null)
            {
                // Use the new safe helper method
                Player? target = gom.GetTargetPlayer(Transform.Position);

                if (target != null)
                {
                    ThrowAttack(target);
                }
            }

            UpdateCollider(CollisionLayer.CHARACTER);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(IsDead)
                return;
            if (_currentAnimation == null)
                return;

            Color drawColor = (_hitFlashTimer > 0f) ? _hitColor : Color.White;
            SpriteEffects effects = _throwAnimTimer > 0f ? _spriteEffects : SpriteEffects.None;
            _currentAnimation.Draw(spriteBatch, RenderTransform.Position, RenderTransform.Size, 0f, _renderScale, drawColor, effects);
        }

        public void SetWorldAudioManager(WorldAudioManager manager)
        {
            _worldAudioManager = manager;
        }

        public void Immobalize(bool value)
        {
            if (Movement is null)
            {
                return;
            }
            Movement.CanMove = !value;
        }
        public override void Attack(ICombat target)
        {
            if (!CanAttack()) return;
            base.Attack(target);
            _audio.Sounds.Play(AudioAssets.Punch);
        }

        public override void ThrowAttack(ICombat target)
        {
            if (!CanAttack()) return;
            if (_collisionManager?.GameObjectManager == null) return;
            if (target is not Player player) return;

            if (Random.Shared.NextDouble() > Attributes.ThrowAttackChance) return;

            float maxRange = Attributes.ThrowRange;
            float minRange = Attributes.ThrowRange * 0.5f;
            float distSq = Vector2.DistanceSquared(Transform.Position, player.Transform.Position);
            if (distSq > maxRange * maxRange || distSq < minRange * minRange) return;

            float dx = player.Transform.Position.X - Transform.Position.X;
            if (Math.Abs(dx) > 0.05f)
            {
                _spriteEffects = dx < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }

            _collisionManager.GameObjectManager.OnEnemyThrowBottle(this, player.Transform.Position);
            _audio.Sounds.Play(AudioAssets.ThrowObject);
            _throwAnimTimer = SpriteManager.GetAnimationSpeed("Hobo_Throw") * 6; // 6 frames
            _currentAnimation = _throwAnimation;
            ResetAttackCooldown();
        }

        private void PlayTalkWithWorldAudio()
        {
            if (_worldAudioManager == null) {
                return;
            }

            float volume = _worldAudioManager.GetVolumeFor(Transform.Position);

            if (volume > 0)
            {
                int index = RandomUtil.NextInt(0, TalkSounds.Length);
                string randomTalk = TalkSounds[index];

                _audio.Sounds.Play(randomTalk);
            }
        }
    }
}