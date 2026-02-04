using System;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.entities.items;
using BikeWars.Utilities;
using Microsoft.Xna.Framework;
using BikeWars.Entities;
using BikeWars.Content.entities.npcharacters;
using BikeWars.Entities.Characters.MapObjects;

namespace BikeWars.Content.managers;

/// CombatManager solves all combat realated collisions from collision manager
/// Reacts to events from collision manager
public class CombatManager
{

    private readonly AudioService _audio;
    private readonly GameObjectManager _gameObjects; // used for spawning items
    public event Action<float> OnHitStopRequested;
    public event Action<float, float> OnScreenShakeRequested;

    private static readonly string[] ThiefDeathSounds = {
        AudioAssets.BikeThiefHit1,
        AudioAssets.BikeThiefHit2,
    };
    private static readonly string[] DogDeathSounds = {
        AudioAssets.DogHit1,
        AudioAssets.DogHit2,
    };
    private static readonly string[] HoboDeathSounds = {
        AudioAssets.HoboHit1,
        AudioAssets.HoboHit2,
    };

    public CombatManager(AudioService audio, GameObjectManager gameObjects)
    {
        _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        _gameObjects = gameObjects ?? throw new ArgumentNullException(nameof(gameObjects));
    }

    public void HandleDeath(CharacterBase target)
    {
        if (target._XpDropped)
            return;

        PlayDeathSound(target);

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

        // Friendly Fire Protection: Towers should not damage Players
        if (projectile.Owner is TowerAlly && target is Player) return;
        if (projectile.Owner is Tower && target is Player) return; // General check for base Tower class


        // Crit Logic
        int damage = projectile.weaponAttributes.Damage;

        if (projectile.Owner is CharacterBase owner)
        {
            damage += owner.Attributes.AttackDamage;
            if (RandomUtil.NextDouble() < owner.Attributes.CritChance)
            {
                damage = (int)(damage * owner.Attributes.CritMultiplier);
            }
        }

        // Apply Damage
        target.TakeDamage(damage, projectile.Owner);
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
        target.TakeDamage(aoe.Damage, aoe.Owner, shouldSquash: false);

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

    public void HandleprojecticleHitDestructible(DestructibleObject destructible, ProjectileBase projectile)
    {
        projectile.HasHit = true;
        destructible.TakeDamage(projectile.weaponAttributes.Damage);
        _audio.Sounds.Play(destructible.Health > 0 ? AudioAssets.WoodCrack : AudioAssets.WoodDestroy);
        _gameObjects.SpawnDamageNumber(GetObjectCenter(destructible), projectile.weaponAttributes.Damage);
    }

    public void HandleAOEHitDestructible(DestructibleObject destructible, AreaOfEffectBase aoe)
    {
        destructible.TakeDamage(aoe.Damage);
        _audio.Sounds.Play(destructible.Health > 0 ? AudioAssets.WoodCrack : AudioAssets.WoodDestroy);
        _gameObjects.SpawnDamageNumber(GetObjectCenter(destructible), aoe.Damage);
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
    public void HandleTramHit(CharacterBase target, object tram)
    {
        if (target.IsDead) return;
        if (target.IsGodMode) return;

        target.TakeDamage(12, tram);
        _audio.Sounds.Play(AudioAssets.TrainHit);

        if (target.Attributes.Health <= 0)
        {
            HandleDeath(target);
        }
    }

    // car collision damage
    public void HandleCarHit(CharacterBase target,  object car)
    {
        if (target.IsDead) return;
        if (target.IsGodMode) return;

        target.TakeDamage(12, car);

        _audio.Sounds.Play(AudioAssets.CarCrash);

        if (target.Attributes.Health <= 0)
        {
            HandleDeath(target);
        }
    }
    public void HandleBaechleHit(CharacterBase target, object baechle)
    {
        if (target.IsDead) return;
        if (target.IsGodMode) return;

        target.TakeDamage(1, baechle);
        _audio.Sounds.Play(AudioAssets.BaechleSplash);

        if (target.Attributes.Health <= 0)
        {
            HandleDeath(target);
        }
    }
    private static Vector2 GetObjectCenter(DestructibleObject destructible)
    {
        var bounds = destructible.Transform.Bounds;
        return new Vector2(bounds.Center.X, bounds.Center.Y);
    }

    private void PlayDeathSound(CharacterBase target)
    {
        string[] soundArray = [];
        if (target is BikeThief)
        {
            soundArray = ThiefDeathSounds;
        }
        else if (target is Dog)
        {
            soundArray = DogDeathSounds;
        }
        else if (target is Dozent)
        {
            _audio.Sounds.Play(AudioAssets.DozentHit);
            return;
        }
        else if (target is PoliceMan)
        {
            _audio.Sounds.Play(AudioAssets.PolizistHit);
            return;
        }
        else if (target is Hobo)
        {
            soundArray = HoboDeathSounds;
        }

        int length = soundArray.Length;
        if (length == 0)
        {
            return;
        }
        int index = RandomUtil.NextInt(0, length);
        string randomTalk = soundArray[index];

        _audio.Sounds.Play(randomTalk);

    }
}

