using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.interfaces;
public interface ICombat
{
    CharacterAttributes Attributes {get; set;}

    void TakeDamage(int amount);
    void Attack(ICombat target);

    void UpdateAttackCooldown(GameTime gameTime);
    bool CanAttack();

    void ResetAttackCooldown();

    bool IsDead { get; }
}
