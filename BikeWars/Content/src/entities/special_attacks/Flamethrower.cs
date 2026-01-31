using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using BikeWars.Content.managers;


namespace BikeWars.Content.entities.items
{
    public class Flamethrower : AreaOfEffectBase
    {
        private const string SPRITE_KEY = "Flamethrower";

        private Texture2D _spriteSheet;

        private int _frameWidth;
        private int _frameHeight;
        private int _frameCount = 10; // Number of frames in attack animation
        private int _currentFrame = 0;

        private float _frameTime = 0.05f;
        private float _frameTimer = 0f;

        private Player _player;
        private Vector2 _dir;
        private float _rotation;
        private Vector2 _spritePos;

        public Flamethrower(Player player, Vector2 dir)
            : base(owner: player, damage: 30, duration: 3.0f) // AOE base constructor
        {
            _player = player;
            _dir = Vector2.Normalize(dir);
        }

        public override void LoadContent(ContentManager content)
        {
            _spriteSheet = SpriteManager.GetTexture(SPRITE_KEY);

            _frameWidth = _spriteSheet.Width / _frameCount;
            _frameHeight = _spriteSheet.Height;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime); // updates timeAlive → used by IsExpired

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Smoothly follow player’s gaze
            _dir = Vector2.Lerp(_dir, Vector2.Normalize(_player.GazeDirection), 0.2f);

            _rotation = (float)Math.Atan2(_dir.Y, _dir.X) + MathHelper.PiOver2;

            // Position flame in front of player
            Vector2 playerCenter = _player.Transform.Position +
                                   new Vector2(_player.Transform.Size.X / 2f,
                                               _player.Transform.Size.Y / 2f);

            Vector2 offset = _dir * 30f;

            Vector2 flameBottomCenter = playerCenter + offset;

            // Location where sprite is drawn (bottom center)
            _spritePos = flameBottomCenter - new Vector2(_frameWidth, _frameHeight);

            // ANIMATION
            _frameTimer += dt;
            if (_frameTimer >= _frameTime)
            {
                _frameTimer = 0f;
                _currentFrame = (_currentFrame + 1) % _frameCount;
            }

            // BUILD HITBOXES
            _hitboxes.Clear();

            int segments = 5;
            float totalLength = _frameHeight;
            float segLength = totalLength / segments;
            float segWidth = _frameWidth;

            for (int i = 0; i < segments; i++)
            {
                float dist = segLength * (i + 0.5f);
                Vector2 center = flameBottomCenter + _dir * dist;

                Vector2 topLeft = center - new Vector2(segWidth, segLength / 2f);

                _hitboxes.Add(new BoxCollider(
                    topLeft,
                    (int)segWidth,
                    (int)segLength,
                    CollisionLayer.AOE,
                    this
                ));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle src = new Rectangle(
                _currentFrame * _frameWidth,
                0,
                _frameWidth,
                _frameHeight
            );

            Vector2 origin = new Vector2(_frameWidth / 2f, _frameHeight);

            spriteBatch.Draw(
                _spriteSheet,
                _spritePos + origin,
                src,
                Color.White,
                _rotation,
                origin,
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }
}
