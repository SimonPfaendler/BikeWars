using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using System.Collections.Generic;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.entities.Inventory;
using BikeWars.Content.managers;
using System.Diagnostics.Metrics;

// ============================================================
// Player.cs
//
//
// Description:
// Represents the player character in the game, handling movement, animation, and sound effects.
// ============================================================
namespace BikeWars.Entities.Characters
{
    public class Player : CharacterBase, IWorldAudioAware
    {
        public Inventory Inventory { get; private set; }
        private PlayerMovement movement { get; set; }
        private CooldownWithDuration sprint { get; }

        public Vector2 GazeDirection { get; private set; }
        public Vector2 AimTarget { get; private set; }
        private Vector2 _facingDirection = Vector2.UnitX; // Default to Right to match initial animation
        
        public TerrainCollider CurrentTerrain { get; set; }
        public float TerrainSpeedMultiplier = 1.0f;
        // public bool IsGodMode { get; set; }

        public event Action ShotBullet;

        private SpriteAnimation _walkDownAnimation;
        private SpriteAnimation _walkUpAnimation;
        private SpriteAnimation _walkLeftAnimation;
        private SpriteAnimation _walkRightAnimation;

        private SpriteAnimation _currentAnimation;

        private readonly AudioService _audio;
        private WorldAudioManager _worldAudioManager;
        private string _currentMovementSound = null;


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

        public override void LoadContent(ContentManager content)
        {
            if (pixel == null)
            {
                pixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
                pixel.SetData(new[] { Color.White });
            }

            _walkDownAnimation = SpriteManager.GetAnimation("Character1_WalkDown");
            _walkLeftAnimation = SpriteManager.GetAnimation("Character1_WalkLeft");
            _walkRightAnimation = SpriteManager.GetAnimation("Character1_WalkRight");
            _walkUpAnimation = SpriteManager.GetAnimation("Character1_WalkUp");

            _currentAnimation = _walkRightAnimation;
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
                CollisionLayer.PLAYER, 
                this
            );
        }

        // 1x1 Texture to represent the player
        public static Texture2D pixel;

        private void Shooting()
        {
            // Only shoot if we have a valid gaze direction
            if (GazeDirection != Vector2.Zero)
            {
                ShotBullet?.Invoke();

                _audio.Sounds.Play(AudioAssets.GunShot);
            }
        }

        public Player(Vector2 start, Point size, AudioService audio)
        {
            MaxHealth = 300;
            Health = MaxHealth;
            AttackDamage = 10;
            AttackCooldown = 2f;
            Transform = new Transform(start, size);
            LastTransform = new Transform(start, size);
            Speed = 200f;
            SprintSpeed = 350f;
            movement = new PlayerMovement(canMove: true, isMoving: false);
            sprint = new CooldownWithDuration(1f, 5f);
            Inventory = new Inventory();
            _audio = audio;
            UpdateCollider();
        }

        public override void Update(GameTime gameTime)
        {
            Update(gameTime, AimTarget);
        }

        public void Update(GameTime gameTime, Vector2 mousePos)
        {
            UpdateAttackCooldown(gameTime);

            // TODO THIS IS NOW ONLY FOR TESTING AND SHOWING
            if (InputHandler.IsPressed(GameAction.SWITCH))
            {
                if (movement.CurrentMovement.GetType() == typeof(BicycleMovement))
                {
                    movement.CurrentMovement = new WalkingMovement(true, true);
                    TerrainSpeedMultiplier = 1.0f;
                } else
                {
                    movement.CurrentMovement = new BicycleMovement(true, true, movement.RotationAcceleration);
                }
            }

            if (InputHandler.IsPressed(GameAction.SHOOT))
            {
                Shooting();
            }
            movement.Update();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Sound-Control
            string desiredSound = movement.CurrentMovement is BicycleMovement
                ? AudioAssets.Driving
                : AudioAssets.Walking;

            float speedThreshold = 5.0f;
            bool hasSpeed = movement.CurrentMovement.Speed > speedThreshold;
            bool isInputMoving = movement.IsMoving();

            bool shouldPlaySound = hasSpeed && isInputMoving;

            if (!shouldPlaySound)
            {
                if (_currentMovementSound != null)
                {
                    _audio.Sounds.StopLoop(_currentMovementSound);
                    _currentMovementSound = null;
                }
            }
            else
            {
                if (_currentMovementSound != desiredSound)
                {
                    if (_currentMovementSound != null)
                        _audio.Sounds.StopLoop(_currentMovementSound);

                    _audio.Sounds.PlayLoop(desiredSound);
                    _currentMovementSound = desiredSound;
                }
            }

            // Sprinting Logic
            sprint.Update(gameTime);
            if (InputHandler.IsHeld(GameAction.SPRINT) && sprint.Ready)
            {
                sprint.Activate();
            }

            CurrentSpeed = sprint.IsActive ? SprintSpeed : movement.CurrentMovement.Speed;
            Vector2 direction = movement.CurrentMovement.Direction;

            if (Transform.Position.X != LastTransform.Position.X || Transform.Position.Y != LastTransform.Position.Y)
                LastTransform = new Transform(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size);

            TerrainSpeedMultiplier = GetTerrainMultiplier();


            bool isMoving = movement.IsMoving();
            if (isMoving)
            {
                direction.Normalize();
                _facingDirection = direction; // Update facing direction
                if (sprint.IsActive)
                {
                    Transform.Position += direction * CurrentSpeed * delta * TerrainSpeedMultiplier;
                }
                else
                {
                    Transform.Position += direction * movement.CurrentMovement.Speed * delta * TerrainSpeedMultiplier;
                }
                _currentAnimation = _walkUpAnimation;
                // choose animation based on main direction
                // @TODO We need to decide on how the animation should look like so don't delete it now
                if (movement.CurrentMovement.GetType() == typeof(WalkingMovement)) // THIS IS ONLY INSERTED TO SHOW. BUT NOT GOOD!
                {
                    if (MathF.Abs(direction.X) > MathF.Abs(direction.Y))
                {
                    _currentAnimation = (direction.X > 0) ? _walkRightAnimation : _walkLeftAnimation;
                }
                else
                {
                    _currentAnimation = (direction.Y > 0) ? _walkDownAnimation : _walkUpAnimation;
                }
                }
            }

            // Gaze Direction Logic
            Vector2 eyePos = Transform.Position;
            Vector2 potentialGaze = Vector2.Zero;

            // 1. Check Controller Input (Right Stick)
            Vector2 rightStick = InputHandler.GamePad.RightStick;
            if (rightStick != Vector2.Zero)
            {
                // Controller aiming
                rightStick.Y *= -1; // Invert Y for correct screen space direction

                potentialGaze = Vector2.Normalize(rightStick);
                AimTarget = eyePos + potentialGaze * 100f; // Visual target for line
            }
            else
            {
                // 2. Fallback to Mouse Input
                AimTarget = mousePos;
                Vector2 diff = AimTarget - eyePos;
                if (diff != Vector2.Zero)
                {
                    potentialGaze = Vector2.Normalize(diff);
                }
            }

            // 3. Apply Angle Restriction
            if (potentialGaze != Vector2.Zero)
            {
                // Check if the angle is within +/- 90 degrees (Dot product > 0)
                if (Vector2.Dot(_facingDirection, potentialGaze) > 0)
                {
                    GazeDirection = potentialGaze;
                }
                else
                {
                    // Clamp to nearest 90 degree angle
                    Vector2 perp1 = new Vector2(-_facingDirection.Y, _facingDirection.X); // -90 degrees
                    Vector2 perp2 = new Vector2(_facingDirection.Y, -_facingDirection.X); // +90 degrees

                    if (Vector2.Dot(perp1, potentialGaze) > Vector2.Dot(perp2, potentialGaze))
                    {
                        GazeDirection = perp1;
                    }
                    else
                    {
                        GazeDirection = perp2;
                    }
                }
            }
            else
            {
                GazeDirection = Vector2.Zero;
            }

            _currentAnimation?.Update(gameTime, isMoving);

            if (movement.CurrentMovement.IsMoving && sprint.IsActive && _currentAnimation != null)
            {
                _ghostSpawnTimer -= delta;
                if (_ghostSpawnTimer <= 0f)
                {
                    _ghostSpawnTimer = GhostSpawnInterval;

                    Rectangle source = _currentAnimation.GetCurrentFrame();

                    _ghostTrail.Add(new GhostFrame
                    {
                        Texture = SpriteManager.GetCharacterAtlas(),
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
        public override void Draw(SpriteBatch spriteBatch)
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
                    color: Color.White * alpha,
                    rotation: movement.Rotation,
                    origin: new Vector2(ghost.Source.Width / 2f, ghost.Source.Height / 2f),
                    effects: SpriteEffects.None,
                    layerDepth: 0f
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

            if (movement.CurrentMovement.GetType() == typeof(WalkingMovement)) // TODO THIS IS ONLY INSERTED TO SHOW. BUT NOT GOOD!
            {
                _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size, movement.CurrentMovement.Rotation);
            } else
            {
                _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size, movement.CurrentMovement.Rotation + MathHelper.PiOver2);
            }

            // Draw line from eye position only if GazeDirection is valid (non-zero)
            if (GazeDirection != Vector2.Zero)
            {
                Vector2 center = Transform.Position;

                // Draw static valid zone arc based on facing direction
                float facingAngle = (float)Math.Atan2(_facingDirection.Y, _facingDirection.X);
                DrawArc(spriteBatch, center, 50f, facingAngle, MathHelper.Pi, Color.Red * 0.5f); // Semi-transparent red for zone

                // Draw aiming line
                Vector2 aimEnd = center + GazeDirection * 50f;
                DrawLine(spriteBatch, center, aimEnd, Color.Red);
            }
        }

        // Is Helpful for example with colliders to set the original position back.
        public override void SetLastTransform()
        {
            Transform = new Transform(new Vector2(LastTransform.Position.X, LastTransform.Position.Y), LastTransform.Size);
        }

        public void Immobalize(bool value)
        {
            if (value)
            {
                movement.CurrentMovement.CanMove = false;
            }
            else
            {
                movement.CurrentMovement.CanMove = true;
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

        private float GetTerrainMultiplier()
        {
            if (movement.CurrentMovement.GetType() == typeof(BicycleMovement))
            {
                if (CurrentTerrain == null)
                    return 1.0f;

                switch (CurrentTerrain.TerrainType)
                {
                    case TerrainType.ROAD:
                        return 1.10f;

                    case TerrainType.GRASS:
                        return 0.90f;

                    default:
                        return 1.0f;
                }
            }
            else
            {
                return 1f;
            }
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

        public void SetWorldAudioManager(WorldAudioManager manager)
        {
            _worldAudioManager = manager;
        }

        private void DrawArc(SpriteBatch spriteBatch, Vector2 center, float radius, float angle, float sweep, Color color, int segments = 16)
        {
            float startAngle = angle - sweep / 2f;
            float step = sweep / segments;

            for (int i = 0; i < segments; i++)
            {
                float theta1 = startAngle + i * step;
                float theta2 = startAngle + (i + 1) * step;

                Vector2 p1 = center + new Vector2((float)Math.Cos(theta1), (float)Math.Sin(theta1)) * radius;
                Vector2 p2 = center + new Vector2((float)Math.Cos(theta2), (float)Math.Sin(theta2)) * radius;

                DrawLine(spriteBatch, p1, p2, color);
            }
        }
    }
}