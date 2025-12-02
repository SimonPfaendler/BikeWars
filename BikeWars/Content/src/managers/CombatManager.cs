using System;
using System.Collections.Generic;
using System.Numerics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled;
using BikeWars.Content.engine.Audio;

namespace BikeWars.Content.managers;

/// CombatManager solves all combat realated collisions from collision manager
/// Reacts to events from collision manager
public class CombatManager
{

    private readonly AudioService _audio;

    public CombatManager(AudioService audio)
    {
        _audio = audio ?? throw new ArgumentNullException(nameof(audio));
    }

    // Projectile hits a character
    public void HandleProjectileHit(CharacterBase target, ProjectileBase projectile)
    {
        if (target.IsDead) return;
        if (target.IsGodMode)
        {
            return;
        }
        if (target == projectile.Owner) return;

        // Apply Damage
        target.TakeDamage(projectile.Damage);
        projectile.HasHit = true;
        
        _audio.Sounds.Play(AudioAssets.BulletHit);
        // if (target.Health <= 0) HandleDeath(target);
    }

    // Two Characters collide (Close combat / Melee attack)
    public void HandleCharacterCollision(CharacterBase a, CharacterBase b)
    {
        if (a.IsDead || b.IsDead) return;

        if (a is Player && b is CharacterBase enemy)
        {
            enemy.Attack(a);
        }
        else if (b is Player && a is CharacterBase enemy2)
        {
            enemy2.Attack(b);
        }

    }
}