using System;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.entities.items;
using BikeWars.Utilities;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.managers;

/// CombatManager solves all combat realated collisions from collision manager
/// Reacts to events from collision manager
public class CombatManager
{

    private readonly AudioService _audio;
    private readonly GameObjectManager _gameObjects; // used for spawning items
    public event Action<float> OnHitStopRequested;
    public event Action<float, float> OnScreenShakeRequested;

    public CombatManager(AudioService audio, GameObjectManager gameObjects)
    {
        _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        _gameObjects = gameObjects ?? throw new ArgumentNullException(nameof(gameObjects));

    }

    public void HandleDeath(CharacterBase target)
    {
        if (target._XpDropped)
            return;

        target._XpDropped = true;
        _gameObjects.SpawnXp(target);
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
        
        // Crit Logic
        int damage = projectile.Damage;

        if (projectile.Owner is CharacterBase owner)
        {
            if (RandomUtil.NextDouble() < owner.Attributes.CritChance)
            {
                damage = (int)(damage * owner.Attributes.CritMultiplier);
            }
        }

        // Apply Damage
        target.TakeDamage(damage);
        projectile.HasHit = true;
        

        // Notify GameScreen to Shake
        OnScreenShakeRequested?.Invoke(2.75f, 0.10f);

        // Apply Knockback
        Vector2 knockbackDir = target.Transform.Position - projectile.Transform.Position;
        if(knockbackDir != Vector2.Zero)
            knockbackDir.Normalize();
        target.ApplyKnockback(knockbackDir, 220f); // knockback

        _audio.Sounds.Play(AudioAssets.BulletHit);

        OnHitStopRequested?.Invoke(0.075f); // Pause for 0.075 sec

        if (target.Attributes.Health <= 0)
        {
            HandleDeath(target);
        }
    }

    public void HandleAOEHit(CharacterBase target, AreaOfEffectBase aoe)
    {
        if (target.IsDead) return;
        if (target.IsGodMode)
        {
            return;
        }
        if (target == aoe.Owner) return;

        // Apply Damage
        target.TakeDamage(aoe.Damage, shouldSquash: false);
        // _gameObjects.SpawnDamageNumber(target.Transform.Position, aoe.Damage); // HANDLED BY GameObjectManager AGGREGATION


        if (aoe is IceTrail)
        {
            target.SlowDown(0.5f);
        }

        _audio.Sounds.Play(AudioAssets.ShortPain);
        if (target.Attributes.Health <= 0)
        {
            HandleDeath(target);
        }
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
    public void HandleTramHit(CharacterBase target)
    {
        if (target.IsDead) return;
        if (target.IsGodMode) return;

        target.TakeDamage(10);
        _audio.Sounds.Play(AudioAssets.TrainHit);

        if (target.Attributes.Health <= 0)
        {
            HandleDeath(target);
        }
    }

    public void HandleBaechleHit(CharacterBase target)
    {
        if (target.IsDead) return;
        if (target.IsGodMode) return;

        target.TakeDamage(1);
        // TODO: Add sound
        
        if (target.Attributes.Health <= 0)
        {
            HandleDeath(target);
        }
    }
}