using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;

namespace BikeWars.Entities;
public class Tower: IWorldAudioAware
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
    private float _attackCooldownTimer = 0f;
    private TowerAttributes _attributes {get;set;}
    public TowerAttributes Attributes {get => _attributes; set => _attributes = value;}
    
    private SpriteAnimation _currentAnimation;
    protected AudioService _audio;


    //private readonly PathFinding _pathFinding;
    // 1x1 Texture to represent the enemy
    public static Texture2D pixel;

    public Tower(Vector2 start, Point size, AudioService audio, PathFinding pathFinding)
    {
        _audio = audio;
        _pathFinding = pathFinding;
        _collisionManager = collisionManager;
        _repathScheduler = repathScheduler;

        Attributes = new TowerAttributes(this, 40, 0, 5, 2f, false);
        Transform = new Transform(start, size);
        LastTransform = new Transform(start, size);
        Speed = 130f;
        Movement = new EnemyMovement(canMove: true, isMoving: false, pathFinding: _pathFinding,
            gridMapper: _collisionManager, repathScheduler: _repathScheduler);
        _idleAnimation = SpriteManager.GetAnimation("Hobo_Idle");
        _walkLeftAnimation = SpriteManager.GetAnimation("Hobo_WalkLeft");
        _walkRightAnimation = SpriteManager.GetAnimation("Hobo_WalkRight");
        _walkDownAnimation = SpriteManager.GetAnimation("Hobo_WalkDown");
        _walkUpAnimation = SpriteManager.GetAnimation("Hobo_WalkUp");
        _currentAnimation = _idleAnimation;
        UpdateCollider();
    }

public override void Update(GameTime gameTime)
    {
        UpdateAttackCooldown(gameTime);
        UpdateKnockback(gameTime);
        UpdateHitFlash(gameTime);
        // Sound- and Movement-Control
        if (Movement is EnemyMovement em)
        {
            em.EnemyPosition = Transform.Position;
            em.PlayerPosition = _collisionManager.GameObjectManager.Player1.Transform.Position;
        }
        Movement.HandleMovement(gameTime);
        HandleSound(Movement.IsMoving);

    Vector2 direction = Movement.Direction;
        LastTransform = new Transform(Transform.Position, Transform.Size);
        if (Movement.IsMoving)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (direction.LengthSquared() > 0.0001f)
            {
                direction.Normalize();
                Transform.Position += direction * Speed * delta;
            }
            
            if (System.Math.Abs(direction.X) > System.Math.Abs(direction.Y))
            {

            _currentAnimation = (direction.X > 0) ? _walkRightAnimation : _walkLeftAnimation;
            }
            else
            {

            _currentAnimation = (direction.Y > 0) ? _walkDownAnimation : _walkUpAnimation;
            }
        }
        else
        {
            _currentAnimation = _idleAnimation;
        }

    if (_currentAnimation != null)
        {
            _currentAnimation.Update(gameTime, Movement.IsMoving);
        }

    UpdateCollider();
    }

public override void Draw(SpriteBatch spriteBatch)
    {
        if(IsDead)
            return;
        if (_currentAnimation == null)
            return;
        
        Color drawColor = (_hitFlashTimer > 0f) ? _hitColor : Color.White;
        _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size, 0f, _renderScale, drawColor);
    }

public void SetWorldAudioManager(WorldAudioManager manager)
    {
        _worldAudioManager = manager;
    }

public void Immobalize(bool value)
    {
        Movement.CanMove = !value;
    }
    public override void Attack(ICombat target)
    {
        if (!CanAttack()) return;
        base.Attack(target);
        _audio.Sounds.Play(AudioAssets.Punch);
    }
}