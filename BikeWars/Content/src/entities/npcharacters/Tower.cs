using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
using System;
using System.Collections.Generic;
using BikeWars.Content.entities.interfaces;

namespace BikeWars.Entities;
public abstract class Tower
{
    //private readonly SpriteAnimation _idleAnimation;
    //private readonly SpriteAnimation _walkLeftAnimation;
    //private readonly SpriteAnimation _walkRightAnimation;
    //private readonly SpriteAnimation _walkUpAnimation;
    //private readonly SpriteAnimation _walkDownAnimation;
    private Transform _transform { get; set; }
    public Transform Transform { get => _transform;  set => _transform = value; }
    public float Speed;
    private BoxCollider _collider { get; set; }
    public BoxCollider Collider {get => _collider; set => _collider = value;}
    protected float _attackCooldownTimer = 0f;
    private TowerAttributes _attributes {get;set;}
    public TowerAttributes Attributes {get => _attributes; set => _attributes = value;}
    public bool IsDead => Attributes.Health <= 0;
    public event Action<Tower, int> OnTookDamage;

    public float Rotation {get; set;}

    public Vector2 GazeDirection { get; set; }

    protected Texture2D _texture;
    protected AudioService _audio;


    //private readonly PathFinding _pathFinding;
    // 1x1 Texture to represent the enemy
    public static Texture2D pixel;

    public Tower(Vector2 start, Point size)
    {
        // _pathFinding = pathFinding;
        // _collisionManager = collisionManager;
        GazeDirection = new Vector2(0, -1);
        Rotation = 0f;
        Transform = new Transform(start, size);


        // Movement = new EnemyMovement(canMove: true, isMoving: false, pathFinding: _pathFinding,
        //     gridMapper: _collisionManager, repathScheduler: _repathScheduler);
        // _idleAnimation = SpriteManager.GetAnimation("Hobo_Idle");
        // _walkLeftAnimation = SpriteManager.GetAnimation("Hobo_WalkLeft");
        // _walkRightAnimation = SpriteManager.GetAnimation("Hobo_WalkRight");
        // _walkDownAnimation = SpriteManager.GetAnimation("Hobo_WalkDown");
        // _walkUpAnimation = SpriteManager.GetAnimation("Hobo_WalkUp");
        // _currentAnimation = SpriteManager.GetTexture("TowerAlly");
        // UpdateCollider();
    }

    public void Update(GameTime gameTime, List<CharacterBase> enemies)
    {
        UpdateAttack(gameTime, enemies);
        // UpdateAttackCooldown(gameTime);
        // UpdateKnockback(gameTime);
        // UpdateHitFlash(gameTime);
        // Sound- and Movement-Control
        // if (Movement is EnemyMovement em)
        // {
        //     em.EnemyPosition = Transform.Position;
        //     em.PlayerPosition = _collisionManager.GameObjectManager.Player1.Transform.Position;
        // }
        // Movement.HandleMovement(gameTime);
        // HandleSound(Movement.IsMoving);

    // Vector2 direction = Movement.Direction;
    //     LastTransform = new Transform(Transform.Position, Transform.Size);
    //     if (Movement.IsMoving)
    //     {
    //         float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

    //         if (direction.LengthSquared() > 0.0001f)
    //         {
    //             direction.Normalize();
    //             Transform.Position += direction * Speed * delta;
    //         }

    //         if (System.Math.Abs(direction.X) > System.Math.Abs(direction.Y))
    //         {

    //         _currentAnimation = (direction.X > 0) ? _walkRightAnimation : _walkLeftAnimation;
    //         }
    //         else
    //         {

    //         _currentAnimation = (direction.Y > 0) ? _walkDownAnimation : _walkUpAnimation;
    //         }
    //     }
    //     else
    //     {
    //         _currentAnimation = _idleAnimation;
    //     }

    // if (_currentAnimation != null)
    //     {
    //         _currentAnimation.Update(gameTime, Movement.IsMoving);
    //     }

    // UpdateCollider();
    }

    protected abstract void UpdateAttack(GameTime gameTime, List<CharacterBase> enemies);

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        float spriteOrientationOffset = -MathF.PI / 2f;
        spriteBatch.Draw(
            _texture,
            Transform.Bounds.Center.ToVector2(),
            null,
            Color.White,
            Rotation + spriteOrientationOffset,
            new Vector2(_texture.Width / 2f, _texture.Height / 2f),
            2.5f,
            SpriteEffects.None,
            0f
        );
    }
    
    public void Immobalize(bool value)
    {
    }

     public virtual void TakeDamage(int amount)
    {
    }

    protected virtual void OnDestroyed()
    {
    }

    public void Dispose()
    {
        _texture.Dispose();
        _texture = null;
    }

    public bool CanAttack()
    {
        return _attackCooldownTimer <= 0f;
    }
}