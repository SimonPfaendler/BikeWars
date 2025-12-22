using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;

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

        // 1x1 Texture to represent the enemy
        public static Texture2D pixel;

        // 1x1 Texture to represent the enemy
        public BikeThief(Vector2 start, Point size, AudioService audio, PathFinding pathFinding,
            CollisionManager collisionManager)
        {
            // Werte kannst du anpassen, wenn der BikeThief z.B. stärker/schneller sein soll
            _audio = audio;
            _pathFinding = pathFinding;
            _collisionManager = collisionManager;

            Attributes = new CharacterAttributes(this, 40, 0, 5, 2f, false);
            Transform = new Transform(start, size);
            LastTransform = new Transform(start, size);
            Speed = 165f;
            Movement = new EnemyMovement(canMove: true, isMoving: false, pathFinding: _pathFinding,
                gridMapper: _collisionManager);
            _idleAnimation = SpriteManager.GetAnimation("BikeThief_Idle");
            _walkLeftAnimation = SpriteManager.GetAnimation("BikeThief_WalkLeft");
            _walkRightAnimation = SpriteManager.GetAnimation("BikeThief_WalkRight");
            _currentAnimation = _idleAnimation;
            UpdateCollider();
        }

        public override void Update(GameTime gameTime)
        {
            Movement.HandleMovement(gameTime);

            UpdateAttackCooldown(gameTime);
            UpdateKnockback(gameTime);
            UpdateHitFlash(gameTime);
            // Sound-Control
            HandleSound(Movement.IsMoving);

            Vector2 direction = Movement.Direction;
            LastTransform = new Transform(
                new Vector2(Transform.Position.X - direction.X, Transform.Position.Y - direction.Y),
                Transform.Size
            );

            if (Movement.IsMoving)
            {
                float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
                direction.Normalize();
                Transform.Position += direction * Speed * delta;

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
            _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size, 0f, drawColor);
        }

        public void SetWorldAudioManager(WorldAudioManager manager)
        {
            _worldAudioManager = manager;
        }

        public void Immobalize(bool value)
        {
            if (value)
            {
                Movement.CanMove = false;
            }
            else
            {
                Movement.CanMove = true;
            }
        }
        public override void Attack(ICombat target)
        {
            if (!CanAttack()) return;
            base.Attack(target);
            _audio.Sounds.Play(AudioAssets.Punch);
        }
    }
}
