using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.entities.interfaces
{
    /// <summary>
    /// Base class for Area-of-Effect attacks (e.g., Flamethrower cone).
    /// - Does NOT move.
    /// - Does NOT get removed on collision.
    /// - Only expires after Duration.
    /// - Can contain multiple hitboxes.
    /// </summary>
    public abstract class AreaOfEffectBase
    {
        public CharacterBase Owner { get; protected set; }

        public int Damage { get; protected set; }

        /// long the AOE stays active (seconds)
        public float Duration { get; protected set; }

        /// How long it has existed
        protected float timeAlive = 0f;

        /// AOE is removed once true
        public bool IsExpired => timeAlive >= Duration;

        /// All colliders that represent this AOE shape
        protected readonly List<ICollider> _hitboxes = new();

        public AreaOfEffectBase(CharacterBase owner, int damage, float duration)
        {
            Owner = owner;
            Damage = damage;
            Duration = duration;
        }

        public virtual void Update(GameTime gameTime)
        {
            // Track lifetime
            timeAlive += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public abstract void LoadContent(ContentManager content);
        public abstract void Draw(SpriteBatch spriteBatch);

        /// Returns list of colliders for the AOE
        public virtual List<ICollider> GetHitboxes() => _hitboxes;

        /// Checks intersection against ANY hitbox
        public virtual bool Intersects(ICollider other)
        {
            foreach (var box in _hitboxes)
            {
                if (box.Intersects(other))
                    return true;
            }
            return false;
        }
    }
}
