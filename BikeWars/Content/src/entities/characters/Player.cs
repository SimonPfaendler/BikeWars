using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework.Audio;
using BikeWars.Content.entities.interfaces;

namespace BikeWars.Entities.Characters
{
    public class Player: CharacterBase
    {
        private bool canMove;
        
        public Color Tint = Color.Black;
        private BoxCollider _collider { get; set; }
        private Movement movement { get; set; }
        public SoundHandler SoundHandler { get; }
        
        private Texture2D texUp, texDown, texLeft, texRight;
        private Texture2D currentTex;
        
        // Animation
        private const int FrameCount = 2;          // frames per sprite 
        private const float SecondsPerFrame = 0.16f;
        private float _frameTimer = 0f;
        private int _frameIndex = 0;
        private bool isMoving { get; set; }
        
        public void LoadContent(ContentManager content, SoundEffect walkingSoundEffect)
        {
            // sprites 
            texRight = content.Load<Texture2D>("assets/sprites/character1/c1_move_right_1x2");
            texLeft  = content.Load<Texture2D>("assets/sprites/character1/c1_move_left_1x2");
            texUp    = content.Load<Texture2D>("assets/sprites/character1/c1_move_up_1x2");
            texDown  = content.Load<Texture2D>("assets/sprites/character1/c1_move_down_1x2");

            // spawn sprite
            currentTex = texRight;

            // sounds
            SoundHandler.WalkingSoundInstance = walkingSoundEffect.CreateInstance();
            SoundHandler.WalkingSoundInstance.IsLooped = true;
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
            canMove = true;
            movement = new Movement();
            
            SoundHandler = new SoundHandler();
            isMoving = false;
            UpdateCollider();
        }
        public override bool Intersects(ICollider collider)
        {
            return _collider.Intersects(collider);
        }

        public override void Update(GameTime gameTime)
        {
            if (!canMove)
            {
                isMoving = false;
                SoundHandler.WalkingSoundInstance.Stop();
                return;    
            }

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 direction = MakeDirection();
            
            // Sound-Control
            isMoving = direction != Vector2.Zero;

            if (SoundHandler.WalkingSoundInstance != null)
            {
                // Start Playing Walking Sound if Player starts moving around
                if (isMoving)
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

        private Vector2 MakeDirection()
        {   
            
            Vector2 direction = Vector2.Zero;
            if (InputHandler.IsHeld(GameAction.MOVE_UP))
            {
                direction.Y -= 1;
            }
            if (InputHandler.IsHeld(GameAction.MOVE_DOWN))
            {
                direction.Y += 1;
            }
            if (InputHandler.IsHeld(GameAction.MOVE_LEFT))
            {
                direction.X -= 1;
            }
            if (InputHandler.IsHeld(GameAction.MOVE_RIGHT))
            {
                direction.X += 1;    
            }
            return direction;
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

        // Use this if the there is a reason the character isn't allowed to move
        public void SetCanMove(bool v)
        {
            canMove = v;
        }
    }
}