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

    private int _cellSize {get; set;}
    private SpatialHash _dynamicHash {get; set;}
    public SpatialHash DynamicHash {get => _dynamicHash; set => _dynamicHash = value;}
    private SpatialHash _staticHash {get; set;}
    public SpatialHash StaticHash {get => _staticHash; set => _staticHash = value;}

    private TiledMap _tiledMap;
    public TiledMap TiledMap {get => _tiledMap; set => _tiledMap = value;}
    private List<BoxCollider> _collisionBoxes {get; set;} // Mainly used for the static layout
    public List<BoxCollider> CollisionBoxes {get => _collisionBoxes; set => _collisionBoxes = value;}
    private HashSet<Point> _streetTiles = new HashSet<Point>();

    public CollisionManager(int cellSize, int worldBounds)
    {
        _cellSize = cellSize;
        DynamicHash = new SpatialHash(cellSize, worldBounds);
        StaticHash = new SpatialHash(cellSize, worldBounds);
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

            int x = tile.X * _cellSize;
            int y = tile.Y * _cellSize;

            BoxCollider box = new BoxCollider(new Vector2(x, y), _cellSize, _cellSize, CollisionLayer.WALL, this);
            CollisionBoxes.Add(box);
            StaticHash.Insert(box);
        }
        LoadTerrainLayer("Streets", TerrainType.ROAD);
        LoadTerrainLayer("Background_graas", TerrainType.GRASS);
    }

    public Vector2 GetPenetrationVector(ICollider a, ICollider b)
    {
        const float SEPARATION = 1.5f; // fixed Push
        if (a is BoxCollider A && b is BoxCollider B)
        {
            Vector2 aCenter = A.Position + new Vector2(A.Width / 2f, A.Height / 2f);
            Vector2 bCenter = B.Position + new Vector2(B.Width / 2f, B.Height / 2f);

            float dx = bCenter.X - aCenter.X;
            float dy = bCenter.Y - aCenter.Y;

            float px = (A.Width / 2f + B.Width / 2f) - Math.Abs(dx);
            float py = (A.Height / 2f + B.Height / 2f) - Math.Abs(dy);

            if (px <= 0 || py <= 0)
                return Vector2.Zero;

            if (px < py)
            {
                // horizontal push
                return new Vector2(Math.Sign(dx) * SEPARATION, 0);
            }
            else
            {
                // vertical push
                return new Vector2(0, Math.Sign(dy) * SEPARATION);
            }
        }

        return Vector2.Zero;
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

    private void HandleCharacterWithStatic(ICollider b, ICollider c)
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
    }
    private void HandleProjectileWithStatic(ICollider b, ICollider c, List<ProjectileBase> toRemoveProjectiles)
    {
        if (b.Layer == CollisionLayer.WALL && c.Layer == CollisionLayer.PROJECTILE)
        {
            if (c.Intersects(b))
            {
                ProjectileBase p = (ProjectileBase)c.Owner;
                toRemoveProjectiles.Add(p);
            }
        }
    }

    private void HandleStatics(ICollider c, List<ICollider> statics, List<ProjectileBase> toRemoveProjectiles)
    {
        foreach (var b in statics)
        {
            HandleCharacterWithStatic(b, c);
            HandleProjectileWithStatic(b, c, toRemoveProjectiles);
        }
    }
    private void HandleDynamics(ICollider c, List<ICollider> dynamics, List<ItemBase> toRemoveItems, List<ProjectileBase> toRemoveProjectiles, List<CharacterBase> toRemoveCharacters)
    {
        foreach (var d in dynamics)
        {
            PickingUpItem(c, d, toRemoveItems);
            HandleCharacters(c, d, toRemoveProjectiles, toRemoveCharacters);
        }
    }

    private void PickingUpItem(ICollider c, ICollider d, List<ItemBase> toRemoveItems)
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
    }

    private void HandleCharacterCollision(ICollider c, ICollider d)
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

                ch.SetLastTransform();
                ch.Transform.Position -= t;
                ch.UpdateCollider();
                chd.SetLastTransform();
                chd.Transform.Position += t;
                chd.UpdateCollider();
            }
        }
    }

    private void HandleCharacterProjectiles(ICollider c, ICollider d, List<ProjectileBase> toRemoveProjectiles, List<CharacterBase> toRemoveCharacters)
    {
        if (d.Layer == CollisionLayer.PROJECTILE)
        {
            if (c.Intersects(d))
            {
            ProjectileBase p = (ProjectileBase)d.Owner;

            // Make sure projectile cannot hit more than once
            if (p.HasHit)
            {
                toRemoveProjectiles.Add(p);
                return;
            }

            // Ignore self-hit
            if (c.Owner == p.Owner)
            {
                return;
            }

            // Event for a character or player gets hit by projectile
            OnProjectileHit?.Invoke((CharacterBase)c.Owner, (ProjectileBase)d.Owner);

            CharacterBase ch = (CharacterBase)c.Owner;
            if(ch.IsDead)
                toRemoveCharacters.Add(ch);

                p.HasHit = true;
                toRemoveProjectiles.Add(p);
            }
        }
    }

    private void HandleCharacters(ICollider c, ICollider d, List<ProjectileBase> toRemoveProjectiles, List<CharacterBase> toRemoveCharacters)
    {
        if (c.Layer == CollisionLayer.CHARACTER || c.Layer == CollisionLayer.PLAYER)
        {
            HandleCharacterCollision(c, d);
            HandleCharacterProjectiles(c, d, toRemoveProjectiles, toRemoveCharacters);
        }
    }

    private void HandleTerrain(ICollider c, List<ICollider> statics)
    {
        if (c.Owner is not Player player)
            return;


        player.CurrentTerrain = null;

        foreach (var s in statics)
        {
            if (s.Layer == CollisionLayer.TERRAIN)
            {

                if (s.Intersects(c))
                {
                    player.CurrentTerrain = (TerrainCollider)s;
                    return;
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
        foreach (KeyValuePair<int, CellData> cell in DynamicHash._cells)
        {
            foreach(var c in cell.Value.Colliders)
            {
                List<ICollider> dynamics = DynamicHash.QueryNearby(c.Position);
                List<ICollider> statics = StaticHash.QueryNearby(c.Position);
                HandleDynamics(c, dynamics, toRemoveItems, toRemoveProjectiles, toRemoveCharacters);
                HandleStatics(c, statics, toRemoveProjectiles);
                HandleTerrain(c, statics);
            }
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

    // makes the hitboxes visible for when in the tech demo
    // makes the hitboxes visible for when in the tech demo
public void DrawHitboxes(SpriteBatch spriteBatch, Texture2D pixel, 
                         Player player, List<CharacterBase> characters, 
                         List<ItemBase> items, List<ProjectileBase> projectiles)
{
    // Static collision boxes
    foreach (var box in _collisionBoxes)
    {
        var rect = new Rectangle(
            (int)box.Position.X,
            (int)box.Position.Y,
            box.Width,
            box.Height
        );
        DrawRectOutline(spriteBatch, pixel, rect, Color.Red * 0.7f);
    }

    // Player hitbox
    if (player?.Collider != null)
    {
        var playerRect = GetColliderRectangle(player.Collider);
        DrawRectOutline(spriteBatch, pixel, playerRect, Color.Red * 0.7f);
        
        // Draw a small indicator for player position
        spriteBatch.Draw(pixel, 
            new Rectangle((int)player.Transform.Position.X - 2, 
                         (int)player.Transform.Position.Y - 2, 4, 4), 
            Color.Lime);
    }

    // NPC/Character hitboxes
    foreach (var character in characters)
    {
        if (character?.Collider != null)
        {
            var charRect = GetColliderRectangle(character.Collider);
            DrawRectOutline(spriteBatch, pixel, charRect, Color.Red * 0.7f);
        }
    }

    // Item hitboxes
    foreach (var item in items)
    {
        if (item?.Collider != null)
        {
            var itemRect = GetColliderRectangle(item.Collider);
            DrawRectOutline(spriteBatch, pixel, itemRect, Color.Red * 0.7f);
        }
    }

    // Projectile hitboxes
    foreach (var projectile in projectiles)
    {
        if (projectile?.Collider != null)
        {
            var projRect = GetColliderRectangle(projectile.Collider);
            DrawRectOutline(spriteBatch, pixel, projRect, Color.Red * 0.7f);
        }
    }   
}

    // Helper method to convert ICollider to Rectangle
    private Rectangle GetColliderRectangle(ICollider collider)
    {
        if (collider is BoxCollider box)
        {
            return new Rectangle(
                (int)box.Position.X,
                (int)box.Position.Y,
                box.Width,
                box.Height
            );
        }
        return Rectangle.Empty;
    }

    private void DrawRectOutline(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rect, Color color)
    {
        if (rect.Width <= 0 || rect.Height <= 0) return;
        
        // top
        spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), color);
        // bottom
        spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), color);
        // left
        spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), color);
        // right
        spriteBatch.Draw(pixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), color);
    }
    private void LoadTerrainLayer(string layerName, TerrainType type)
    {
        var layer = TiledMap.GetLayer<TiledMapTileLayer>(layerName);
        if (layer == null)
            return;

        foreach (var tile in layer.Tiles)
        {
            if (tile.GlobalIdentifier == 0)
                continue;

            int x = tile.X * _cellSize;
            int y = tile.Y * _cellSize;

            TerrainCollider tc = new TerrainCollider(
                new Vector2(x, y),
                _cellSize,
                _cellSize,
                type
            );

            StaticHash.Insert(tc);
        }
    }

}