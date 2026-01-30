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

public class Dozent: CharacterBase, IWorldAudioAware
{
    private readonly SpriteAnimation _idleAnimation;
        private readonly SpriteAnimation _walkLeftAnimation;
        private readonly SpriteAnimation _walkRightAnimation;
        private readonly SpriteAnimation _walkUpAnimation;
        private readonly SpriteAnimation _walkDownAnimation;
        private SpriteAnimation _currentAnimation;

        private readonly PathFinding _pathFinding;
        private readonly CollisionManager _collisionManager;
        private readonly RepathScheduler _repathScheduler;
        private float _attackAnimationTimer = 0f;
        private const float AttackAnimationDuration = 2f;

        protected override string WalkingSound => AudioAssets.Walking;
        
        private float _talkTimer = 0f;
        private const float TALK_INTERVAL = 7.5f;
        
        private static readonly string[] TalkSounds = {
            AudioAssets.DozentTalk_1,
            AudioAssets.DozentTalk_2,
            AudioAssets.DozentTalk_3,
        };


        public Dozent(Vector2 start, float size, AudioService audio, PathFinding pathFinding,
            CollisionManager collisionManager, RepathScheduler repathScheduler)
        {
            _audio = audio;
            _pathFinding = pathFinding;
            _collisionManager = collisionManager;
            _repathScheduler = repathScheduler;

            Attributes = new CharacterAttributes(this, 300, 0, 80, 2f, false);
            Transform = new Transform(start, size);
            LastTransform = new Transform(start, size);
            RenderTransform = new Transform(start, new Point(32, 32));
            Speed = 120f;
            Movement = new EnemyMovement(canMove: true, isMoving: false, pathFinding: _pathFinding,
                gridMapper: _collisionManager, repathScheduler: _repathScheduler);
            _idleAnimation = SpriteManager.GetAnimation("Dozent_Idle");
            _walkLeftAnimation = SpriteManager.GetAnimation("Dozent_WalkLeft");
            _walkRightAnimation = SpriteManager.GetAnimation("Dozent_WalkRight");
            _walkDownAnimation = SpriteManager.GetAnimation("Dozent_WalkDown");
            _walkUpAnimation = SpriteManager.GetAnimation("Dozent_WalkUp");
            _currentAnimation = _idleAnimation;
            UpdateCollider();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateAttackCooldown(gameTime);
            UpdateKnockback(gameTime);
            UpdateHitFlash(gameTime);
            // Sound- and Movement-Control
            if (Movement is EnemyMovement em)
            {
                em.EnemyPosition = Transform.Position;
                em.PlayerPosition = _collisionManager.GameObjectManager.Player1.Transform.Position;

            }
            
            _talkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_talkTimer >= TALK_INTERVAL)
            {
                _talkTimer = 0f;

                PlayTalkWithWorldAudio();
            }
            
            Movement.HandleMovement(gameTime);
            HandleSound(Movement.IsMoving);
            LastTransform = new Transform(Transform.Position, Transform.Size);

            Vector2 direction = Movement.Direction;
            if (Movement.IsMoving)
            {
                float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (direction.LengthSquared() > 0.0001f)
                {
                    direction.Normalize();
                    Transform.Position += direction * Speed * delta;
                }

                if (_attackAnimationTimer > 0f)
                {
                    _attackAnimationTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    _currentAnimation = _idleAnimation;
                }
                else
                {

                    if (System.Math.Abs(direction.X) > System.Math.Abs(direction.Y))
                    {

                        _currentAnimation = (direction.X > 0) ? _walkRightAnimation : _walkLeftAnimation;
                    }
                    else
                    {

                        _currentAnimation = (direction.Y > 0) ? _walkDownAnimation : _walkUpAnimation;
                    }
                }


                if (_currentAnimation != null)
                {
                    _currentAnimation.Update(gameTime, Movement.IsMoving);
                }

                UpdateCollider();
            }
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
            _currentAnimation = _idleAnimation;
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
