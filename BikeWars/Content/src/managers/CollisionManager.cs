using System;
using System.Collections.Generic;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.managers;
public class CollisionManager
{
    // Events that can be followed by other classes
    public event Action<Player, ItemBase> OnItemPickup;
    public event Action<CharacterBase, ProjectileBase> OnProjectileHit;
    public event Action<CharacterBase, CharacterBase> OnCharacterCollision;

    private const string MAP = "assets/Map/Bike_Wars_Map";
    private const string TILED_MAP_LAYER = "Collision";
    private const int CELL_SIZE = 16;
    private const float BOUNCE = 0.001f;

    private SpatialHash _dynamicHash {get; set;}
    public SpatialHash DynamicHash {get => _dynamicHash; set => _dynamicHash = value;}
    private SpatialHash _staticHash {get; set;}
    public SpatialHash StaticHash {get => _staticHash; set => _staticHash = value;}

    private TiledMap _tiledMap;
    public TiledMap TiledMap {get => _tiledMap; set => _tiledMap = value;}
    private List<BoxCollider> _collisionBoxes {get; set;} // Mainly used for the static layout
    public List<BoxCollider> CollisionBoxes {get => _collisionBoxes; set => _collisionBoxes = value;}
    public CollisionManager(int cellSize, float insertRadius)
    {
        DynamicHash = new SpatialHash(cellSize, insertRadius);
        StaticHash = new SpatialHash(cellSize, insertRadius);
        CollisionBoxes = new List<BoxCollider>();
    }
    public bool isColliding(ICollider collisionBox1, ICollider collisionBox2)
    {
        return collisionBox1.Intersects(collisionBox2);
    }

    public void LoadContent(ContentManager content)
    {
        TiledMap = content.Load<TiledMap>(MAP);
        var collisionLayer = TiledMap.GetLayer<TiledMapTileLayer>(TILED_MAP_LAYER);
        foreach (var tile in collisionLayer.Tiles)
        {
            if(tile.GlobalIdentifier == 0) continue;

            int x = tile.X * CELL_SIZE;
            int y = tile.Y * CELL_SIZE;

            BoxCollider box = new BoxCollider(new Vector2(x, y), CELL_SIZE, CELL_SIZE, CollisionLayer.WALL, this);
            CollisionBoxes.Add(box);
            StaticHash.Insert(box);
        }
    }

    // Should be helpful to make two colliders bounce away in a good distance so it doesn't stuck together
    public Vector2 GetPenetrationVector(ICollider a, ICollider b)
    {

        // TODO if we want to use Circle too we need to implement it
        if (a is BoxCollider bca && b is BoxCollider bcb)
        {
            // Calculate center to center
            float aWidthHalf = bca.Width / 2;
            float aHeightHalf = bca.Height / 2;
            float bWidthHalf = bcb.Width / 2;
            float bHeightHalf = bcb.Height / 2;

            Vector2 aCenter = new Vector2(a.Position.X + aWidthHalf, a.Position.Y + aHeightHalf);
            Vector2 bCenter = new Vector2(b.Position.X + bWidthHalf, b.Position.Y + bHeightHalf);

            float dx = bCenter.X - aCenter.X;
            float dy = bCenter.Y - aCenter.Y;
            float px = aWidthHalf  + bWidthHalf  - Math.Abs(dx);
            float py = aHeightHalf  + bHeightHalf  - Math.Abs(dy);

            if (px <= 0 || py <= 0) {
                return Vector2.Zero; // No collision
            }

            if (px < py)
            {
                return new Vector2(dx < 0 ? -px : px, 0);
            }
            else
            {
                return new Vector2(0, dy < 0 ? -py : py);
            }
        }
        return new Vector2(0,0);
    }

    private void Insertions(List<ItemBase> items, Player player, List<ProjectileBase> projectiles, List<CharacterBase> characters)
    {
        foreach (var c in items)
        {
            DynamicHash.Insert(c.Collider);
        }
        DynamicHash.Insert(player.Collider);
        foreach(ProjectileBase p in projectiles)
        {
            DynamicHash.Insert(p.Collider);
        }
        foreach(CharacterBase c in characters)
        {
            DynamicHash.Insert(c.Collider);
        }
    }

    private void HandleStatics(ICollider c, List<ICollider> statics, List<ProjectileBase> toRemoveProjectiles)
    {
        foreach (var b in statics)
        {
            if (c.Layer == CollisionLayer.CHARACTER || c.Layer == CollisionLayer.PLAYER)
            {
                if (c.Intersects(b))
                {
                    CharacterBase ch = (CharacterBase)c.Owner;
                    ch.SetLastTransform();
                    ch.UpdateCollider();
                }
            }

            if (c.Layer == CollisionLayer.PROJECTILE)
            {
                if (c.Intersects(b))
                {
                    ProjectileBase p = (ProjectileBase)c.Owner;
                    toRemoveProjectiles.Add(p);
                }
            }
        }
    }
    private void HandleDynamics(ICollider c, List<ICollider> dynamics, List<ItemBase> toRemoveItems, List<ProjectileBase> toRemoveProjectils, List<CharacterBase> toRemoveCharacters)
    {
        foreach (var d in dynamics)
        {
            if (c.Layer == CollisionLayer.PLAYER)
            {
                if (d.Layer == CollisionLayer.ITEM)
                {
                    if (c.Intersects(d))
                    {
                        // Event for picking up items
                        OnItemPickup?.Invoke((Player)c.Owner, (ItemBase)d.Owner);

                        toRemoveItems.Add((ItemBase)d.Owner);
                    }
                }
            }
            if (c.Layer == CollisionLayer.CHARACTER || c.Layer == CollisionLayer.PLAYER)
            {
                if (d.Layer == CollisionLayer.CHARACTER || d.Layer == CollisionLayer.PLAYER)
                {
                    if (c.Intersects(d) && c.Owner != d.Owner)
                    {
                        // Event for two Characters directly colliding (Close combat)
                        OnCharacterCollision?.Invoke((CharacterBase)c.Owner, (CharacterBase)d.Owner);

                        CharacterBase ch = (CharacterBase)c.Owner;
                        CharacterBase chd = (CharacterBase)d.Owner;
                        Vector2 t = GetPenetrationVector(c, d);
                        ch.Transform.Position -= t * BOUNCE;
                        chd.Transform.Position += t * BOUNCE;
                        ch.UpdateCollider();
                        chd.UpdateCollider();
                    }
                }
                if (d.Layer == CollisionLayer.PROJECTILE)
                {
                    if (c.Intersects(d))
                    {
                        ProjectileBase p = (ProjectileBase)d.Owner;

                        // Make sure projectile cannot hit more than once
                        if (p.HasHit)
                        {
                            toRemoveProjectils.Add(p);
                            break;
                        }

                        // Ignore self-hit
                        if (c.Owner == p.Owner)
                        {
                            break;
                        }

                        // Event for a character or player gets hit by projectile
                        OnProjectileHit?.Invoke((CharacterBase)c.Owner, (ProjectileBase)d.Owner);

                        CharacterBase ch = (CharacterBase)c.Owner;

                        if(ch.IsDead)
                            toRemoveCharacters.Add(ch);

                        p.HasHit = true;
                        toRemoveProjectils.Add(p);
                    }
                }
            }
        }
    }

    public void Update(Player player, List<ItemBase> items, List<ProjectileBase> projectiles, List<CharacterBase> characters)
    {
        DynamicHash.Clear();
        Insertions(items, player, projectiles, characters);
        List<ProjectileBase> toRemoveProjectiles = new List<ProjectileBase>();
        List<ItemBase> toRemoveItems = new List<ItemBase>();
        List<CharacterBase> toRemoveCharacters = new List<CharacterBase>();
        foreach (ICollider c in DynamicHash.AllColliders())
        {
            List<ICollider> statics = StaticHash.QueryNearby(c.Position);
            List<ICollider> dynamics = DynamicHash.QueryNearby(c.Position);
            HandleStatics(c, statics, toRemoveProjectiles);
            HandleDynamics(c, dynamics, toRemoveItems, toRemoveProjectiles, toRemoveCharacters);
        }
        foreach (ProjectileBase p in toRemoveProjectiles)
        {
            projectiles.Remove(p);
        }
        foreach (ItemBase i in toRemoveItems)
        {
            items.Remove(i);
        }
        foreach (CharacterBase c in toRemoveCharacters)
        {
            characters.Remove(c);
        }
    }

    public void DrawHitboxes(SpriteBatch spriteBatch, Texture2D pixel)
    {
        // BoxCollider stores position + width/height, we convert to a Rectangle
        foreach (var box in _collisionBoxes)
        {
            var rect = new Rectangle(
                (int)box.Position.X,
                (int)box.Position.Y,
                (int)box.Width,
                (int)box.Height
            );
            DrawRectOutline(spriteBatch, pixel, rect, Color.Red * 0.7f);
        }
    }

    private void DrawRectOutline(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rect, Color color)
    {
        // top
        spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), color);
        // bottom
        spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), color);
        // left
        spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), color);
        // right
        spriteBatch.Draw(pixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), color);
    }
    
    
    // This is old code to have some logic movement for the units
    // If the hobo hits a wall, push him back/sideways and start sidestepping.
            // foreach (var box in _collisionManager.CollisionBoxes)
            // {
            //     if (hobo.Intersects(box))
            //     {
            //         var dir = hobo.Movement.Direction;

            //         if (dir != Vector2.Zero)
            //         {
            //             Vector2 rightNudge = new Vector2(dir.Y, -dir.X);

            //             if (rightNudge != Vector2.Zero)
            //             {
            //                 rightNudge.Normalize();
            //                 hobo.Transform.Position += rightNudge * SideNudgeStrength;
            //             }
            //         }

            //         hobo.UpdateCollider();

            //         if (hobo.Movement.State == EnemyState.Chasing)
            //         {
            //             hobo.Movement.StartSidestepping(hobo.Movement.Direction);
            //         }

            //         break;
            //     }
            // }

            // If the BikeThief hits a wall, push him back/sideways and start sidestepping.
            // foreach (var box in _collisionManager.CollisionBoxes)
            // {
            //     if (bikethief.Intersects(box))
            //     {
            //         var dir = bikethief.Movement.Direction;

            //         if (dir != Vector2.Zero)
            //         {
            //             Vector2 rightNudge = new Vector2(dir.Y, -dir.X);

            //             if (rightNudge != Vector2.Zero)
            //             {
            //                 rightNudge.Normalize();
            //                 bikethief.Transform.Position += rightNudge * SideNudgeStrength;
            //             }
            //         }

            //         bikethief.UpdateCollider();

            //         if (bikethief.Movement.State == EnemyState.Chasing)
            //         {
            //             bikethief.Movement.StartSidestepping(bikethief.Movement.Direction);
            //         }

            //         break;
            //     }
            // }
}