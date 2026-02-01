using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.entities.MapObjects;
// if DogBowl is active, Dogs targrt the Dog Bowl (which is handeld in Dog.cs) only one DogBowl can be active at once
public class DogBowl: ObjectBase
{
    private bool _full;
    public bool Full => _full;
    private readonly Texture2D _texEmpty;
    private readonly Texture2D _texFull;

    private int PADDING_INTERACTION_AREA = 40;
    private static readonly CooldownWithDuration _bowlCooldown =
        new CooldownWithDuration(durationSeconds: 20f, cooldownSeconds: 5f);

    public static bool BowlIsActive => _bowlCooldown.IsActive;
    public static int RemainingTotalSeconds =>
        (int)Math.Ceiling(_bowlCooldown.RemainingCooldown + _bowlCooldown.RemainingDuration);
    public static bool Ready => _bowlCooldown.Ready;
    public static Vector2 BowlPosition { get; private set; }

    public DogBowl(Vector2 start, Point size, bool full = false)
    {
        _full = full;
        Transform = new Transform(start, size);
        Collider = new BoxCollider(new Vector2(Transform.Position.X - PADDING_INTERACTION_AREA / 2, Transform.Position.Y - PADDING_INTERACTION_AREA / 2), Transform.Size.X + PADDING_INTERACTION_AREA, Transform.Size.Y + PADDING_INTERACTION_AREA, CollisionLayer.INTERACT, this);
        _texEmpty = managers.SpriteManager.GetTexture("Dog_Bowl");
        _texFull  = managers.SpriteManager.GetTexture("Dog_Bowl_full");
        if (_full)
        {CurrentTex = _texFull;}
        else
        {
            CurrentTex = _texEmpty;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.White);
    }

    public override void Update(GameTime gameTime)
    {
        if (_full && !_bowlCooldown.IsActive)
        {
            _full = false;
            CurrentTex = _texEmpty;
        }
    }
    public override bool Intersects(ICollider collider)
    {
        return Collider.Intersects(collider);
    }

    public bool TryActivateDogBowl()
    {
        if (_bowlCooldown.Ready)
        {
            return true;
        }
        return false;
    }

    public void ActivateDogBowl(Vector2 bowlposition)
    {
        BowlPosition = bowlposition;
        _bowlCooldown.Activate();
    }

    public void FillUpDogBowl()
    {
        if (_full) return;
        _full = true;
        CurrentTex = _texFull;
    }

    public static void UpdateBowl(GameTime gameTime)
    {
        _bowlCooldown.Update(gameTime);
    }


}