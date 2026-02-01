using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.entities.interfaces;
public abstract class ObjectBase
{
    public Transform Transform { get; protected set; }
    public BoxCollider Collider { get; protected set; }
    public BoxCollider CollisionCollider { get; protected set; }

    protected Texture2D CurrentTex { get; set; }

    public virtual void Update(GameTime gameTime) { }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (CurrentTex == null) return;
        spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.White);
    }

    public virtual bool Intersects(ICollider other)
        => Collider != null && Collider.Intersects(other);
}
