using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Entities;

namespace BikeWars.Content.entities.interfaces;

// These should be used for basic projectiles, like bullet, fireballs or something like that
// Like everything that will fly and should do damage.
public abstract class ProjectileBase : IProjectile, IWeapon
{
    public object Owner {get; set;} // TODO Should be optimized. This is now only that we know that the projectiles is firedBy

    public WeaponAttributes weaponAttributes {get; set;}
    public bool HasHit {get; set;} // Should be used for the collision. Or it will render it too often.
    private Transform _transform { get; set; }
    public Transform Transform { get => _transform;  set => _transform = value; }
    private ICollider _collider { get; set; }
    public virtual ICollider Collider { get => _collider; set => _collider = value; }
    private IMoveable _movement { get; set; }
    public virtual IMoveable Movement { get => _movement; set => _movement = value; }

    private Texture2D _texRight {get; set;}
    private Texture2D _currentTex {get; set;}
    public Texture2D TexRight {get => _texRight; set => _texRight = value;}
    public Texture2D CurrentTex {get => _currentTex; set => _currentTex = value;}
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);
    public abstract bool Intersects(ICollider other);
    public abstract void LoadContent(ContentManager contentManager);

    public abstract void LevelUp();
    public WeaponAttributes WeaponAttributes()
    {
        return weaponAttributes;
    }
}