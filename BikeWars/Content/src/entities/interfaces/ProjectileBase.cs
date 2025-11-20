using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.entities.interfaces;

// These should be used for basic projectiles, like bullet, fireballs or something like that
// Like everything that will fly and should do damage.
public abstract class ProjectileBase : IProjectile
{
    public int Damage { get; set; }
    private Transform _transform { get; set; }
    public Transform Transform { get => _transform;  set => _transform = value; }
    private ICollider _collider { get; set; }
    public virtual ICollider Collider { get => _collider; set => _collider = value; }
    private IMoveable _movement { get; set; }
    public virtual IMoveable Movement { get => _movement; set => _movement = value; }

    private Texture2D _texUp {get; set;}
    private Texture2D _texDown {get; set;}
    private Texture2D _texLeft {get; set;}
    private Texture2D _texRight {get; set;}
    private Texture2D _currentTex {get; set;}
    public Texture2D TexUp {get => _texUp; set => _texUp = value;}
    public Texture2D TexDown {get => _texDown; set => _texDown = value;}
    public Texture2D TexLeft {get => _texLeft; set => _texLeft = value;}
    public Texture2D TexRight {get => _texRight; set => _texRight = value;}
    public Texture2D CurrentTex {get => _currentTex; set => _currentTex = value;}
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);
    public abstract bool Intersects(ICollider other);
    public abstract void LoadContent(ContentManager contentManager);
}