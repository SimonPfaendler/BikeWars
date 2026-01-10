using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using BikeWars.Content.managers;

namespace BikeWars.Content.entities.special_attacks
{
    public class KamikazeExplosion : AreaOfEffectBase
    {
        private SpriteAnimation _animation;
        private Vector2 _position;

        public KamikazeExplosion(CharacterBase owner, Vector2 position)
            : base(owner: owner, damage: 50, duration: 0.9f) // 9 frames * ~0.1s
        {
            _position = position;
            _animation = SpriteManager.GetAnimation("KamikazeOpa_Death");
            
            // Scaled to 0.75x
            // Visual Size: 256 * 0.75 = 192
            
            // Hitbox removed for purely visual effect
        }

        public override void LoadContent(ContentManager content)
        {
            // Animation loaded via SpriteManager
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_animation != null)
            {
                _animation.Update(gameTime, true);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
             if (_animation != null)
            {
                // Draw at position. Scaled to 192x192 (0.75x of 256)
                _animation.Draw(spriteBatch, _position, new Point(192, 192), 0f, Color.White);
            }
        }
    }
}
