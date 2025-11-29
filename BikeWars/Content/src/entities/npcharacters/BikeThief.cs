using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
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

            // === IDLE ===
            // TODO: Rechteck auf BikeThief-Idle-Sprite anpassen
            var idleFrames = new List<Rectangle>
            {
                new Rectangle(281, 385, 128/2, 184/3)
            };
            _idleAnimation = new SpriteAnimation(_characterAtlas, idleFrames, 0.6f);


            // e2_bikethief_walking_left.png

            int leftBaseX = 153;
            int leftBaseY = 385;
            int leftW = 128;
            int leftH = 184;
            int leftFrameW = leftW / 2;
            int leftFrameH = leftH / 3;

            var leftFrames = new List<Rectangle>
            {
                // Zeile 0
                new Rectangle(leftBaseX + 0 * leftFrameW, leftBaseY + 0 * leftFrameH, leftFrameW, leftFrameH),
                new Rectangle(leftBaseX + 1 * leftFrameW, leftBaseY + 0 * leftFrameH, leftFrameW, leftFrameH),

                // Zeile 1
                new Rectangle(leftBaseX + 0 * leftFrameW, leftBaseY + 1 * leftFrameH, leftFrameW, leftFrameH),
                new Rectangle(leftBaseX + 1 * leftFrameW, leftBaseY + 1 * leftFrameH, leftFrameW, leftFrameH),

                // Zeile 2
                new Rectangle(leftBaseX + 0 * leftFrameW, leftBaseY + 2 * leftFrameH, leftFrameW, leftFrameH),
                new Rectangle(leftBaseX + 1 * leftFrameW, leftBaseY + 2 * leftFrameH, leftFrameW, leftFrameH),
            };

            _walkLeftAnimation = new SpriteAnimation(_characterAtlas, leftFrames, 0.15f);

            // e2_bikethief_walking_right.png

            int rightBaseX = 281;
            int rightBaseY = 385;
            int rightW = 128;
            int rightH = 184;
            int rightFrameW = rightW / 2;
            int rightFrameH = rightH / 3;

            var rightFrames = new List<Rectangle>
            {
                // Zeile 0
                new Rectangle(rightBaseX + 0 * rightFrameW, rightBaseY + 0 * rightFrameH, rightFrameW, rightFrameH),
                new Rectangle(rightBaseX + 1 * rightFrameW, rightBaseY + 0 * rightFrameH, rightFrameW, rightFrameH),

                // Zeile 1
                new Rectangle(rightBaseX + 0 * rightFrameW, rightBaseY + 1 * rightFrameH, rightFrameW, rightFrameH),
                new Rectangle(rightBaseX + 1 * rightFrameW, rightBaseY + 1 * rightFrameH, rightFrameW, rightFrameH),

                // Zeile 2
                new Rectangle(rightBaseX + 0 * rightFrameW, rightBaseY + 2 * rightFrameH, rightFrameW, rightFrameH),
                new Rectangle(rightBaseX + 1 * rightFrameW, rightBaseY + 2 * rightFrameH, rightFrameW, rightFrameH),
            };

            _walkRightAnimation = new SpriteAnimation(_characterAtlas, rightFrames, 0.15f);

            // Startzustand: Idle
            _currentAnimation = _idleAnimation;

            // sounds
        }

        public override void UpdateCollider()
        {
            Collider = new BoxCollider(
                new Vector2(Transform.Position.X, Transform.Position.Y),
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

        public override void TakeDamage(int amount)
        {
            if (IsDead) return;

            Health -= amount;

            if (Health <= 0)
            {
                Health = 0;
            }
        }
        public override void Attack(ICombat target)
        {
            if (!CanAttack()) return;
            target.TakeDamage(AttackDamage);
            ResetAttackCooldown(); 
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

        // Ist hilfreich z.B. bei Kollisionen, um die ursprüngliche Position wiederherzustellen.
        public override void SetLastTransform()
        {
            Transform = new Transform(
                new Vector2(LastTransform.Position.X, LastTransform.Position.Y),
                LastTransform.Size
            );
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
    }
}
