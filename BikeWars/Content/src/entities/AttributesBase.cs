using System;
using BikeWars.Content.entities.interfaces;

namespace BikeWars.Entities
{
    public abstract class AttributesBase
    {
        public object Owner { get; protected set; }
        protected virtual void Die() { }
        
        private int _health { get; set; }
        public int Health
        {
            get => _health;
            set
            {
                if (value <= 0)
                {
                    _health = 0;
                    Die();
                    return;
                }
                if (value > MaxHealth)
                {
                    _health = MaxHealth;
                }
                _health = value;
            }
        }

        private int _maxHealth { get; set; }
        public int MaxHealth
        {
            get => _maxHealth;
            set
            {
                if (value < 0)
                {
                    _maxHealth = 0;
                }
                _maxHealth = value;
            }
        }

        private int _attackDamage { get; set; }
        public int AttackDamage
        {
            get => _attackDamage;
            set
            {
                if (value < 0)
                {
                    _attackDamage = 0;
                }
                _attackDamage = value;
            }
        }

        private float _attackCooldown { get; set; }
        public float AttackCooldown
        {
            get => _attackCooldown;
            set
            {
                if (value < 0)
                {
                    _attackCooldown = 0;
                }
                _attackCooldown = value;
            }
        }

        public bool CanAutoAttack { get; set; }
        public float CritChance { get; set; } = 0.1f; // 10% chance
        public float CritMultiplier { get; set; } = 2.0f; // 2x damage
        
        protected AttributesBase(object owner, int maxHealth, int health, int attackDamage, float attackCooldown, bool canAutoAttack)
        {
            Owner = owner;
            MaxHealth = maxHealth;
            if (health <= 0)
            {
                Health = MaxHealth;
            }
            else
            {
                Health = health;
            }
            AttackDamage = attackDamage;
            AttackCooldown = attackCooldown;
            CanAutoAttack = canAutoAttack;
        }

        public float ThrowAttackChance { get; set; } = 0.005f; // 0.1% chance
        public float ThrowRange { get; set; } = 400f;
    }
}