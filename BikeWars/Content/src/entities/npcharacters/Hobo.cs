using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework.Audio;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers; // SpriteAnimation

namespace BikeWars.Entities.Characters
{
    public class Hobo: CharacterBase, ICombat
    {

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int AttackDamage { get; set; }
        public float AttackSpeed { get; set; }
        public bool IsDead => Health <= 0;

        private BoxCollider _collider { get; set; }
        private EnemyMovement movement { get; set; }
        public SoundHandler SoundHandler { get; }
        
        public EnemyMovement Movement => movement;

        // Animation mit SpriteManager
        private Texture2D _characterAtlas;

        private SpriteAnimation _idleAnimation;
        private SpriteAnimation _walkLeftAnimation;
        private SpriteAnimation _walkRightAnimation;

        private SpriteAnimation _currentAnimation;

        public void LoadContent(ContentManager content, SoundEffect walkingSoundEffect)
        {
            // Atlas laden
            _characterAtlas = content.Load<Texture2D>("assets/sprites/characters/character_atlas");

            // idle
            // e1_drunkdude_standing.png "frame": {"x":0,"y":0,"w":40,"h":50}
            var idleFrames = new List<Rectangle>
            {
                new Rectangle(0, 0, 40, 50)
            };
            _idleAnimation = new SpriteAnimation(_characterAtlas, idleFrames, 0.4f);

            // e1_drunkdude_walking_left.png 

            int leftBaseX = 64;
            int leftBaseY = 128;
            int leftW = 96;
            int leftH = 127;
            int leftFrameW = leftW / 2;   
            int leftFrameH = leftH / 2;       

            var leftFrames = new List<Rectangle>
            {
                new Rectangle(leftBaseX + 0 * leftFrameW, leftBaseY, leftFrameW, leftFrameH),
                new Rectangle(leftBaseX + 1 * leftFrameW, leftBaseY, leftFrameW, leftFrameH)
            };
            _walkLeftAnimation = new SpriteAnimation(_characterAtlas, leftFrames, 0.15f);


            // e1_drunkdude_walking_right.png  "frame": {"x":296,"y":0,"w":80,"h":108}

            int rightBaseX = 190;
            int rightBaseY = 0;
            int rightW = 80;
            int rightH = 108;
            int rightFrameW = rightW / 2; 
            int rightFrameH = rightH / 2;     

            var rightFrames = new List<Rectangle>
            {
                new Rectangle(rightBaseX + 0 * rightFrameW, rightBaseY, rightFrameW, rightFrameH),
                new Rectangle(rightBaseX + 1 * rightFrameW, rightBaseY, rightFrameW, rightFrameH)
            };
            _walkRightAnimation = new SpriteAnimation(_characterAtlas, rightFrames, 0.15f);

            // Startzustand: Idle
            _currentAnimation = _idleAnimation;

            // sounds
            SoundHandler.WalkingSoundInstance = walkingSoundEffect.CreateInstance();
            SoundHandler.WalkingSoundInstance.IsLooped = true;
        }

        public void UpdateCollider()
        {
            _collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y);
        }

        // 1x1 Texture to represent the enemy
        public static Texture2D pixel;


        public Hobo(Vector2 start, Point size)
        {
            MaxHealth = 40;
            Health = MaxHealth;
            AttackDamage = 5;
            AttackSpeed = 2f;
            Transform = new Transform(start, size);
            LastTransform = new Transform(start, size);
            Speed = 100f;
            movement = new EnemyMovement(canMove: true, isMoving: false);
            SoundHandler = new SoundHandler();
            UpdateCollider();
        }
        public override bool Intersects(ICollider collider)
        {
            return _collider.Intersects(collider);
        }

        public void TakeDamage(int amount)
        {
            Health -= amount;
        }

        public void Attack(ICombat target)
        {
            target.TakeDamage(AttackDamage);
        }

        public override void Update(GameTime gameTime)
        {
            movement.HandleMovement(gameTime);
            if (!movement.CanMove)
            {
                SoundHandler.WalkingSoundInstance.Stop();
                return;
            }
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Sound-Control
            if (SoundHandler.WalkingSoundInstance != null)
            {
                // Start Playing Walking Sound if Player starts moving around
                if (movement.IsMoving)
                {
                    SoundHandler.WalkingSoundInstance.Play();
                }
                // Stop Playing Walking Sound if Player stops moving around
                else
                {
                    SoundHandler.WalkingSoundInstance.Stop();
                }
            }

            LastTransform = new Transform(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size);
            Vector2 direction = movement.Direction;
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

        public void Draw(SpriteBatch spriteBatch)
        {
            if(IsDead)
                return;
            if (_currentAnimation == null)
                return;

            _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size, 0f);
        }

        public void PauseSounds()
        {
            if (SoundHandler?.WalkingSoundInstance != null &&
                SoundHandler.WalkingSoundInstance.State == SoundState.Playing)
            {
                SoundHandler.WalkingSoundInstance.Pause();
            }
        }

        public void ResumeSounds()
        {
            if (SoundHandler?.WalkingSoundInstance != null &&
                SoundHandler.WalkingSoundInstance.State == SoundState.Paused)
            {
                SoundHandler.WalkingSoundInstance.Resume();
            }
        }

        // Is Helpful for example with colliders to set the original position back.
        public void SetLastTransform()
        {
            Transform = new Transform(new Vector2(LastTransform.Position.X, LastTransform.Position.Y), LastTransform.Size);
        }

        public void Immobalize(bool value)
        {
            if (value) {
                movement.CanMove = false;
            } else
            {
                movement.CanMove = true;
            }
        }
    }
}