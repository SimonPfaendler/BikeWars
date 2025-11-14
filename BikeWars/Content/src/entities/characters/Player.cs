using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework.Audio;
using BikeWars.Content.entities.interfaces;
// ============================================================
// Player.cs
//
//
// Description:
// Represents the player character in the game, handling movement, animation, and sound effects.
// ============================================================
namespace BikeWars.Entities.Characters
{
    public class Player: CharacterBase
    {
        public Color Tint = Color.Black;
        private BoxCollider _collider { get; set; }
        private PlayerMovement movement { get; set; }
        public SoundHandler SoundHandler { get; }

        private Texture2D texUp, texDown, texLeft, texRight;
        private Texture2D currentTex;

        // Animation
        private const int FrameCount = 2;          // frames per sprite
        private const float SecondsPerFrame = 0.16f;
        private float _frameTimer = 0f;
        private int _frameIndex = 0;

        public void LoadContent(ContentManager content, SoundEffect drivingSoundEffect)
        {
            // sprites
            texRight = content.Load<Texture2D>("assets/sprites/character1/c1_move_right_1x2");
            texLeft  = content.Load<Texture2D>("assets/sprites/character1/c1_move_left_1x2");
            texUp    = content.Load<Texture2D>("assets/sprites/character1/c1_move_up_1x2");
            texDown  = content.Load<Texture2D>("assets/sprites/character1/c1_move_down_1x2");

            // spawn sprite
            currentTex = texRight;

            // sounds
            SoundHandler.DrivingSoundInstance = drivingSoundEffect.CreateInstance();
            SoundHandler.DrivingSoundInstance.IsLooped = true;
        }

        public void UpdateCollider()
        {
            _collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y);
        }

        // 1x1 Texture to represent the player
        public static Texture2D pixel;

        public Player(Vector2 start, Point size)
        {
            Transform = new Transform(start, size);
            LastTransform = new Transform(start, size);
            Speed = 200f;
            movement = new PlayerMovement(canMove: true, isMoving: false);
            SoundHandler = new SoundHandler();
            UpdateCollider();
        }
        public override bool Intersects(ICollider collider)
        {
            return _collider.Intersects(collider);
        }

        public override void Update(GameTime gameTime)
        {
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

            LastTransform = new Transform(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size);
            Vector2 direction = movement.Direction;
            if (direction == Vector2.Zero)
            {
                // show first frane
                _frameIndex = 0;
                _frameTimer = 0f;
            }
            else
            {
                direction.Normalize();
                Transform.Position += direction * Speed * delta;

                // choose sprite
                if (MathF.Abs(direction.X) > MathF.Abs(direction.Y))
                    currentTex = (direction.X > 0) ? texRight : texLeft;
                else
                    currentTex = (direction.Y > 0) ? texDown : texUp;

                // Animation
                _frameTimer += delta;
                if (_frameTimer >= SecondsPerFrame)
                {
                    _frameTimer -= SecondsPerFrame;
                    _frameIndex = (_frameIndex + 1) % FrameCount;
                }
            }
            UpdateCollider();
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (currentTex == null) return;

            // 1 Spalte, 2 Zeilen -> volle Breite, halbe Höhe
            int frameWidth  = currentTex.Width;
            int frameHeight = currentTex.Height / FrameCount;

            // saubere Ganzzahl-Position, sonst „zittert“ Pixelart
            var dest = new Rectangle(
                (int)MathF.Round(Transform.Position.X),
                (int)MathF.Round(Transform.Position.Y),
                Transform.Size.X,
                Transform.Size.Y
            );

            // VERTIKAL zuschneiden: x=0, y=frameIndex * frameHeight
            var source = new Rectangle(0, _frameIndex * frameHeight, frameWidth, frameHeight);

            spriteBatch.Draw(currentTex, destinationRectangle: dest, sourceRectangle: source, color: Color.White);
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