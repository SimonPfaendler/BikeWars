using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;

namespace BikeWars.Content.entities.items
{
    public class IceTrail : AreaOfEffectBase
    {
        private const string SPRITE_PATH = "assets/sprites/projectiles/ice_trail";

        private Texture2D _spriteSheet;

        private int _frameWidth;
        private int _frameHeight;
        private int _frameCount = 5;
        private int _currentFrame = 0;

        private float _frameTime = 0.18f; // How fast he animation runs
        private float _frameTimer = 0f;

        private Player _player;
        private Vector2 _dir;
        private float _rotation;
        private Vector2 _spritePos;

        // stores previously spawned animation frames
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

        private float _spawnTimer = 0f;
        private float _spawnInterval = 0.15f; // spawn every X seconds


        public IceTrail(Player player, Vector2 dir)
            : base(owner: player, damage: 1, duration: 3.0f)
        {
            _player = player;
            _dir = Vector2.Normalize(dir);
        }

        public override void LoadContent(ContentManager content)
        {
            _spriteSheet = content.Load<Texture2D>(SPRITE_PATH);

            _frameWidth = _spriteSheet.Width / _frameCount;
            _frameHeight = _spriteSheet.Height;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // position behind player
            Vector2 playerCenter = _player.Transform.Position +
                new Vector2(_player.Transform.Size.X / 2f,
                            _player.Transform.Size.Y / 2f);

            Vector2 offset = new Vector2(-30, -30);
            Vector2 trailCenter = playerCenter - offset;

            // draw location
            _spritePos = trailCenter - new Vector2(_frameWidth, _frameHeight);

            // animation advance
            _frameTimer += dt;
            if (_frameTimer >= _frameTime)
            {
                _frameTimer = 0f;
                _currentFrame = (_currentFrame + 1) % _frameCount;
            }

            // spawn hitbox + trail sprite only on interval
            _spawnTimer += dt;
            if (_spawnTimer >= _spawnInterval)
            {
                _spawnTimer = 0f;

                // store a visual trail copy
                _trailSprites.Add(new TrailSprite(_spritePos));

                // add regular hitbox for collision manager
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
            // draw trail first (behind)
            foreach (var t in _trailSprites)
            {
                Rectangle srcTrail = new Rectangle(
                    t.Frame * _frameWidth,
                    0,
                    _frameWidth,
                    _frameHeight);

                spriteBatch.Draw(_spriteSheet, t.Pos, srcTrail, Color.White);
            }

            // draw current animation on top
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
                Vector2.Zero, // origin was not defined in original, so keep zero
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }
}
