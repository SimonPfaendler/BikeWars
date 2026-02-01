using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.entities.MapObjects;
using BikeWars.Content.managers;
using BikeWars.Utilities;

namespace BikeWars.Entities.Characters
{
    public class Dog: CharacterBase, IWorldAudioAware
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

        protected override string WalkingSound => AudioAssets.Walking;
        private float _barkTimer = 0f;
        private const float BARK_INTERVAL = 4.0f;

        private static readonly string[] BarkSounds = {
            AudioAssets.BarkBora,
            AudioAssets.BarkClemens,
            AudioAssets.BarkCarlota,
            AudioAssets.BarkGiulla,
            AudioAssets.BarkSimon,
            AudioAssets.BarkSimon2,
            AudioAssets.BarkFritz,
            AudioAssets.Miau,
            AudioAssets.BarkMadita
        };

        public Dog(Vector2 start, float size, AudioService audio, PathFinding pathFinding,
            CollisionManager collisionManager, RepathScheduler repathScheduler)
        {
            _audio = audio;
            _pathFinding = pathFinding;
            _collisionManager = collisionManager;
            _repathScheduler = repathScheduler;

            Attributes = new CharacterAttributes(this, 25, 0, 3, 2f, false);
            Transform = new Transform(start, size);
            RenderTransform = new Transform(start, new Point(32, 32));
            Speed = 135f;
            Movement = new EnemyMovement(canMove: true, isMoving: false, pathFinding: _pathFinding,
                gridMapper: _collisionManager, repathScheduler: _repathScheduler);
            _idleAnimation = SpriteManager.GetAnimation("Dog_Idle");
            _walkLeftAnimation = SpriteManager.GetAnimation("Dog_WalkLeft");
            _walkRightAnimation = SpriteManager.GetAnimation("Dog_WalkRight");
            _walkDownAnimation = SpriteManager.GetAnimation("Dog_WalkDown");
            _walkUpAnimation = SpriteManager.GetAnimation("Dog_WalkUp");
            _currentAnimation = _idleAnimation;
            UpdateCollider();
        }

        public override void Update(GameTime gameTime)
        {
            _barkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_barkTimer >= BARK_INTERVAL)
            {
                _barkTimer = 0f;

                PlayBarkWithWorldAudio();
            }

            UpdateAttackCooldown(gameTime);
            UpdateKnockback(gameTime);
            UpdateHitFlash(gameTime);
            // Sound- and Movement-Control
            if (Movement is EnemyMovement em)
            {
                em.EnemyPosition = Transform.Position;
                if (!DogBowl.BowlIsActive)
                {
                    Player? target = _collisionManager.GameObjectManager.GetTargetPlayer(Transform.Position);
                    if (target != null)
                    {
                         em.PlayerPosition = target.Transform.Position;
                    }
                }
                else
                {
                    em.PlayerPosition = DogBowl.BowlPosition;
                }
            }
            Movement.HandleMovement(gameTime);
            HandleSound(Movement.IsMoving);

            Vector2 direction = Movement.Direction;
            // LastTransform = new Transform(Transform.Position, Transform.Size);

            if (Movement.IsMoving)
            {
                float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (direction.LengthSquared() > 0.0001f)
                {
                    direction.Normalize();
                    Transform.Position += direction * Speed * delta;
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


            if (_currentAnimation != null)
            {
                _currentAnimation.Update(gameTime, Movement.IsMoving);
            }

            UpdateCollider();
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

        public override void Attack(ICombat target)
        {
            if (!CanAttack()) return;
            base.Attack(target);
            _audio.Sounds.Play(AudioAssets.Punch);
        }

        private void PlayBarkWithWorldAudio()
        {
            if (_worldAudioManager == null) {
                return;
            }

            float volume = _worldAudioManager.GetVolumeFor(Transform.Position);

            if (volume > 0)
            {
                int index = RandomUtil.NextInt(0, BarkSounds.Length);
                string randomBark = BarkSounds[index];

                _audio.Sounds.Play(randomBark);
            }
        }
    }
}