using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.managers;
using BikeWars.Content.engine.Audio;
using BikeWars.Entities.Characters;

namespace BikeWars.Content.components
{
    public class Tram
    {
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public float Rotation { get; private set; }
        public Point Size { get; private set; }
        public List<BoxCollider> Colliders { get; private set; }
        public bool HasHonked { get; set; } = false;
        public bool IsExpired { get; set; } = false;

        private readonly Texture2D _texture;
        private const int COLLIDER_SEGMENT_SIZE = 40;
        private const float SPEED = 700f;

        private AudioService _audio;
        private Player _player; // Reference to player for distance
        public event Action<float, float> RequestScreenShake;

        public Tram(Vector2 startPosition, Vector2 targetPosition, AudioService audio, Player player)
        {
            Position = startPosition;
            Size = new Point(515, 50);

            _audio = audio;
            _player = player;

            // Calculate Direction and Rotation
            Vector2 direction = targetPosition - startPosition;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }
            Velocity = direction * SPEED;
            Rotation = (float)Math.Atan2(direction.Y, direction.X);

            Colliders = new List<BoxCollider>();
            InitializeColliders();
            _texture = SpriteManager.GetTexture("Tram");
        }

        private void InitializeColliders()
        {
            // Create a chain of colliders along the length of the tram
            int numberOfSegments = Size.X / COLLIDER_SEGMENT_SIZE;
            for (int i = 0; i < numberOfSegments; i++)
            {
                var collider = new BoxCollider(Vector2.Zero, COLLIDER_SEGMENT_SIZE, Size.Y, CollisionLayer.TRAM, this);
                Colliders.Add(collider);
            }
            UpdateColliders();
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Velocity * dt;
            UpdateColliders();

            if (_player != null)
            {
                float distToPlayer = Vector2.Distance(Position, _player.Transform.Position);
                if (distToPlayer > 5500)
                {
                    IsExpired = true;
                }

                // Screen Shake if near
                if (distToPlayer < 1400)
                {
                    float intensity = 6f * (1f - (distToPlayer / 1400f));
                    RequestScreenShake?.Invoke(intensity, 0.2f);
                }

                // Honk if near
                if (!HasHonked && distToPlayer < 3000)
                {
                    _audio.Sounds.Play(AudioAssets.TrainHorn);
                    HasHonked = true;
                }
            }
        }

        private void UpdateColliders()
        {
            // Update the position of each collider segment based on the Tram's current position and rotation

            // Direction vector for the length of the tram
            Vector2 forward = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));

            // Calculate the starting offset
            float halfLength = Size.X / 2f;
            Vector2 startOffset = -forward * halfLength;

            // Distance between segment centers
            float segmentSpacing = (float)Size.X / Colliders.Count;

            for (int i = 0; i < Colliders.Count; i++)
            {
                // Calculate center position of this segment along the tram line
                float distanceAlongTram = (i * segmentSpacing) + (segmentSpacing / 2f);
                Vector2 segmentCenterPos = Position + startOffset + (forward * distanceAlongTram);
                Vector2 colliderPos = segmentCenterPos - new Vector2(Colliders[i].Width / 2f, Colliders[i].Height / 2f);
                Colliders[i].Position = colliderPos;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_texture == null) return;
            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            Vector2 scale = Vector2.One;

            spriteBatch.Draw(
                _texture,
                Position,
                null,
                Color.White,
                Rotation,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );

        }
    }
}
