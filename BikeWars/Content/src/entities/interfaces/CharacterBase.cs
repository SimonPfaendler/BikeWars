using System;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.entities.interfaces;
public abstract class CharacterBase : ICharacter, ICombat
{
    private Transform _transform { get; set; }
    public Transform Transform { get => _transform;  set => _transform = value; }
    private Transform _lastTransform { get; set; }
    public Transform LastTransform { get => _lastTransform; set => _lastTransform = value; }
    public float Speed;
    public float SprintSpeed;
    public float CurrentSpeed { get; set; }
    private BoxCollider _collider { get; set; }
    public BoxCollider Collider {get => _collider; set => _collider = value;}
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int AttackDamage { get; set; }
    public float AttackCooldown { get; set; }
    private float _attackCooldownTimer = 0f;
    public bool IsDead => Health <= 0;
    public bool IsGodMode { get; set; } = false;
    
    protected EnemyMovement Movement;
    protected SpriteAnimation CurrentAnimation;
    protected AudioService _audio;
    protected WorldAudioManager _worldAudioManager;
    
    protected virtual string WalkingSound => AudioAssets.Walking;

    public void UpdateAttackCooldown(GameTime gameTime)
    {
        if (_attackCooldownTimer > 0f)
        {
            _attackCooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_attackCooldownTimer < 0f)
                _attackCooldownTimer = 0f;
        }
    }
    
    public virtual void TakeDamage(int amount)
    {
        
        if (IsGodMode)
            return; 
        
        if (IsDead) return;

        Health -= amount;

        if (Health <= 0)
        {
            Health = 0;
        }
    }

    public bool CanAttack()
    {
        return _attackCooldownTimer <= 0f;
    }
    
    public virtual void Attack(ICombat target)
    {
        if (!CanAttack()) return;
        target.TakeDamage(AttackDamage);
        ResetAttackCooldown(); 
    }
    
    // Is Helpful for example with colliders to set the original position back.
    public virtual void SetLastTransform()
    {
        Transform = new Transform(new Vector2(LastTransform.Position.X, LastTransform.Position.Y), LastTransform.Size);
    }

    public void ResetAttackCooldown()
    {
        _attackCooldownTimer = AttackCooldown;
    }
    
    public abstract void UpdateCollider();
    public abstract void Update(GameTime gameTime); // Use this to update the logic like where the position is or resize the collision box
    public abstract void Draw(SpriteBatch spriteBatch);
    public abstract void LoadContent(ContentManager contentManager);
    public bool Intersects(ICollider other)
    {
        return Collider.Intersects(other);
    }
    
    protected void HandleMovementAndSound(GameTime gameTime, EnemyMovement movement)
    {
        movement.HandleMovement(gameTime);
        
        if (_audio == null)
            return;

        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        bool isMoving = movement.Direction != Vector2.Zero && movement.CanMove;
        Console.WriteLine($"Hobo moving: {isMoving}, Direction: {movement.Direction}, Audible: {_worldAudioManager?.IsAudible(Transform.Position)}");

        if (isMoving && _worldAudioManager != null && _worldAudioManager.IsAudible(Transform.Position))
            _audio.Sounds.ResumeLoop(WalkingSound);
        else
            _audio.Sounds.PauseLoop(WalkingSound);

        if (isMoving)
        {
            Vector2 dir = movement.Direction;
            dir.Normalize();
            Transform.Position += dir * Speed * delta;
        }
    }
}