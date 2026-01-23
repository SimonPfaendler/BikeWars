using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using BikeWars.Entities.Characters;

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

        protected override string WalkingSound => AudioAssets.Walking;


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
            Speed = 130f;
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
