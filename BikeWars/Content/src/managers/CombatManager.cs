using System;
using System.Collections.Generic;
using System.Numerics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled;

namespace BikeWars.Content.managers;

/// CombatManager solves all combat realated collisions from collision manager
/// Reacts to events from collision manager
public class CombatManager
{
    public Action<CharacterBase> OnCharacterDeath; // Event for deah
    public Action<Player, ItemBase> OnItemPickup;   // Event for item pickup

    public CombatManager(CollisionManager collisionManager)
    {
        // Register to events from collision manager
        collisionManager.OnProjectileHit += HandleProjectileHit;
        collisionManager.OnItemPickup += HandleItemPickup;
        collisionManager.OnCharacterCollision += HandleCharacterCollision;
    }

    private void HandleProjectileHit(CharacterBase target, ProjectileBase projectile)
    {
        if (target.IsDead) return;
        if (target == projectile.Owner) return;

        target.TakeDamage(projectile.Damage);

        projectile.HasHit = true;
        projectile.MarkForRemoval = true;

        if (target.Health <= 0) HandleDeath(target);
    }

    private void HandleItemPickup(Player player, ItemBase item)
    {
        // player.Inventory?.Add(item);
        // item.MarkForRemoval = true;
        // OnItemPickup?.Invoke(player, item);
    }

    private void HandleCharacterCollision(CharacterBase a, CharacterBase b)
    {
        if (a.IsDead || b.IsDead) return;

        if (a is Player && b is EnemyBase enemy)
        {
            a.TakeDamage(enemy.Damage);
            if (a.Health <= 0) HandleDeath(a);
        }
        else if (b is Player && a is EnemyBase enemy2)
        {
            b.TakeDamage(enemy2.Damage);
            if (b.Health <= 0) HandleDeath(b);
        }
    }

    private void HandleDeath(CharacterBase character)
    {
        character.IsDead = true;
        OnCharacterDeath?.Invoke(character);
    }
}
