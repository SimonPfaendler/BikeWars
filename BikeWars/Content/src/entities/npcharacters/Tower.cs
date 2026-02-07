using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using System;
using System.Collections.Generic;
using BikeWars.Content.entities.interfaces;

namespace BikeWars.Entities;
public abstract class Tower
{
    private Transform _transform { get; set; }
    public Transform Transform { get => _transform;  set => _transform = value; }
    public float Speed;
    private BoxCollider _collider { get; set; }
    public BoxCollider Collider {get => _collider; set => _collider = value;}
    protected float _attackCooldownTimer = 0f;
    private TowerAttributes _attributes {get;set;}
    public TowerAttributes Attributes {get => _attributes; set => _attributes = value;}
    public bool IsDead => Attributes.Health <= 0;
    //public event Action<Tower, int> OnTookDamage;

    public float Rotation {get; set;}

    public Vector2 GazeDirection { get; set; }

    protected Texture2D _texture;
    protected AudioService _audio;


    //private readonly PathFinding _pathFinding;
    // 1x1 Texture to represent the enemy
    public static Texture2D pixel;

    public Tower(Vector2 start, Point size)
    {
        GazeDirection = new Vector2(0, -1);
        Rotation = 0f;
        Transform = new Transform(start, size);
    }

    public void Update(GameTime gameTime, List<CharacterBase> enemies)
    {
        UpdateAttack(gameTime, enemies);
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

     public virtual void TakeDamage(int amount)
    {
    }
}