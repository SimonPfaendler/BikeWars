using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.entities.interfaces;
public abstract class ItemBase : IItem
{
    public virtual bool InventoryItem => false; //Items which go in the inventory
    public bool IsBike {get; set;}
    public virtual bool IsConsumable => false;
    public virtual int HealAmount => 0;
    private Transform _transform { get; set; }
    public Transform Transform { get => _transform;  set => _transform = value; }
    private BoxCollider _collider { get; set; }
    public virtual BoxCollider Collider { get => _collider; set => _collider = value; }
    public bool IsPickedUp {get; set;}
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

    public virtual void Update(GameTime gameTime)
    {}

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.White);
    }

    public virtual bool Intersects(ICollider other)
    {
        return Collider.Intersects(other);
    }
    protected void InitpickupRange(int pickupRange = 40)   // should be used for real Items
    {

        Collider = new BoxCollider(
            new Vector2(Transform.Position.X - pickupRange / 2f,
                Transform.Position.Y - pickupRange / 2f),
            Transform.Size.X + pickupRange,
            Transform.Size.Y + pickupRange,
            CollisionLayer.ITEM,
            this
        );
    }
    
}