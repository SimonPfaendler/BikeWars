using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BikeWars.Components;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework.Audio;

namespace BikeWars.Entities.Characters
{
    public class Player
    {
        public Transform Transform;
        public Transform lastTransform;
        public float Speed = 200f;
        public Color Tint = Color.Black;
        private BoxCollider _collider { get; set; }
        
        private Texture2D texUp, texDown, texLeft, texRight;
        private Texture2D currentTex;
        
        // Animation
        private const int FrameCount = 2;          // frames per sprite 
        private const float SecondsPerFrame = 0.16f;
        private float _frameTimer = 0f;
        private int _frameIndex = 0;
        public SoundEffect WalkingSound { get; private set; }
        public SoundEffectInstance WalkingSoundInstance { get; private set; }
        private bool wasMoving = false;
        
        public void LoadContent(ContentManager content, SoundEffect walkingSoundEffect)
        {
            WalkingSound = walkingSoundEffect;
            WalkingSoundInstance = WalkingSound.CreateInstance();
            WalkingSoundInstance.IsLooped = true;
            
            // sprites 
            texRight = content.Load<Texture2D>("assets/sprites/character1/c1_move_right_1x2");
            texLeft  = content.Load<Texture2D>("assets/sprites/character1/c1_move_left_1x2");
            texUp    = content.Load<Texture2D>("assets/sprites/character1/c1_move_up_1x2");
            texDown  = content.Load<Texture2D>("assets/sprites/character1/c1_move_down_1x2");

            // spawn sprite
            currentTex = texRight;
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
            lastTransform = new Transform(start, size);
            UpdateCollider();
        }
        public bool Intersects(ICollider collider)
        {
            return _collider.Intersects(collider);
        }

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboardState = Keyboard.GetState();

            Vector2 direction = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.W))
                direction.Y -= 1;
            if (keyboardState.IsKeyDown(Keys.S))
                direction.Y += 1;
            if (keyboardState.IsKeyDown(Keys.A))
                direction.X -= 1;
            if (keyboardState.IsKeyDown(Keys.D))
                direction.X += 1;
            
            // Sound-Control
            bool isMoving = direction != Vector2.Zero;
            
            if (WalkingSoundInstance != null)
            {
                // Start Playing Walking Sound if Player starts moving around
                if (isMoving && !wasMoving)
                {
                    WalkingSoundInstance.Play();
                }
                // Stop Playing Walking Sound if Player stops moving around
                else if (!isMoving && wasMoving)
                {
                    WalkingSoundInstance.Stop();
                }
            }
            
            wasMoving = isMoving;

            lastTransform = new Transform(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size);

            if (direction != Vector2.Zero)
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
            else
            {
                // show first frane
                _frameIndex = 0;
                _frameTimer = 0f;
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
    }
}