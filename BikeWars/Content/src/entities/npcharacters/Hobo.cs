using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using BikeWars.Content.utils;

namespace BikeWars.Entities.Characters
{
    public class Hobo: CharacterBase, IWorldAudioAware
    {
        // Animation mit SpriteManager
        private Texture2D _characterAtlas;

        private SpriteAnimation _idleAnimation;
        private SpriteAnimation _walkLeftAnimation;
        private SpriteAnimation _walkRightAnimation;
        private SpriteAnimation _walkUpAnimation;
        private SpriteAnimation _walkDownAnimation;
        private SpriteAnimation _currentAnimation;
        
        protected override string WalkingSound => AudioAssets.Walking;

        public override void LoadContent(ContentManager content)
        {
            // Atlas laden
            _characterAtlas = content.Load<Texture2D>("assets/sprites/characters/character_atlas");

            // idle
            // e1_drunkdude_standing.png
            var idleFrames = SpriteFrameDictionary.GetFrames("Hobo_Idle");
            _idleAnimation = new SpriteAnimation(_characterAtlas, idleFrames, 0.4f);

            // e1_drunkdude_walking_left.png
            var leftFrames = SpriteFrameDictionary.GetFrames("Hobo_WalkLeft");
            _walkLeftAnimation = new SpriteAnimation(_characterAtlas, leftFrames, 0.15f);

            // e1_drunkdude_walking_right.png
            var rightFrames = SpriteFrameDictionary.GetFrames("Hobo_WalkRight");
            _walkRightAnimation = new SpriteAnimation(_characterAtlas, rightFrames, 0.15f);

            // e1_drunkdude_walking_down.png
            var downFrames = SpriteFrameDictionary.GetFrames("Hobo_WalkDown");
            _walkDownAnimation = new SpriteAnimation(_characterAtlas, downFrames, 0.15f);

            // e1_drunkdude_walking_up.png
            var upFrames = SpriteFrameDictionary.GetFrames("Hobo_WalkUp");
            _walkUpAnimation = new SpriteAnimation(_characterAtlas, upFrames, 0.15f);

            // Startzustand: Idle
            _currentAnimation = _idleAnimation;
            
        }

        public override void UpdateCollider()
        {
            Vector2 colliderPosition = new Vector2(
                Transform.Position.X - Transform.Size.X / 2f,
                Transform.Position.Y - Transform.Size.Y / 2f
            );
    
            Collider = new BoxCollider(
                colliderPosition, 
                Transform.Size.X, 
                Transform.Size.Y, 
                CollisionLayer.CHARACTER, 
                this
            );
        }

        // 1x1 Texture to represent the enemy
        public static Texture2D pixel;


        public Hobo(Vector2 start, Point size, AudioService audio)
        {
            _audio = audio;
            MaxHealth = 40;
            Health = MaxHealth;
            AttackDamage = 5;
            AttackCooldown = 2f;
            Transform = new Transform(start, size);
            LastTransform = new Transform(start, size);
            Speed = 100f;
            Movement = new EnemyMovement(canMove: true, isMoving: false);
            UpdateCollider();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateAttackCooldown(gameTime);
            LastTransform = new Transform(Transform.Position, Transform.Size);
            // Sound- and Movement-Control
            Movement.HandleMovement(gameTime);
            HandleSound(Movement.IsMoving);

            Vector2 direction = Movement.Direction;
            //bool isMoving = direction != Vector2.Zero;

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
            if (value) {
                Movement.CanMove = false;
            } else
            {
                Movement.CanMove = true;
            }
        }
        public override void Attack(ICombat target)
        {
            if (!CanAttack()) return;
            target.TakeDamage(AttackDamage);
            ResetAttackCooldown();
            _audio.Sounds.Play(AudioAssets.Punch);
        }
    }
}