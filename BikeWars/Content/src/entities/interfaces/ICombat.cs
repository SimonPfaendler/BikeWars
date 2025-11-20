using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public interface ICombat
{
    int Health { get; set; }
    int MaxHealth { get; set; }

    int AttackDamage { get; set; }
    float AttackSpeed { get; set; }

    void TakeDamage(int amount);
    void Attack(ICombat target);
    bool IsDead { get; }

}    
