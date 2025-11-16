using BikeWars.Content.engine.interfaces;
using BikeWars.Content.engine;
using Microsoft.Xna.Framework;

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

    private ICollider _collider { get; set; }
    public abstract void Update(GameTime gameTime); // Use this to update the logic like where the position is or resize the collision box
    public abstract bool Intersects(ICollider other);
}