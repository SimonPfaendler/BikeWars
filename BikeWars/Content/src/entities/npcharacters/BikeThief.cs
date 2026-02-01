using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using BikeWars.Utilities;

namespace BikeWars.Entities.Characters
{
    public class BikeThief : CharacterBase, IWorldAudioAware
    {
        private readonly SpriteAnimation _idleAnimation;
        private readonly SpriteAnimation _walkLeftAnimation;
        private readonly SpriteAnimation _walkRightAnimation;
        private SpriteAnimation _currentAnimation;
        protected override string WalkingSound => AudioAssets.Walking;

        private readonly PathFinding _pathFinding;
        private readonly CollisionManager _collisionManager;
        private readonly RepathScheduler _repathScheduler;
        private float _talkTimer = 0f;
        private const float TALK_INTERVAL = 7.5f;

        private static readonly string[] TalkSounds = {
            AudioAssets.BikeThiefLaugh,
            AudioAssets.BikeThiefTalk,
        };

        public BikeThief(Vector2 start, float size, AudioService audio, PathFinding pathFinding,
            CollisionManager collisionManager, RepathScheduler repathScheduler)
        {
            // Werte kannst du anpassen, wenn der BikeThief z.B. stärker/schneller sein soll
            _audio = audio;
            _pathFinding = pathFinding;
            _collisionManager = collisionManager;
            _repathScheduler =  repathScheduler;

            Attributes = new CharacterAttributes(this, 40, 0, 5, 2f, false);
            Transform = new Transform(start, size);
            RenderTransform = new Transform(start, new Point(32, 32));
            Speed = 125f;
            Movement = new EnemyMovement(canMove: true, isMoving: false, pathFinding: _pathFinding,
                gridMapper: _collisionManager, repathScheduler: _repathScheduler);
            _idleAnimation = SpriteManager.GetAnimation("BikeThief_Idle");
            _walkLeftAnimation = SpriteManager.GetAnimation("BikeThief_WalkLeft");
            _walkRightAnimation = SpriteManager.GetAnimation("BikeThief_WalkRight");
            _currentAnimation = _idleAnimation;
            UpdateCollider();
        }

        public override void Update(GameTime gameTime)
        {
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

            _talkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_talkTimer >= TALK_INTERVAL)
            {
                _talkTimer = 0f;

                PlayTalkWithWorldAudio();
            }

            UpdateAttackCooldown(gameTime);
            UpdateKnockback(gameTime);
            UpdateHitFlash(gameTime);
            // Sound-Control
            HandleSound(Movement.IsMoving);

            Vector2 direction = Movement.Direction;

            if (Movement.IsMoving)
            {
                float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (direction.LengthSquared() > 0.0001f)
                {
                    direction.Normalize();
                    Transform.Position += direction * Speed * delta;
                }

                if (direction.X > 0.1f)
                {
                    _currentAnimation = _walkRightAnimation;
                }
                else if (direction.X < -0.1f)
                {
                    _currentAnimation = _walkLeftAnimation;
                }
                else
                {
                    _currentAnimation = _idleAnimation;
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
            if (IsDead)
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
