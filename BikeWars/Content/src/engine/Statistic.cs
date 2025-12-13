namespace BikeWars.Content.engine;
public class Statistic
{
    private int _kills { get; set; }
    public int Kills {
        get => _kills;
        set
        {
            if (value <= 0) {
                _kills = 0;
                return;
            }
            _kills = value;
        }
    }

    public void AddKill()
    {
        Kills += 1;
    }

    public Statistic()
    {
        Kills = 0;
    }

    public Statistic(int kills)
    {
        Kills = kills;
    }

    // private int _maxHealth { get; set; }
    // public int MaxHealth {
    //     get => _maxHealth;
    //     set
    //     {
    //         if (value < 0)
    //         {
    //             _maxHealth = 0;
    //         }
    //         _maxHealth = value;
    //     }
    // }

    // private int _attackDamage {get; set;}
    // public int AttackDamage {
    //     get => _attackDamage;
    //     set {
    //         if (value < 0)
    //         {
    //             _attackDamage = 0;
    //         }
    //         _attackDamage = value;
    //     }
    // }

    // private float _attackCooldown {get; set;}
    // public float AttackCooldown {
    //     get => _attackCooldown;
    //     set {
    //         if (value < 0)
    //         {
    //             _attackCooldown = 0;
    //         }
    //         _attackCooldown = value;
    //     }
    // }

    // private bool _canAutoAttack {get; set;}
    // public bool CanAutoAttack {
    //     get => _canAutoAttack;
    //     set  => _canAutoAttack = value;
    // }
    // public Statistic(int maxHealth, int health, int attackDamage, float attackCoolDown, bool canAutoAttack)
    // {
    //     MaxHealth = maxHealth;
    //     if (health <= 0)
    //     {
    //         Health = MaxHealth;
    //     } else
    //     {
    //         Health = health;
    //     }
    //     AttackDamage = attackDamage;
    //     AttackCooldown = attackCoolDown;
    //     CanAutoAttack = canAutoAttack;
    // }
}