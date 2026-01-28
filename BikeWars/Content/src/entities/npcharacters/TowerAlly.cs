using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using BikeWars.Entities;
using System;
using BikeWars.Content.entities.interfaces;
using System.Collections.Generic;

namespace BikeWars.Entities;
public enum TowerState
{
    Inactive,
    Active,
    Cooldown
}

public class TowerAlly : Tower
{
    private const float ACTIVE_DURATION = 30f;
    private const float COOLDOWN_DURATION = 60f;

    private int PADDING_INTERACTION_AREA = 40;
    private BoxCollider _collisionCollider {get;set;}
    public BoxCollider CollisionCollider {get => _collisionCollider; set => _collisionCollider = value; } // Now this collider is for collision
    public event Action<TowerAlly> OnShoot;
    
    public TowerState State { get; private set; } = TowerState.Inactive;
    private float _activeTimer;
    private float _cooldownTimer;

    // Visual feedback colors
    private readonly Color _activeColor = Color.White;
    private readonly Color _inactiveColor = Color.Gray;
    private readonly Color _cooldownColor = new Color(255, 100, 100); // Red-ish

    public TowerAlly(Vector2 pos, Point size)
        : base(pos, size)
    {
        _texture = SpriteManager.GetTexture("TowerAlly");
        Attributes = new TowerAttributes(this, 300, 0, 5, 0.5f, false);
        Collider = new BoxCollider(new Vector2(Transform.Position.X - PADDING_INTERACTION_AREA / 2, Transform.Position.Y - PADDING_INTERACTION_AREA / 2), Transform.Size.X + PADDING_INTERACTION_AREA, Transform.Size.Y + PADDING_INTERACTION_AREA, CollisionLayer.INTERACT, this);
        CollisionCollider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.TOWER, this);
        Speed = 0f; // Tower shouldn't move
        
        // Ensure origin is centered for rotation. 
        // Note: The base Draw method uses _texture.Width/2, _texture.Height/2 as origin.
        // We just need to make sure our Transform is handled correctly.
    }
    public void Activate()
    {
        if (State == TowerState.Inactive)
        {
            State = TowerState.Active;
            _activeTimer = ACTIVE_DURATION;
        }
    }

    protected override void UpdateAttack(GameTime gameTime, List<CharacterBase> enemies)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // State Management
        switch (State)
        {
            case TowerState.Active:
                _activeTimer -= deltaTime;
                if (_activeTimer <= 0)
                {
                    State = TowerState.Cooldown;
                    _cooldownTimer = COOLDOWN_DURATION;
                    // Optional: Sound for powering down
                }
                break;
            case TowerState.Cooldown:
                _cooldownTimer -= deltaTime;
                if (_cooldownTimer <= 0)
                {
                    State = TowerState.Inactive;
                    // Optional: Sound for ready
                }
                return; // Can't attack in cooldown
            case TowerState.Inactive:
                return; // Can't attack when inactive
        }

        // Only proceed if Active
        if (State != TowerState.Active) return;

        _attackCooldownTimer -= deltaTime;
        
        var target = FindNearestEnemy(enemies);
        
        // If we have a target, rotate towards it
        if (target != null)
        {
            Vector2 myCenter = Transform.Bounds.Center.ToVector2();
            Vector2 targetCenter = target.Transform.Bounds.Center.ToVector2();
            Vector2 diff = targetCenter - myCenter;

            // Calculate target angle
            float targetAngle = MathF.Atan2(diff.Y, diff.X) + MathF.PI / 2f; 

            Rotation = MathHelper.Lerp(Rotation, targetAngle, 0.1f); 
            GazeDirection = new Vector2(MathF.Cos(Rotation - MathF.PI / 2f), MathF.Sin(Rotation - MathF.PI / 2f));
        }
        
        if (_attackCooldownTimer <= 0f)
        {
            ShootAt(target);
            _attackCooldownTimer = Attributes.AttackCooldown;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // Custom Draw to handle Feedback Colors
        Color tint = State switch
        {
            TowerState.Active => _activeColor,
            TowerState.Cooldown => _cooldownColor,
            _ => _inactiveColor
        };
        
        // Using the base draw logic but with our tint
        float spriteOrientationOffset = 0f; // We handled offset in Rotation calculation
        
         spriteBatch.Draw(
            _texture,
            Transform.Bounds.Center.ToVector2(),
            null,
            tint,
            Rotation + spriteOrientationOffset,
            new Vector2(_texture.Width / 2f, _texture.Height / 2f),
            2.5f,
            SpriteEffects.None,
            0f
        );

        // Optional: Draw Range or Status UI?
    }

    private CharacterBase FindNearestEnemy(List<CharacterBase> enemies)
    {
        CharacterBase nearest = null;
        float minDistSq = float.MaxValue;

        Vector2 myCenter = Transform.Bounds.Center.ToVector2();

        foreach (var enemy in enemies)
        {
            if (enemy.IsDead)
                continue;

            Vector2 enemyCenter = enemy.Transform.Bounds.Center.ToVector2();
            float distSq = Vector2.DistanceSquared(myCenter, enemyCenter);

            if (distSq > Attributes.AttackRange * Attributes.AttackRange)
                continue;

            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                nearest = enemy;
            }
        }
        return nearest;
    }
    private void ShootAt(CharacterBase target)
    {
        OnShoot?.Invoke(this);
    }
}