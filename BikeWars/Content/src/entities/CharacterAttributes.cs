// ============================================================
// The CharacterAttributes should be used for the basic Stats to play with. Like Health or Damage
//
//
// ============================================================
using System;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities;

namespace BikeWars.Entities.Characters;
public class CharacterAttributes : AttributesBase
    {
        public event Action<CharacterBase> OnDied;
    

        public CharacterAttributes(object owner, int maxHealth, int health, int attackDamage, float attackCoolDown, bool canAutoAttack)
            : base(owner, maxHealth, health, attackDamage, attackCoolDown, canAutoAttack)
        { 
        }
        protected override void Die()
        {
        if (Owner is CharacterBase character)
            OnDied?.Invoke(character);
        }
    }
