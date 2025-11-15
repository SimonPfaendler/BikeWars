using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework.Content;
using System;

namespace BikeWars.Content.entities.items;
// XP Basisklasse, die in den einzelnen XP Klassen benutzt wird
public abstract class Xp: ItemBase, IPickable
{
    private BoxCollider _collider { get; set; }
    public override BoxCollider Collider
    {
        get { return _collider; }
    }
    public int xp_value { get; protected set; }
    protected abstract string TEXTURE_PATH { get; }
    private float _pulseTimer = 0f;
    private Color _currentColor;

    private static readonly Color PulseColorA = Color.White;
    private static readonly Color PulseColorB = Color.LimeGreen;
    private const float PULSE_SPEED = 4f;

    public Xp(Vector2 start, Point size, int xp_value)
    {
        Transform = new Transform(start, size);
        _collider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X,
            Transform.Size.Y);
        xp_value = xp_value;
        _currentColor = PulseColorA;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(CurrentTex, Transform.Bounds, _currentColor);
    }

    public override void Update(GameTime gameTime) // Color is changing for better visibility
    {
        _pulseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        float t = (float)(Math.Sin(_pulseTimer * PULSE_SPEED) * 0.5f + 0.5f);
        _currentColor = Color.Lerp(PulseColorA, PulseColorB, t);
    }
    public override void LoadContent(ContentManager content)
    {
        TexRight = content.Load<Texture2D>(TEXTURE_PATH);
        CurrentTex = TexRight;
    }
    public override bool Intersects(ICollider collider)
    {
        return _collider.Intersects(collider);
    }
}