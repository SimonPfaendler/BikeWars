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

        /// Tracks last damage time per target to enforce damage interval
        private readonly Dictionary<object, float> _lastDamageTime = new();

        /// Damage is applied once per this interval (in seconds)
        protected float DamageInterval { get; set; } = 1.0f;

        public AreaOfEffectBase(CharacterBase owner, int damage, float duration)
        {
            Owner = owner;
            Damage = damage;
            Duration = duration;
        }

        /// Checks if enough time has passed since last damage to this target
        public virtual bool CanDamage(CharacterBase target)
        {
            if (target == null)
                return false;

            if (target == Owner)
                return false;

            return CanDamageObject(target);
        }

        /// Generic variant so AOEs can also hurt non-character targets (e.g., destructible walls)
        public virtual bool CanDamageObject(object target)
        {
            if (target == null)
                return false;

            if (!_lastDamageTime.ContainsKey(target))
                _lastDamageTime[target] = 0f;

            if (timeAlive - _lastDamageTime[target] >= DamageInterval)
            {
                _lastDamageTime[target] = timeAlive;
                return true;
            }
            return false;
        }

        public virtual void Update(GameTime gameTime)
        {
            // Track lifetime
            timeAlive += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void LoadContent(ContentManager content);

        /// Returns list of colliders for the AOE
        public virtual List<ICollider> GetHitboxes() => _hitboxes;
    }
}
