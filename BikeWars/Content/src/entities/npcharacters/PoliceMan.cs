#nullable enable
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using BikeWars.Entities.Characters;
using BikeWars.Utilities;

namespace BikeWars.Content.entities.npcharacters;

public class PoliceMan: CharacterBase, IWorldAudioAware
{
        private readonly SpriteAnimation _walkLeftAnimation;
        private readonly SpriteAnimation _walkRightAnimation;
        private readonly SpriteAnimation _walkUpAnimation;
        private readonly SpriteAnimation _walkDownAnimation;
        private readonly SpriteAnimation _attackLeftAnimation;
        private readonly SpriteAnimation _attackRightAnimation;

        private SpriteAnimation _currentAnimation;

        private readonly PathFinding _pathFinding;
        private readonly CollisionManager _collisionManager;
        private readonly RepathScheduler _repathScheduler;
        private float _attackAnimationTimer = 0f;
        private const float AttackAnimationDuration = 1f;
        private bool IsAttacking => _attackAnimationTimer > 0f;

        private float _talkTimer = 0f;
        private const float TALK_INTERVAL = 5.0f;

        private static readonly string[] TalkSounds = {
            AudioAssets.HaltStop,
            AudioAssets.Geisterfahrer,
        };

        protected override string WalkingSound => AudioAssets.Walking;

        public PoliceMan(Vector2 start, float size, AudioService audio, PathFinding pathFinding,
            CollisionManager collisionManager, RepathScheduler repathScheduler, ITargetProvider targetProvider): base(targetProvider)
        {
            _audio = audio;
            _pathFinding = pathFinding;
            _collisionManager = collisionManager;
            _repathScheduler = repathScheduler;

            Attributes = new CharacterAttributes(this, 60, 0, 10, 2f, false);
            Transform = new Transform(start, size);
            LastTransform = new Transform(start, size);
            RenderTransform = new Transform(start, new Point(32, 32));
            Speed = 120f;
            Movement = new EnemyMovement(canMove: true, isMoving: false, pathFinding: _pathFinding,
                gridMapper: _collisionManager, repathScheduler: _repathScheduler);
            _walkLeftAnimation = SpriteManager.GetAnimation("policeman_walking_left");
            _walkRightAnimation = SpriteManager.GetAnimation("policeman_walking_right");
            _walkDownAnimation = SpriteManager.GetAnimation("policeman_walking_down");
            _walkUpAnimation = SpriteManager.GetAnimation("policeman_walking_up");
            _attackLeftAnimation = SpriteManager.GetAnimation("policeman_attack_left");
            _attackRightAnimation = SpriteManager.GetAnimation("policeman_attack_right");
            _currentAnimation = _walkDownAnimation;
            UpdateCollider(CollisionLayer.CHARACTER);
        }

        public override void Update(GameTime gameTime)
        {
            _talkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_talkTimer >= TALK_INTERVAL)
            {
                _talkTimer = 0f;

                PlayTalkWithWorldAudio();
            }

            UpdateAttackCooldown(gameTime);
            UpdateKnockback(gameTime);
            UpdateHitFlash(gameTime);
            // Sound- and Movement-Control
            if (Movement is EnemyMovement em)
            {
                em.EnemyPosition = Transform.Position;
                Player? target = _collisionManager.GameObjectManager.GetTargetPlayer(Transform.Position);
                if (target != null)
                {
                     em.PlayerPosition = target.Transform.Position;
                }
            }
            Movement.HandleMovement(gameTime);
            HandleSound(Movement.IsMoving);
            LastTransform = new Transform(Transform.Position, Transform.Size);

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Movement.IsMoving)
            {
                Vector2 direction = Movement.Direction;
                if (direction.LengthSquared() > 0.0001f)
                {
                    direction.Normalize();
                    Transform.Position += direction * Speed * delta;
                }
            }
            if (IsAttacking)
            {
                _attackAnimationTimer -= delta;
                _currentAnimation.Update(gameTime, true);
            }
            else
            {
                if (Movement.IsMoving)
                {
                    Vector2 direction = Movement.Direction;

                    if (Math.Abs(direction.X) > Math.Abs(direction.Y))
                        _currentAnimation = (direction.X > 0) ? _walkRightAnimation : _walkLeftAnimation;
                    else
                        _currentAnimation = (direction.Y > 0) ? _walkDownAnimation : _walkUpAnimation;
                }

                _currentAnimation.Update(gameTime, Movement.IsMoving);
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
            _currentAnimation.Draw(spriteBatch, RenderTransform.Position, RenderTransform.Size, 0f, _renderScale, drawColor);
        }

        public void SetWorldAudioManager(WorldAudioManager manager)
        {
            _worldAudioManager = manager;
        }

        public void Immobalize(bool value)
        {
            Movement.CanMove = !value;
        }
        public override void Attack(ICombat target)
        {
            if (!CanAttack()) return;
            base.Attack(target);
            _audio.Sounds.Play(AudioAssets.Punch);
            _attackAnimationTimer = AttackAnimationDuration;

            if (_currentAnimation == _walkRightAnimation || _currentAnimation == _walkUpAnimation)
            {
                _currentAnimation = _attackRightAnimation;
            }
            else
            {
                _currentAnimation = _attackLeftAnimation;
            }
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
