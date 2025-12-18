using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;

namespace BikeWars.Entities.Characters
{
    public class Hobo: CharacterBase, IWorldAudioAware
    {
        private SpriteAnimation _idleAnimation;
        private SpriteAnimation _walkLeftAnimation;
        private SpriteAnimation _walkRightAnimation;
        private SpriteAnimation _walkUpAnimation;
        private SpriteAnimation _walkDownAnimation;
        private SpriteAnimation _currentAnimation;

        private readonly PathFinding _pathFinding;
        private readonly CollisionManager _collisionManager;

        protected override string WalkingSound => AudioAssets.Walking;

        // 1x1 Texture to represent the enemy
        public static Texture2D pixel;


        public Hobo(Vector2 start, Point size, AudioService audio, PathFinding pathFinding,
            CollisionManager collisionManager)
        {
            _audio = audio;
            _pathFinding = pathFinding;
            _collisionManager = collisionManager;

            Attributes = new CharacterAttributes(this, 40, 0, 5, 2f, false);
            Transform = new Transform(start, size);
            LastTransform = new Transform(start, size);
            Speed = 100f;
            Movement = new EnemyMovement(canMove: true, isMoving: false, pathFinding: _pathFinding,
                gridMapper: _collisionManager);
            _idleAnimation = SpriteManager.GetAnimation("Hobo_Idle");
            _walkLeftAnimation = SpriteManager.GetAnimation("Hobo_WalkLeft");
            _walkRightAnimation = SpriteManager.GetAnimation("Hobo_WalkRight");
            _walkDownAnimation = SpriteManager.GetAnimation("Hobo_WalkDown");
            _walkUpAnimation = SpriteManager.GetAnimation("Hobo_WalkUp");
            _currentAnimation = _idleAnimation;
            UpdateCollider();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateAttackCooldown(gameTime);
            // Sound- and Movement-Control
            Movement.HandleMovement(gameTime);
            HandleSound(Movement.IsMoving);

            Vector2 direction = Movement.Direction;
            LastTransform = new Transform(new Vector2(Transform.Position.X - direction.X, Transform.Position.Y - direction.Y), Transform.Size);
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
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(IsDead)
                return;
            if (_currentAnimation == null)
                return;
            _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size, 0f);
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
        }
    }
}