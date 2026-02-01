// ============================================================
// The CharacterAttributes should be used for the basic Stats to play with. Like Health or Damage
//
//
// ============================================================
using System;
using BikeWars.Content.entities.interfaces;

namespace BikeWars.Entities.Characters;
    public class CharacterAttributes : AttributesBase
    {
        public event Action<CharacterBase> OnDied;
        public bool SuppressDeathEvent { get; set; } = false;

        public CharacterAttributes(object owner, int maxHealth, int health, int attackDamage, float attackCoolDown, bool canAutoAttack)
            : base(owner, maxHealth, health, attackDamage, attackCoolDown, canAutoAttack)
        {
        }
        protected override void Die()
        {
            if (SuppressDeathEvent) return;

            if (Owner is CharacterBase character)
                OnDied?.Invoke(character);
        }

        public void ForceDie()
        {
            if (Owner is CharacterBase character)
                OnDied?.Invoke(character);
        }
    }
