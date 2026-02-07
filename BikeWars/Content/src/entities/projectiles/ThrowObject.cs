#nullable enable
using System;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using BikeWars.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.components;

namespace BikeWars.Content.entities.projectiles
{
    /// <summary>
    /// Generic lobbed projectile that fakes a parabolic arc via mid-flight scaling.
    /// Collisions are disabled while airborne and enabled on landing for a brief linger window.
    /// </summary>
    public class ThrowObject : ProjectileBase
    {
        private readonly Vector2 _start;
        public Vector2 _target;
        private readonly float _duration;
        private float _elapsed;

        private readonly float _arcScale;
        private readonly float _lingerDuration;

        private bool _landed;
        private float _lingerTimer;

        private float _rotation;
        private readonly float _spinSpeed = 8f; // rad/s
        private float _currentScale = 1f;
        private Vector2 _drawCenter;
        private readonly Vector2 _origin;

        private readonly Point _baseSize;
        private readonly BoxCollider _collider;

        public bool IsFinished { get; protected set; }

        public override ICollider Collider => _collider;

        public ThrowObject(Vector2 start, Vector2 target, object? owner, string textureKey, int damage, float speed = 100f, float arcScale = 1.2f, float lingerDuration = 0.25f, WeaponAttributes? attributes = null)
        {
            Owner = owner;
            if (attributes != null)
            {
                weaponAttributes = attributes;
                speed = attributes.Speed > 0 ? attributes.Speed : speed;
                arcScale = attributes.ArcScale > 0 ? attributes.ArcScale : arcScale;
                lingerDuration = attributes.LingerDuration > 0 ? attributes.LingerDuration : lingerDuration;
            }
            else
            {
                weaponAttributes = new WeaponAttributes(owner, 1, 5, damage, speed, arcScale, lingerDuration);
            }
            HasHit = false;

            TexRight = SpriteManager.GetTexture(textureKey);
            CurrentTex = TexRight;

            _origin = new Vector2(CurrentTex.Width / 2f, CurrentTex.Height / 2f);
            _drawCenter = start;

            _start = start;
            _target = target;
            _arcScale = arcScale;
            _lingerDuration = lingerDuration;

            _baseSize = new Point(CurrentTex.Width, CurrentTex.Height);
            Transform = new Transform(start - new Vector2(_baseSize.X / 2f, _baseSize.Y / 2f), _baseSize);

            _collider = new BoxCollider(Transform.Position, Transform.Size.X, Transform.Size.Y, CollisionLayer.NONE, this);

            float distance = Vector2.Distance(start, target);
            _duration = MathHelper.Clamp(distance / speed, 0.25f, 1.0f);
        }

        public override void Update(GameTime gameTime)
        {
            if (IsFinished)
            {
                return;
            }

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _elapsed += delta;
            float t = MathF.Min(1f, _elapsed / _duration);

            Vector2 center = Vector2.Lerp(_start, _target, t);
            _drawCenter = center;

            if (!_landed)
            {
                // Sinusoidal bump to fake a parabolic arc by scaling up mid-flight
                float heightFactor = MathF.Sin(MathF.PI * t);
                float scale = 1f + heightFactor * _arcScale;
                _currentScale = scale;

                int width = Math.Max(6, (int)(_baseSize.X * scale));
                int height = Math.Max(6, (int)(_baseSize.Y * scale));

                Vector2 topLeft = center - new Vector2(width / 2f, height / 2f);

                Transform.Size = new Point(width, height);
                Transform.Position = topLeft;
                _collider.Position = topLeft;
                _collider.SetSize(width, height);
                _collider.Layer = CollisionLayer.NONE; // no hits while in the air

                // spin while airborne
                _rotation += _spinSpeed * delta;
                if (_rotation > MathHelper.TwoPi) _rotation -= MathHelper.TwoPi;

                if (t >= 1f)
                {
                    OnLanded();
                }
            }
            else
            {
                _lingerTimer -= delta;
                if (_lingerTimer <= 0f)
                {
                    IsFinished = true;
                }
            }
        }

        protected virtual void OnLanded()
        {
            _landed = true;
            _lingerTimer = _lingerDuration;

            // Snap to final size/position and enable collisions
            Transform.Size = _baseSize;
            Transform.Position = _target - new Vector2(_baseSize.X / 2f, _baseSize.Y / 2f);
            _collider.Position = Transform.Position;
            _collider.SetSize(_baseSize.X, _baseSize.Y);
            _collider.Layer = CollisionLayer.PROJECTILE;

            _currentScale = 1f;
            _drawCenter = _target;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                CurrentTex,
                position: _drawCenter,
                sourceRectangle: null,
                color: Color.White,
                rotation: _rotation,
                origin: _origin,
                scale: _currentScale,
                effects: SpriteEffects.None,
                layerDepth: 0f
            );
        }

        public override bool Intersects(ICollider other)
        {
            return _collider.Intersects(other);
        }

        public override void LoadContent(ContentManager contentManager)
        {
            // Textures are fetched from SpriteManager in constructor.
        }

        public override void LevelUp()
        {
            return;
        }
    }
}
