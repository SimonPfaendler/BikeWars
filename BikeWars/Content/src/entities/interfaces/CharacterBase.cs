using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Entities.Characters;

namespace BikeWars.Content.entities.interfaces;
public abstract class CharacterBase : ICharacter, ICombat
{
    private Transform _transform { get; set; }
    public Transform Transform { get => _transform;  set => _transform = value; }
    private Transform _lastTransform { get; set; }
    public Transform LastTransform { get => _lastTransform; set => _lastTransform = value; }
    public float Speed;
    public bool slowed;
    public float SprintSpeed;
    public float CurrentSpeed { get; set; }
    private BoxCollider _collider { get; set; }
    public BoxCollider Collider {get => _collider; set => _collider = value;}

    private CharacterAttributes _attributes {get;set;}
    public CharacterAttributes Attributes {get => _attributes; set => _attributes = value;}
    private float _attackCooldownTimer = 0f;
    public bool IsDead => Attributes.Health <= 0;
    public bool IsGodMode { get; set; } = false;

    public bool _XpDropped { get; set; } = false; // for making sure each enemy only drops XP once

    public EnemyMovement Movement { get; protected set; }

    protected SpriteAnimation CurrentAnimation;
    protected AudioService _audio;
    protected WorldAudioManager _worldAudioManager;

    protected virtual string WalkingSound => AudioAssets.Walking;

    public event Action<CharacterBase, int> OnTookDamage;

    protected Vector2 _knockbackVelocity;
    private const float KnockbackDecay = 10f; // Velocity decay per second

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (IsGodMode || IsDead) return;
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
            _knockbackVelocity += direction * force;
        }
    }

    protected void UpdateKnockback(GameTime gameTime)
    {
        if (_knockbackVelocity == Vector2.Zero) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Apply fallback movement
        LastTransform = new Transform(Transform.Position, Transform.Size);
        Transform.Position += _knockbackVelocity * dt;

        // Decay velocity
        // Can be improved with linear or exponential decay
        float speed = _knockbackVelocity.Length();
        speed -= KnockbackDecay * speed * dt; // Exponential-ish decay

        if (speed <= 0.5f) // Threshold to stop
        {
            _knockbackVelocity = Vector2.Zero;
        }
        else
        {
            _knockbackVelocity.Normalize();
            _knockbackVelocity *= speed;
        }
        
    }

    public void UpdateAttackCooldown(GameTime gameTime)
    {
        if (_attackCooldownTimer > 0f)
        {
            _attackCooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_attackCooldownTimer < 0f)
                _attackCooldownTimer = 0f;
        }
    }

    protected float _hitFlashTimer = 0f;
    protected Color _hitColor = Color.Gray;

    public virtual void TakeDamage(int amount)
    {
        if (IsGodMode)
            return;

        if (IsDead) return;
        OnTookDamage?.Invoke(this, amount);
        Attributes.Health -= amount;
        _hitFlashTimer = 0.2f; // Flash for 0.2 seconds
    }
    public bool CanAttack()
    {
        return _attackCooldownTimer <= 0f;
    }

    public virtual void SlowDown(float slowFactor)
    {
        if(slowed) return; // Make sure enemy can only be slowed once

        Speed *= slowFactor;
        slowed = true;
    }

    public virtual void Attack(ICombat target)
    {
        if (!CanAttack()) return;
        target.TakeDamage(Attributes.AttackDamage);
        ResetAttackCooldown();
    }

    // Is Helpful for example with colliders to set the original position back.
    public virtual void SetLastTransform()
    {
        Transform = new Transform(new Vector2(LastTransform.Position.X, LastTransform.Position.Y), LastTransform.Size);
    }

    public void ResetAttackCooldown()
    {
        _attackCooldownTimer = Attributes.AttackCooldown;
    }

    public void UpdateCollider()
    {
        Vector2 colliderPosition = new Vector2(
            Transform.Position.X - Transform.Size.X / 2f,
            Transform.Position.Y - Transform.Size.Y / 2f
        );

        if (Collider == null)
        {
            Collider = new BoxCollider(
                colliderPosition,
                Transform.Size.X,
                Transform.Size.Y,
                CollisionLayer.PLAYER,
                this
            );
            return;
        }
        Collider.Position = colliderPosition;
        Collider.Width = Transform.Size.X;
        Collider.Height = Transform.Size.Y;
    }
    public void UpdateHitFlash(GameTime gameTime)
    {
        if (_hitFlashTimer > 0f)
        {
            _hitFlashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
    public abstract void Update(GameTime gameTime); // Use this to update the logic like where the position is or resize the collision box
    public abstract void Draw(SpriteBatch spriteBatch);
    public bool Intersects(ICollider other)
    {
        return Collider.Intersects(other);
    }

    protected void HandleSound(bool isMoving)
    {
        if (_audio == null)
            return;
        if (isMoving && _worldAudioManager != null && _worldAudioManager.IsAudible(Transform.Position))
            _audio.Sounds.ResumeLoop(WalkingSound);
        else
            _audio.Sounds.PauseLoop(WalkingSound);
    }
}