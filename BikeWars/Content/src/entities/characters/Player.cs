using Microsoft.Xna.Framework;
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
        private InputHandler _ip;
        public Color Tint = Color.Black;
        private BoxCollider _collider { get; set; }

        private Movement movement { get; set;}
        
        public SoundEffect WalkingSound { get; private set; }
        public SoundEffectInstance WalkingSoundInstance { get; private set; }
        private bool wasMoving = false;
        
        public void LoadContent(SoundEffect walkingSoundEffect)
        {
            WalkingSound = walkingSoundEffect;
            WalkingSoundInstance = WalkingSound.CreateInstance();
            WalkingSoundInstance.IsLooped = true;
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
            _ip = new InputHandler();
            UpdateCollider();
        }
        public override bool Intersects(ICollider collider)
        {
            return _collider.Intersects(collider);
        }

        public override void Update(GameTime gameTime)
        {
            if (!canMove) return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 direction = MakeDirection();
            
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

            LastTransform = new Transform(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size);
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                Transform.Position += direction * Speed * delta;
            }
            UpdateCollider();
        }

        private Vector2 MakeDirection()
        {
            Vector2 direction = Vector2.Zero;
            if (_ip.PressingAction(Action.MOVE_UP))
            {
                direction.Y -= 1;
            }
            if (_ip.PressingAction(Action.MOVE_DOWN))
            {
                direction.Y += 1;
            }
            if (_ip.PressingAction(Action.MOVE_LEFT))
            {
                direction.X -= 1;
            }
            if (_ip.PressingAction(Action.MOVE_RIGHT))
            {
                direction.X += 1;    
            }
            return direction;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (pixel == null)
            {
                // Create a 1x1 white texture if it doesn't exist
                pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                pixel.SetData(new[] { Microsoft.Xna.Framework.Color.White });
            }
            // Draw the player as a colored rectangle
            spriteBatch.Draw(pixel, Transform.Bounds, Tint);
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