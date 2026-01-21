#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using BikeWars.Content.managers;

namespace BikeWars.Content.entities.items
{
    public class DamageCircle : AreaOfEffectBase
    {
        private const string SPRITE_KEY = "DamageCircle";

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
        
        private readonly Transform _sourceTransform;
        private readonly Func<Vector2> _positionProvider;
        private readonly bool _damagePlayers;


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


        public DamageCircle(
            Transform sourceTransform,
            CharacterBase? owner,
            bool damagePlayers = true)
            : base(owner, damage: 20, duration: 2.0f)
        {
            _sourceTransform = sourceTransform;
            _damagePlayers = damagePlayers;

            // Always query the live position so the circle follows even if the owner's transform instance gets replaced
            _positionProvider = owner != null
                ? () => owner.Transform.Position
                : () => _sourceTransform.Position;

            foreach (Vector2 boxPosition in _boxPositions)
            {
                _hitboxes.Add(new BoxCollider(
                    sourceTransform.Position + boxPosition,
                    100,
                    100,
                    CollisionLayer.AOE,
                    this
                ));
            }
        }




        public override void LoadContent(ContentManager content)
        {
            _spriteSheet = SpriteManager.GetTexture(SPRITE_KEY);

            _frameWidth = _spriteSheet.Width / _frameCount;
            _frameHeight = _spriteSheet.Height;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _spritePos = _sourceTransform.Position;
            _spritePos = _positionProvider.Invoke();

            for (int i = 0; i < _hitboxes.Count; i++)
            {
                _hitboxes[i].Position = _spritePos + _boxPositions[i];
            }

            _frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_frameTimer >= _frameTime)
            {
                _frameTimer = 0f;
                _currentFrame = (_currentFrame + 1) % _frameCount;
            }
        }

        public override bool CanDamage(CharacterBase target)
        {
            if (!_damagePlayers && target is Player)
                return false;

            return base.CanDamage(target);
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
