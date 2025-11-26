using System;
using System.Collections.Generic;
using System.Numerics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled;

namespace BikeWars.Content.managers;
public class CollisionManager
{
    private const string MAP = "assets/Map/Bike_Wars_Map";
    private const string TILED_MAP_LAYER = "Collision";
    private const int CELL_SIZE = 16;
    private const float BOUNCE = 0.025f;

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

    private void Insertions(List<ItemBase> items, Player player, Hobo hobo, BikeThief bikeThief, List<ProjectileBase> projectiles)
    {
        foreach (var c in items)
        {
            DynamicHash.Insert(c.Collider);
        }
        DynamicHash.Insert(player.Collider);
        DynamicHash.Insert(hobo.Collider);
        DynamicHash.Insert(bikeThief.Collider);
        foreach(ProjectileBase p in projectiles)
        {
            DynamicHash.Insert(p.Collider);
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
    private void HandleDynamics(ICollider c, List<ICollider> dynamics, List<ItemBase> toRemoveItems, List<ProjectileBase> toRemoveProjectils)
    {
        foreach (var d in dynamics)
        {
            if (c.Layer == CollisionLayer.PLAYER)
            {
                if (d.Layer == CollisionLayer.ITEM)
                {
                    if (c.Intersects(d))
                    {
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
                        if (p.HasHit)
                        {
                            toRemoveProjectils.Add(p);
                            break;
                        }
                        CharacterBase ch = (CharacterBase)c.Owner;

                        if (c.Owner == p.Owner) // The shot came from this character
                        {
                            break;
                        }
                        ch.TakeDamage(p.Damage);
                        p.HasHit = true;
                        toRemoveProjectils.Add(p);
                    }
                }
            }
        }
    }

    public void Update(Player player, Hobo hobo, BikeThief bikeThief, List<ItemBase> items, List<ProjectileBase> projectiles)
    {
        DynamicHash.Clear();
        Insertions(items, player, hobo, bikeThief, projectiles);
        List<ProjectileBase> toRemoveProjectiles = new List<ProjectileBase>();
        List<ItemBase> toRemoveItems = new List<ItemBase>();
        foreach (ICollider c in DynamicHash.AllColliders())
        {
            List<ICollider> statics = StaticHash.QueryNearby(c.Position);
            List<ICollider> dynamics = DynamicHash.QueryNearby(c.Position);
            HandleStatics(c, statics, toRemoveProjectiles);
            HandleDynamics(c, dynamics, toRemoveItems, toRemoveProjectiles);
        }
        foreach (ProjectileBase p in toRemoveProjectiles)
        {
            projectiles.Remove(p);
        }
        foreach (ItemBase i in toRemoveItems)
        {
            items.Remove(i);
        }
    }
}