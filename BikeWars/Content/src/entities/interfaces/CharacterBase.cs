using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
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

    private Transform _renderTransform { get; set; }
    public Transform RenderTransform { get => _renderTransform; set => _renderTransform = value; }
    public float Speed;
    public bool slowed;
    public float CurrentSpeed { get; set; }
    private CircleCollider _collider { get; set; }
    public CircleCollider Collider {get => _collider; set => _collider = value;}
    public Vector2 GazeDirection {get; set;}
    private CharacterAttributes _attributes {get;set;}
    public CharacterAttributes Attributes {get => _attributes; set => _attributes = value;}
    private float _attackCooldownTimer = 0f;
    public bool IsDead => Attributes.Health <= 0 && !IsDying; // Dead if 0 HP AND not in dying state
    public bool IsGodMode { get; set; } = false;
    public bool CanRevive { get; set; } = false;
    public bool IsDying { get; protected set; } = false;
    public float DyingTimer { get; protected set; } = 0f;
    protected const float DyingDuration = 20f;

    public bool _XpDropped { get; set; } = false; // for making sure each enemy only drops XP once

    public EnemyMovement Movement { get; protected set; }
    protected AudioService _audio;
    protected WorldAudioManager _worldAudioManager;

    protected virtual string WalkingSound => AudioAssets.Walking;

    public event Action<CharacterBase, int, object> OnTookDamage;

    protected Vector2 _knockbackVelocity;
    private const float KnockbackDecay = 10f; // Velocity decay per second

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (IsGodMode || IsDead || IsDying) return;
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
    protected Color _hitColor = Color.DarkRed;

    // Tweening props
    protected Vector2 _renderScale = Vector2.One;
    protected float _pulseTimer = 0f;

    public void TriggerSquash(float x, float y)
    {
        _renderScale = new Vector2(x, y);
    }

    protected void UpdateTweens(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Squash Recovery
        // Lerp back to (1,1)
        float lerpT = MathF.Min(10f * dt, 1f);
        _renderScale = Vector2.Lerp(_renderScale, Vector2.One, lerpT);

        // Continuous Pulse (only if close to 1,1 to avoid conflict with squash)
        if (Vector2.Distance(_renderScale, Vector2.One) < 0.15f)
        {
            _pulseTimer += dt * 5f;
            float intensity = 0.1f * GetPulseMultiplier();
            float pulse = MathF.Sin(_pulseTimer) * intensity;
            _renderScale = Vector2.Lerp(_renderScale, Vector2.One, Math.Min(10f * dt, 1f));
        }
        else
        {
            // If squashing heavily, sync timer so it doesn't jump
            _pulseTimer = 0f;
        }
    }

    public virtual bool IsCharacterMoving()
    {
        return Movement != null && Movement.IsMoving;
    }

    public virtual float GetPulseMultiplier()
    {
        return 1.0f;
    }

    public virtual void TakeDamage(int amount, object hitBy, bool shouldSquash = true)
    {
        if (IsGodMode || IsDead || IsDying) return;

        // Check if deadly damage
        if (CanRevive && !IsDying && (Attributes.Health - amount <= 0))
        {
            IsDying = true;
            DyingTimer = DyingDuration;
            Attributes.SuppressDeathEvent = true;
            Attributes.Health = 0;
            return;
        }
        Attributes.Health -= amount;
        OnTookDamage?.Invoke(this, amount, hitBy);
        _hitFlashTimer = 0.2f;

        // Squash effect on hit
        if (shouldSquash)
        {
            TriggerSquash(1.5f, 1.2f);
        }
    }

    protected void UpdateDyingState(GameTime gameTime)
    {
        if (!IsDying) return;

        DyingTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (DyingTimer <= 0f)
        {
            IsDying = false;
            Attributes.ForceDie();
        }
    }

    public void Revive()
    {
        if (!IsDying) return;

        IsDying = false;
        Attributes.SuppressDeathEvent = false;

        int reviveHealth = (int)(Attributes.MaxHealth * 0.5f);
        Attributes.Health = reviveHealth;
        DyingTimer = 0f;
    }

    // Explicit interface implementation to satisfy ICombat
    void ICombat.TakeDamage(int amount, object hitBy)
    {
        TakeDamage(amount, hitBy, true);
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
        target.TakeDamage(Attributes.AttackDamage, this);
        ResetAttackCooldown();
    }

    public void ResetAttackCooldown()
    {
        _attackCooldownTimer = Attributes.AttackCooldown;
    }

    public virtual void ThrowAttack(ICombat target) { }

    public void UpdateCollider(CollisionLayer layer)
    {
        Vector2 colliderPosition = new Vector2(
            Transform.Position.X - Transform.Size.X / 2f,
            Transform.Position.Y - Transform.Size.Y / 2f
        );

        if (Collider == null)
        {
            Collider = new CircleCollider(
                Transform.Radius,
                colliderPosition,
                // CollisionLayer.PLAYER,
                layer,
                this
            );
        } else
        {
            Collider.Radius = Transform.Radius;
            Collider.Position = colliderPosition;
            Collider.Width = Transform.Size.X;
            Collider.Height = Transform.Size.Y;
        }
        RenderTransform.Position = Transform.Position;
    }
    public void UpdateHitFlash(GameTime gameTime)
    {
        if (_hitFlashTimer > 0f)
        {
            _hitFlashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        UpdateTweens(gameTime);
    }
    public abstract void Update(GameTime gameTime);
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