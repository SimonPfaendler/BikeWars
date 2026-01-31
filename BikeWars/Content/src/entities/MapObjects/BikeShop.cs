using System;
using BikeWars.Content.components;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Entities.Characters.MapObjects;

public class BikeShop: ObjectBase
{
    private BoxCollider _collisionCollider {get;set;}
    private CooldownWithDuration _shopCooldown = new CooldownWithDuration(1, 10);
    public bool ShopReady => _shopCooldown.Ready;
    private int _timeleft = 0;
    public new BoxCollider CollisionCollider {get => _collisionCollider; set => _collisionCollider = value; } // Now this collider is for collision

    private int PADDING_INTERACTION_AREA = 40;
    public BikeShop(Vector2 start, Point size)
    {
        Transform = new Transform(start, size);
        Collider = new BoxCollider(new Vector2(Transform.Position.X - PADDING_INTERACTION_AREA / 2, Transform.Position.Y - PADDING_INTERACTION_AREA / 2), Transform.Size.X + PADDING_INTERACTION_AREA, Transform.Size.Y + PADDING_INTERACTION_AREA, CollisionLayer.INTERACT, this);
        CollisionCollider = new BoxCollider(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size.X, Transform.Size.Y, CollisionLayer.WALL, this);
        CurrentTex = SpriteManager.GetTexture("Fahrradwerkstatt");
    }

    public void SetCooldown(int time)
    {
        _shopCooldown = new CooldownWithDuration(1, time);
        _shopCooldown.Activate();
    }

    public override void Update(GameTime gameTime)
    {
        _shopCooldown.Update(gameTime);
        _timeleft = (int)Math.Ceiling(_shopCooldown.RemainingCooldown);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!ShopReady)
        {
            spriteBatch.Draw(CurrentTex, Transform.Bounds, Color.Red);
            DrawTimeLeft(spriteBatch);
        }
        else
        {
            base.Draw(spriteBatch);
        }
    }

    public void DrawTimeLeft(SpriteBatch spriteBatch)
    {
        string text = $"{_timeleft}";
        
        
        float scale = 1.5f;
        Vector2 textSize = UIAssets.DefaultFont.MeasureString(text) * scale;
        
        Vector2 shopTopCenter = new Vector2(
            Transform.Bounds.Center.X,
            Transform.Bounds.Top
        );

        Vector2 textPos = new Vector2(
            shopTopCenter.X - textSize.X / 2f,
            shopTopCenter.Y - textSize.Y - 10f
        );
        
        spriteBatch.DrawString(UIAssets.DefaultFont, text, textPos, Color.Red, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}