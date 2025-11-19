using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework.Audio;
using BikeWars.Content.entities.interfaces;
using System.Collections.Generic;
using BikeWars.Content.managers;
using BikeWars.Content.screens;

// ============================================================
// Player.cs
//
//
// Description:
// Represents the player character in the game, handling movement, animation, and sound effects.
// ============================================================
namespace BikeWars.Entities.Characters
{
    public class Player : CharacterBase
    {

        private BoxCollider _collider { get; set; }
        private PlayerMovement movement { get; set; }
        public SoundHandler SoundHandler { get; }
        private CooldownWithDuration sprint { get; }

        public Vector2 GazeDirection { get; private set; }
        public Vector2 AimTarget { get; private set; }

        public event Action ShotBullet;
        private Texture2D _characterAtlas;

        private SpriteAnimation _walkDownAnimation;
        private SpriteAnimation _walkUpAnimation;
        private SpriteAnimation _walkLeftAnimation;
        private SpriteAnimation _walkRightAnimation;

        private SpriteAnimation _currentAnimation;

        private struct GhostFrame
        {
            public Texture2D Texture;
            public Vector2 Position;
            public Rectangle Source;
            public float TimeLeft;
        }

        private readonly List<GhostFrame> _ghostTrail = new List<GhostFrame>();

        private float _ghostSpawnTimer = 0f;
        private const float GhostSpawnInterval = 0.05f; // alle 0,05s ein neues Ghost
        private const float GhostLifeTime = 0.1f;

        public void LoadContent(ContentManager content, SoundEffect drivingSoundEffect)
        {
            _characterAtlas = content.Load<Texture2D>("assets/sprites/characters/character_atlas");
            if (pixel == null)
            {
                pixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
                pixel.SetData(new[] { Color.White });
            }

            // Down – c1_move_down_1x2.png: x=40, y=0, w=64, h=128
            var downFrames = new List<Rectangle>
            {
                new Rectangle(40, 0, 64, 64),
                new Rectangle(40, 64, 64, 64)
            };
            _walkDownAnimation = new SpriteAnimation(_characterAtlas, downFrames, 0.16f);

            // Left – c1_move_left_1x2.png: x=104, y=0, w=64, h=128
            var leftFrames = new List<Rectangle>
            {
                new Rectangle(104, 0, 64, 64),
                new Rectangle(104, 64, 64, 64)
            };
            _walkLeftAnimation = new SpriteAnimation(_characterAtlas, leftFrames, 0.16f);

            // Right – c1_move_right_1x2.png: x=168, y=0, w=64, h=128
            var rightFrames = new List<Rectangle>
            {
                new Rectangle(168, 0, 64, 64),
                new Rectangle(168, 64, 64, 64)
            };
            _walkRightAnimation = new SpriteAnimation(_characterAtlas, rightFrames, 0.16f);

            // Up – c1_move_up_1x2.png: x=232, y=0, w=64, h=128
            var upFrames = new List<Rectangle>
            {
                new Rectangle(232, 0, 64, 64),
                new Rectangle(232, 64, 64, 64)
            };
            _walkUpAnimation = new SpriteAnimation(_characterAtlas, upFrames, 0.16f);

            _currentAnimation = _walkRightAnimation;

            SoundHandler.DrivingSoundInstance = drivingSoundEffect.CreateInstance();
            SoundHandler.DrivingSoundInstance.IsLooped = true;
        }

        public void UpdateCollider()
        {
            _collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y);
        }

        // 1x1 Texture to represent the player
        public static Texture2D pixel;

        private void Shooting()
        {
            ShotBullet?.Invoke();
        }

        public Player(Vector2 start, Point size)
        {
            Transform = new Transform(start, size);
            LastTransform = new Transform(start, size);
            Speed = 200f;
            SprintSpeed = 350f;
            movement = new PlayerMovement(canMove: true, isMoving: false);
            SoundHandler = new SoundHandler();
            sprint = new CooldownWithDuration(1f, 5f);
            UpdateCollider();
        }
        public override bool Intersects(ICollider collider)
        {
            return _collider.Intersects(collider);
        }

        public override void Update(GameTime gameTime)
        {
            Update(gameTime, AimTarget);
        }

        public void Update(GameTime gameTime, Vector2 mousePos)
        {
            if (InputHandler.IsPressed(GameAction.SHOOT))
            {
                Shooting();
            }
            movement.HandleMovement(gameTime);
            if (!movement.CanMove)
            {
                SoundHandler.DrivingSoundInstance.Stop();
                return;
            }
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Sound-Control
            if (SoundHandler.DrivingSoundInstance != null)
            {
                // Start Playing Walking Sound if Player starts moving around
                if (movement.IsMoving)
                {
                    SoundHandler.DrivingSoundInstance.Play();
                }
                // Stop Playing Walking Sound if Player stops moving around
                else
                {
                    SoundHandler.DrivingSoundInstance.Stop();
                }
            }

            // Sprinting Logic
            sprint.Update(gameTime);
            if (InputHandler.IsHeld(GameAction.SPRINT) && sprint.Ready)
            {
                sprint.Activate();

            }

            CurrentSpeed = sprint.IsActive ? SprintSpeed : Speed;


            LastTransform = new Transform(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size);

            Vector2 direction = movement.Direction;
            bool isMoving = direction != Vector2.Zero;

            if (isMoving)
            {
                direction.Normalize();
                Transform.Position += direction * CurrentSpeed * delta;

                // choose animation based on main direction
                if (MathF.Abs(direction.X) > MathF.Abs(direction.Y))
                {
                    _currentAnimation = (direction.X > 0) ? _walkRightAnimation : _walkLeftAnimation;
                }
                else
                {
                    _currentAnimation = (direction.Y > 0) ? _walkDownAnimation : _walkUpAnimation;
                }
            }

            // Gaze Direction Logic (Mouse Only)
            Vector2 eyePos = new Vector2(Transform.Position.X + Transform.Size.X / 2f, Transform.Position.Y);
            AimTarget = mousePos;
            
            Vector2 diff = AimTarget - eyePos;
            if (diff != Vector2.Zero)
            {
                GazeDirection = Vector2.Normalize(diff);
            }

            _currentAnimation?.Update(gameTime, isMoving);

            if (movement.IsMoving && sprint.IsActive && _currentAnimation != null)
            {
                _ghostSpawnTimer -= delta;
                if (_ghostSpawnTimer <= 0f)
                {
                    _ghostSpawnTimer = GhostSpawnInterval;

                    Rectangle source = _currentAnimation.GetCurrentFrame();

                    _ghostTrail.Add(new GhostFrame
                    {
                        Texture = _characterAtlas,
                        Position = Transform.Position,
                        Source = source,
                        TimeLeft = GhostLifeTime
                    });
                }
            }


            for (int i = _ghostTrail.Count - 1; i >= 0; i--)
            {
                var ghost = _ghostTrail[i];
                ghost.TimeLeft -= delta;

                if (ghost.TimeLeft <= 0f)
                {
                    _ghostTrail.RemoveAt(i);
                }
                else
                {
                    _ghostTrail[i] = ghost;
                }
            }

            UpdateCollider();
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var ghost in _ghostTrail)
            {
                float alpha = ghost.TimeLeft / GhostLifeTime;

                var ghostDest = new Rectangle(
                    (int)MathF.Round(ghost.Position.X),
                    (int)MathF.Round(ghost.Position.Y),
                    Transform.Size.X,
                    Transform.Size.Y
                );

                spriteBatch.Draw(
                    ghost.Texture,
                    destinationRectangle: ghostDest,
                    sourceRectangle: ghost.Source,
                    color: Color.White * alpha
                );
            }
            // saubere Ganzzahl-Position, sonst „zittert“ Pixelart
            var dest = new Rectangle(
                (int)MathF.Round(Transform.Position.X),
                (int)MathF.Round(Transform.Position.Y),
                Transform.Size.X,
                Transform.Size.Y
            );

            // 2) Spieler zeichnen
            if (_currentAnimation == null)
            return;

            _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size);
            
            // Draw line from eye position
            Vector2 eyePos = new Vector2(Transform.Position.X + Transform.Size.X / 2f, Transform.Position.Y);
            DrawLine(spriteBatch, eyePos, AimTarget, Color.Red);
        }

        // Is Helpful for example with colliders to set the original position back.
        public void SetLastTransform()
        {
            Transform = new Transform(new Vector2(LastTransform.Position.X, LastTransform.Position.Y), LastTransform.Size);
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

        public bool IsSprinting()
        {
            return sprint.IsActive;
        }

        public float CooldownTimer()
        {
            return sprint.GetRemainingCooldown();
        }
        // Helper function to draw a line
        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            spriteBatch.Draw(pixel,
                new Rectangle((int)start.X, (int)start.Y, (int)length, 2), // 2 is thickness
                null,
                color,
                angle,
                Vector2.Zero,
                SpriteEffects.None,
                0);
        }
    }
}