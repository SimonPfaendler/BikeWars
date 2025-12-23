using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;

namespace BikeWars.Content.entities.items
{
    public class DamageCircle : AreaOfEffectBase
    {
        private const string SPRITE_PATH = "assets/sprites/projectiles/damage_circle";

        private Texture2D _spriteSheet;

        private int _frameWidth;
        private int _frameHeight;
        private int _frameCount = 5;
        private int _currentFrame = 0;

        private float _frameTime = 0.18f; // How fast the animation runs
        private float _frameTimer = 0f;

        private Player _player;
        private float _rotation;
        private Vector2 _spritePos;

        private List<Vector2> _boxPositions = new()
        {
            new Vector2(0, 0),
            new Vector2(100, 0),
            new Vector2(-100, 0),
            new Vector2(-200, 0),
            new Vector2(0, 100),
            new Vector2(-100, 100),
            new Vector2(0, -100),
            new Vector2(100, -100),
            new Vector2(-100, -100),
            new Vector2(-200, -100),
            new Vector2(-100, -200),
            new Vector2(0, -200),
        };


        public DamageCircle(Player player)
            : base(owner: player, damage: 1, duration: 2.0f)
        {
            _player = player;
            _rotation = 0f;
            // add regular hitbox for collision manager
            foreach (Vector2 boxPosition in _boxPositions)
            {
                _hitboxes.Add(new BoxCollider(
                _spritePos + boxPosition,
                100,
                100,
                CollisionLayer.AOE,
                this
            ));
            }

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

            // draw location
            _spritePos = _player.Transform.Position;

            for(int i = 0; i < 12; i++)
            {
                _hitboxes[i].Position = _spritePos + _boxPositions[i];
            }


            // animation advance
            _frameTimer += dt;
            if (_frameTimer >= _frameTime)
            {
                _frameTimer = 0f;
                _currentFrame = (_currentFrame + 1) % _frameCount;
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

            Vector2 origin = new Vector2(_frameWidth / 2f, _frameHeight / 2f);

            spriteBatch.Draw(
                _spriteSheet,
                _spritePos - origin,
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
