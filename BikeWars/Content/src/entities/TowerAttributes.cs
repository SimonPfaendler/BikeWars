// ============================================================
// The Towerattributes should be used for the basic Stats to play with. Like Health or Damage
//
//
// ============================================================
using System;
namespace BikeWars.Entities;
public class TowerAttributes : AttributesBase
{
    public float AttackRange = 200f;

    public event Action<Tower> OnDied;


    public TowerAttributes(object owner, int maxHealth, int health, int attackDamage, float attackCoolDown, bool canAutoAttack)
        : base(owner, maxHealth, health, attackDamage, attackCoolDown, canAutoAttack)

    {

    }
    protected override void Die()
    {
        if (Owner is Tower tower)
            OnDied?.Invoke((Tower)Owner);
    }
}