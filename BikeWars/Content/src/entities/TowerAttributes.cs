// ============================================================
// The Towerattributes should be used for the basic Stats to play with. Like Health or Damage
//
//
// ============================================================
using System;
using BikeWars.Content.entities.interfaces;

namespace BikeWars.Entities;
public class TowerAttributes
{
    
    public event Action<TowerAttributes> OnDied;
    private int _health { get; set; }
    public int Health {
        get => _health;
        set
        {
            if (value <= 0) {
                _health = 0;
                OnDied?.Invoke((TowerAttributes)owner);
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
    public int MaxHealth {
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
    public object owner {get; set;}

    private int _attackDamage {get; set;}
    public int AttackDamage {
        get => _attackDamage;
        set {
            if (value < 0)
            {
                _attackDamage = 0;
            }
            _attackDamage = value;
        }
    }
    private float _attackCooldown {get; set;}
    public float AttackCooldown {
        get => _attackCooldown;
        set {
            if (value < 0)
            {
                _attackCooldown = 0;
            }
            _attackCooldown = value;
        }
    }

    private bool _canAutoAttack {get; set;}
    public bool CanAutoAttack {
        get => _canAutoAttack;
        set  => _canAutoAttack = value;
    }

    public float CritChance { get; set; } = 0.1f; // 10% chance
    public float CritMultiplier { get; set; } = 2.0f; // 2x damage

    public TowerAttributes(object o, int maxHealth, int health, int attackDamage, float attackCoolDown, bool canAutoAttack)
    {
        owner = o;
        MaxHealth = maxHealth;
        if (health <= 0)
        {
            Health = MaxHealth;
        } else
        {
            Health = health;
        }
        AttackDamage = attackDamage;
        AttackCooldown = attackCoolDown;
        CanAutoAttack = canAutoAttack;
    }
}