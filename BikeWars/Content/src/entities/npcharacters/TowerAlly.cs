using Microsoft.Xna.Framework;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using BikeWars.Entities;
using System;
using BikeWars.Content.entities.interfaces;
using System.Collections.Generic;

namespace BikeWars.Entities;
public class TowerAlly : Tower
{
    private int PADDING_INTERACTION_AREA = 40;
    private BoxCollider _collisionCollider {get;set;}
    public BoxCollider CollisionCollider {get => _collisionCollider; set => _collisionCollider = value; } // Now this collider is for collision
    public event Action<TowerAlly> OnShoot;
    private bool _isActivated;

    public TowerAlly(Vector2 pos, Point size, AudioService audio)
        : base(pos, size, audio)
    {
        _texture = SpriteManager.GetTexture("TowerAlly");
        Attributes = new TowerAttributes(this, 40, 0, 5, 0.5f, false);
        Collider = new BoxCollider(new Vector2(Transform.Position.X - PADDING_INTERACTION_AREA / 2, Transform.Position.Y - PADDING_INTERACTION_AREA / 2), Transform.Size.X + PADDING_INTERACTION_AREA, Transform.Size.Y + PADDING_INTERACTION_AREA, CollisionLayer.INTERACT, this);
        CollisionCollider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.TOWER, this);
        Speed = 130f;
        _isActivated = false;
    }
    public void Activate()
    {
        _isActivated = true;
    }

    protected override void UpdateAttack(GameTime gameTime, List<CharacterBase> enemies)
    {
        if (!_isActivated) return;

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _attackCooldownTimer -= deltaTime;
        if (_attackCooldownTimer > 0f) return;

        var target = FindNearestEnemy(enemies);
        if (target == null) return;

        Vector2 myCenter = Transform.Bounds.Center.ToVector2();
        Vector2 targetCenter = target.Transform.Bounds.Center.ToVector2();

        float rotationSpeed = MathF.PI; // Radiant pro Sekunde
        Vector2 desiredDir = Vector2.Normalize(targetCenter - myCenter);
        float targetAngle = MathF.Atan2(desiredDir.X, desiredDir.Y);
        // float diff = MathHelper.WrapAngle(targetAngle - Rotation);
        // float diff = MathHelper.WrapAngle(targetAngle);
        // Rotation += MathF.Sign(diff) * Math.Min(rotationSpeed * deltaTime, MathF.Abs(diff));
        Rotation = targetAngle;
        GazeDirection = new Vector2(MathF.Cos(Rotation), MathF.Sin(Rotation));

        ShootAt(target);

        _attackCooldownTimer = Attributes.AttackCooldown;
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