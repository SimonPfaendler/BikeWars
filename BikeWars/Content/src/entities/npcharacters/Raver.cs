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
        private readonly SpriteAnimation _idleAnimation;

        public Raver(Vector2 start, Point size, AudioService audio)
        {
            _audio = audio;
            Attributes = new CharacterAttributes(this, maxHealth: 35, health: 35, attackDamage: 2, attackCoolDown: 0f, canAutoAttack: false);

            Transform = new Transform(start, size);
            // LastTransform = new Transform(start, size);
            RenderTransform = new Transform(start, new Point(32, 32));

            Speed = 0.5f;
            CurrentSpeed = Speed;

            Movement = null;

            _idleAnimation = SpriteManager.GetAnimation("Hobo_Idle");

            UpdateCollider();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateAttackCooldown(gameTime);
            UpdateKnockback(gameTime);
            UpdateHitFlash(gameTime);

            _idleAnimation?.Update(gameTime, false);

            UpdateCollider();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead)
                return;

            // TODO: your animation/sprite here.
            // If you want a placeholder like Hobo.pixel, you can draw a rectangle.

            if (_idleAnimation == null)
                return;

            Color drawColor = (_hitFlashTimer > 0f) ? _hitColor : Color.White;

            _idleAnimation.Draw(
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

