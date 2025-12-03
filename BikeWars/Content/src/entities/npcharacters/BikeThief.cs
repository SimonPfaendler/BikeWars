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
    public class BikeThief : CharacterBase, IWorldAudioAware
    {
        private readonly AudioService _audio;
        private WorldAudioManager _worldAudioManager;
        private EnemyMovement movement { get; set; }

        public EnemyMovement Movement => movement;

        // Animation mit SpriteManager
        private Texture2D _characterAtlas;

        private SpriteAnimation _idleAnimation;
        private SpriteAnimation _walkLeftAnimation;
        private SpriteAnimation _walkRightAnimation;

        private SpriteAnimation _currentAnimation;

        public override void LoadContent(ContentManager content)
        {
            // Atlas laden (Pfad ggf. anpassen)
            _characterAtlas = content.Load<Texture2D>("assets/sprites/characters/character_atlas");

            // IDLE
            var idleFrames = SpriteFrameDictionary.GetFrames("BikeThief_Idle");
            _idleAnimation = new SpriteAnimation(_characterAtlas, idleFrames, 0.6f);


            // e2_bikethief_walking_left.png
            var leftFrames = SpriteFrameDictionary.GetFrames("BikeThief_WalkLeft");
            _walkLeftAnimation = new SpriteAnimation(_characterAtlas, leftFrames, 0.15f);

            // e2_bikethief_walking_right.png
            var rightFrames = SpriteFrameDictionary.GetFrames("BikeThief_WalkRight");
            _walkRightAnimation = new SpriteAnimation(_characterAtlas, rightFrames, 0.15f);

            // Startzustand: Idle
            _currentAnimation = _idleAnimation;

            // sounds
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

        public BikeThief(Vector2 start, Point size, AudioService audio)
        {
            // Werte kannst du anpassen, wenn der BikeThief z.B. stärker/schneller sein soll
            _audio = audio;
            MaxHealth = 40;
            Health = MaxHealth;
            AttackDamage = 5;
            AttackCooldown = 2f;
            Transform = new Transform(start, size);
            LastTransform = new Transform(start, size);
            Speed = 100f;
            movement = new EnemyMovement(canMove: true, isMoving: false);
            UpdateCollider();
        }

        public override void Update(GameTime gameTime)
        {
            movement.HandleMovement(gameTime);

            UpdateAttackCooldown(gameTime);

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Sound-Control
            bool thiefIsMoving = movement.Direction != Vector2.Zero && movement.CanMove;

            if (thiefIsMoving && CanPlaySound())
            {
                _audio.Sounds.ResumeLoop(AudioAssets.Walking);
            }
            else
            {
                _audio.Sounds.PauseLoop(AudioAssets.Walking);
            }

            Vector2 direction = movement.Direction;
            LastTransform = new Transform(
                new Vector2(Transform.Position.X - direction.X, Transform.Position.Y - direction.Y),
                Transform.Size
            );

            bool isMoving = direction != Vector2.Zero;

            if (isMoving)
            {
                direction.Normalize();
                Transform.Position += direction * Speed * delta;

                // Animation anhand der horizontalen Richtung wählen
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
                    // bewegt sich nur hoch/runter → vorerst Idle
                    _currentAnimation = _idleAnimation;
                }
            }
            else
            {
                _currentAnimation = _idleAnimation;
            }

            // SpriteAnimation updaten (setzt intern FrameIndex usw.)
            if (_currentAnimation != null)
            {
                _currentAnimation.Update(gameTime, isMoving);
            }

            UpdateCollider();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead)
                return;
            if (_currentAnimation == null)
                return;

            _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size, 0f);
        }
        
        public void SetWorldAudioManager(WorldAudioManager manager)
        {
            _worldAudioManager = manager;
        }
        
        private bool CanPlaySound()
        {
            return _worldAudioManager != null &&
                   _worldAudioManager.IsAudible(Transform.Position);
        }

        public void Immobalize(bool value)
        {
            if (value)
            {
                movement.CanMove = false;
            }
            else
            {
                movement.CanMove = true;
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
