using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;
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

    public void UpdateAttackCooldown(GameTime gameTime)
    {
        if (_attackCooldownTimer > 0f)
        {
            _attackCooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_attackCooldownTimer < 0f)
                _attackCooldownTimer = 0f;
        }
    }

    public bool CanAttack()
    {
        return _attackCooldownTimer <= 0f;
    }

    public void ResetAttackCooldown()
    {
        _attackCooldownTimer = AttackCooldown;
    }

    public abstract void TakeDamage(int amount);
    public abstract void Attack(ICombat target);

    public abstract void SetLastTransform();
    public abstract void UpdateCollider();
    public abstract void Update(GameTime gameTime); // Use this to update the logic like where the position is or resize the collision box
    public abstract void Draw(SpriteBatch spriteBatch);
    public abstract void LoadContent(ContentManager contentManager);
    public bool Intersects(ICollider other)
    {
        return Collider.Intersects(other);
    }
}