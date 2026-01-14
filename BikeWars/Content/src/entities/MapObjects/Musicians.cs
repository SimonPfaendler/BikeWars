using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.entities.MapObjects;

public class Musicians : ItemBase
{
    private const int PADDING_INTERACTION_AREA = 100;
    private BoxCollider _collisionCollider { get; set; }
    public BoxCollider CollisionCollider { get => _collisionCollider; set => _collisionCollider = value; }

    public Musicians(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);
        
        Collider = new BoxCollider(new Vector2(Transform.Position.X - PADDING_INTERACTION_AREA / 2, Transform.Position.Y - PADDING_INTERACTION_AREA / 2), Transform.Size.X + PADDING_INTERACTION_AREA, Transform.Size.Y + PADDING_INTERACTION_AREA, CollisionLayer.INTERACT, this);
        CollisionCollider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.WALL, this);
        
        TexRight = managers.SpriteManager.GetTexture("Straßenmusikanten");
        CurrentTex = TexRight;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.White);
    }

    public override void Update(GameTime gameTime)
    {
    }

    public override bool Intersects(ICollider collider)
    {
        return Collider.Intersects(collider);
    }
    
    public bool IsPlayerNearby(Vector2 playerPosition)
    {
        const float MUSIC_RADIUS = 500f; 
        return Vector2.Distance(Transform.Position, playerPosition) < MUSIC_RADIUS;
    }
    
    // following code needed for interaction logic (music change and attack, see GameScreen.cs)
    private bool _overrideActive = false;

    public bool IsOverrideActive => _overrideActive;

    public bool TryTriggerOverride()
    {
        if (_overrideActive)
            return false;

        _overrideActive = true;
        return true;
    }

    public void ResetOverride()
    {
        _overrideActive = false;
    }

}