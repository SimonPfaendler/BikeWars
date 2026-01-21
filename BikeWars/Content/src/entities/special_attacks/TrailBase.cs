using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using BikeWars.Content.managers;

namespace BikeWars.Content.entities.items
{
    public abstract class TrailBase : AreaOfEffectBase
    {
        private string _spriteKey;

        private Texture2D _spriteSheet;

        protected int _frameWidth;
        protected int _frameHeight;
        protected int _frameCount = 5;
        protected int _currentFrame = 0;

        protected float _frameTime = 0.18f; // Animation speed
        protected float _frameTimer = 0f;

        protected Player _player;
        protected Vector2 _dir;
        protected float _rotation;
        protected Vector2 _spritePos;

        private float _spawnTimer = 0f;
        private float _spawnInterval = 0.15f; // spawn every X seconds

        private class TrailSprite
        {
            public Vector2 Pos;
            public int Frame = 0;
            public float Timer = 0f;
            public TrailSprite(Vector2 pos)
            {
                Pos = pos;
            }
        }

        private List<TrailSprite> _trailSprites = new List<TrailSprite>();




        protected TrailBase(Player player, Vector2 dir, string spriteKey, int damage, float duration)
            : base(owner: player, damage: damage, duration: duration)
        {
            _player = player;
            _dir = Vector2.Normalize(dir);
            _rotation = 0f;
            _spriteKey = spriteKey;
        }

        public override void LoadContent(ContentManager content)
        {
            _spriteSheet = SpriteManager.GetTexture(_spriteKey);

            _frameWidth = _spriteSheet.Width / _frameCount;
            _frameHeight = _spriteSheet.Height;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 playerCenter = _player.Transform.Position +
                new Vector2(_player.Transform.Size.X / 2f,
                            _player.Transform.Size.Y / 2f);

            Vector2 offset = new Vector2(-30, -30);
            Vector2 trailCenter = playerCenter - offset;

            _spritePos = trailCenter - new Vector2(_frameWidth, _frameHeight);

            _frameTimer += dt;
            if (_frameTimer >= _frameTime)
            {
                _frameTimer = 0f;
                _currentFrame = (_currentFrame + 1) % _frameCount;
            }

            _spawnTimer += dt;
            if (_spawnTimer >= _spawnInterval)
            {
                _spawnTimer = 0f;

                _trailSprites.Add(new TrailSprite(_spritePos));

                _hitboxes.Add(new BoxCollider(
                    _spritePos,
                    _frameWidth,
                    _frameHeight,
                    CollisionLayer.AOE,
                    this
                ));

            }

            foreach (var t in _trailSprites)
            {
                t.Timer += dt;

                if (t.Timer >= _frameTime)
                {
                    t.Timer -= _frameTime;
                    t.Frame = (t.Frame + 1) % _frameCount;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (var t in _trailSprites)
            {
                Rectangle srcTrail = new Rectangle(
                    t.Frame * _frameWidth,
                    0,
                    _frameWidth,
                    _frameHeight);

                spriteBatch.Draw(_spriteSheet, t.Pos, srcTrail, Color.White);
            }

            Rectangle src = new Rectangle(
                _currentFrame * _frameWidth,
                0,
                _frameWidth,
                _frameHeight);

            spriteBatch.Draw(
                _spriteSheet,
                _spritePos,
                src,
                Color.White,
                _rotation,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }
}
