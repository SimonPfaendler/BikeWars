// ============================================================
// The CharacterAttributes should be used for the basic Stats to play with. Like Health or Damage
//
//
// ============================================================
namespace BikeWars.Entities.Characters
{
    public class CharacterAttributes
    {
        private int _health { get; set; }
        public int Health {
            get => _health;
            set
            {
                if (value <= 0) {
                    _health = 0;
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

        public CharacterAttributes(int maxHealth, int health, int attackDamage, float attackCoolDown, bool canAutoAttack)
        {
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
}