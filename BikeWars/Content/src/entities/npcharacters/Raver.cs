using Microsoft.Xna.Framework;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Entities.Characters
{
    public class Raver : CharacterBase
    {
        // the player should get damage when it touches a raver
        public const int ContactDamageCharacter = 2;
        public const int ContactDamageBike = 2;
        private readonly SpriteAnimation _walkLeftAnimation;
        private readonly SpriteAnimation _walkRightAnimation;

        private SpriteAnimation _currentAnimation;
        
        //constructor
        public Raver(Vector2 start, float size, AudioService audio, string leftAnimKey,
            string rightAnimKey)
        {
            _audio = audio;
            Attributes = new CharacterAttributes(this, maxHealth: 35, health: 35, attackDamage: 2, attackCoolDown: 0f, canAutoAttack: false);
            
            //get the position of the raver
            Transform = new Transform(start, size);
            
            // used to draw the sprite
            RenderTransform = new Transform(start, new Point(32, 32));

            Movement = null;

            _walkLeftAnimation  = SpriteManager.GetAnimation(leftAnimKey);
            _walkRightAnimation = SpriteManager.GetAnimation(rightAnimKey);
            
            _currentAnimation = _walkLeftAnimation;
            
            UpdateCollider();
        }
        
        // Sets the direction the raver is facing by selecting
        // the appropriate walking animation
        public void SetFacingLeft(bool startLeft)
        {
            if (startLeft)
                _currentAnimation = _walkLeftAnimation;
            else
                _currentAnimation = _walkRightAnimation;
        }

        public override void Update(GameTime gameTime)
        {
            UpdateAttackCooldown(gameTime);
            UpdateKnockback(gameTime);
            UpdateHitFlash(gameTime);

            if (_currentAnimation != null)
                _currentAnimation.Update(gameTime, false);

            UpdateCollider();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead)
                return;

            if (_currentAnimation == null)
                return;

            Color drawColor = (_hitFlashTimer > 0f) ? _hitColor : Color.White;

            _currentAnimation.Draw(
                spriteBatch,
                RenderTransform.Position,
                RenderTransform.Size,
                0f,
                _renderScale,
                drawColor
            );
        }
    }
}

