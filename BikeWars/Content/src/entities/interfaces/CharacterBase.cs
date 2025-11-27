using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.entities.interfaces;
public abstract class CharacterBase : ICharacter
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

    public abstract void SetLastTransform();
    public abstract void UpdateCollider();
    public abstract void Update(GameTime gameTime); // Use this to update the logic like where the position is or resize the collision box
    public abstract void Draw(SpriteBatch spriteBatch);
    public abstract void LoadContent(ContentManager contentManager);
    public bool Intersects(ICollider other)
    {
        return Collider.Intersects(other);
    }
    public abstract void TakeDamage(int amount);
}