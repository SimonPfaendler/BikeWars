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
    public event Action<CharacterBase, AreaOfEffectBase> OnAOEHit;

    private const string MAP = "assets/Map/Bike_Wars_Map";
    private const string TILED_MAP_LAYER = "Collision";

    public int _cellSize {get; set;}

    private SpatialHash _dynamicHash {get; set;}
    public SpatialHash DynamicHash {get => _dynamicHash; set => _dynamicHash = value;}
    public HashSet<ICollider> allDynamics = new();
    public HashSet<ICollider> activeDynamics = new();
    private SpatialHash _staticHash {get; set;}
    public SpatialHash StaticHash {get => _staticHash; set => _staticHash = value;}

    private TiledMap _tiledMap;
    public TiledMap TiledMap {get => _tiledMap; set => _tiledMap = value;}
    private HashSet<ICollider> _toRemoveColliders {get; set;}

    // the grid for the pathfinding
    public Node[,] PathGrid { get; private set; }

    public CollisionManager(int cellSize, int worldBounds)
    {
        _cellSize = cellSize;
        DynamicHash = new SpatialHash(cellSize, worldBounds);
        StaticHash = new SpatialHash(cellSize, worldBounds);
        _toRemoveColliders = new HashSet<ICollider>();
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
            StaticHash.Insert(box);
        }

        // build pathfinding grid based on collision layer
        int gridWidth = collisionLayer.Width;
        int gridHeight = collisionLayer.Height;
        PathGrid = new Node[gridWidth, gridHeight];

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                var tile = collisionLayer.GetTile((ushort) x, (ushort) y);

                bool walkable = tile.GlobalIdentifier == 0;
                PathGrid[x, y] = new Node(x, y, walkable);
            }
        }

        WallPadding();
        LoadTerrainLayer("Streets", TerrainType.ROAD);
        LoadTerrainLayer("Floor", TerrainType.GRASS);
        LoadSpawnLayer("Enemy_Spawn");
    }

    // takes a world position in pixels (Vector2) and returns which tile that position is inside
    public Point WorldToGrid(Vector2 worldPos)
    {
        int gridX = (int) (worldPos.X / _cellSize);
        int gridY = (int) (worldPos.Y / _cellSize);

        return new Point(gridX, gridY);
    }

    // takes a Node (which stores grid X/Y tile coordinates)
    // and returns the world-space pixel position at the center of that tile.
    public Vector2 GridToWorldCenter(Node node)
    {
        return new Vector2(
            node.X * _cellSize + _cellSize / 2f,
            node.Y * _cellSize + _cellSize / 2f
        );
    }

    // sets the neighbours of an unwalkable tile
    // so that the enemies don't get stuck on edges of hitboxes
    private void WallPadding(int padding = 1)
    {
        bool[,] copy = CopyWalkableGrid();
        InflateWalls(copy, padding);
        WriteInflatedGrid(copy);
    }

    // copy walkable grid
    private bool [,] CopyWalkableGrid()
    {
        int width = PathGrid.GetLength(0);
        int height = PathGrid.GetLength(1);

        bool [,] inflated =  new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                inflated[x, y] = PathGrid[x, y].Walkable;
            }
        }
        return inflated;
    }

    // inflate the walls of the unwalkable nodes
    private void InflateWalls(bool[,] inflated, int padding)
    {
        int width = PathGrid.GetLength(0);
        int height = PathGrid.GetLength(1);

        int [,] dirs = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!PathGrid[x, y].Walkable)
                {
                    for (int dy = -padding; dy <= padding; dy++)
                    {
                        for (int dx = -padding; dx <= padding; dx++)
                        {
                            if (dx == 0 && dy == 0)
                                continue;

                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                                continue;

                            if (!inflated[nx, ny])
                                continue;

                            inflated[nx, ny] = false;
                        }
                    }
                }
            }
        }
    }

    // write the padding into the grid
    private void WriteInflatedGrid(bool[,] inflated)
    {
        int width = PathGrid.GetLength(0);
        int height = PathGrid.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                PathGrid[x, y].Walkable = inflated[x, y];
            }
        }
    }

    public Vector2 GetPenetrationVector(ICollider a, ICollider b)
    {
        const float SEPARATION = 1.25f; // fixed Push
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

    public void AddDynamic(ICollider c)
    {
        DynamicHash.Insert(c);
        allDynamics.Add(c);
    }

    public void Insertions(HashSet<ItemBase> items, HashSet<Player> players, HashSet<ProjectileBase> projectiles, HashSet<AreaOfEffectBase> aoeAttacks, HashSet<CharacterBase> characters)
    {
        foreach (ItemBase c in items)
        {
            AddDynamic(c.Collider);
        }
        foreach (var p in players)
        {
            if (p != null)
            {
                AddDynamic(p.Collider);
            }
        }
        foreach(ProjectileBase p in projectiles)
        {
            AddDynamic(p.Collider);
        }
        foreach (var aoe in aoeAttacks)
        {
            foreach (var hitbox in aoe.GetHitboxes())
            {
                AddDynamic(hitbox);
            }
        }
        foreach(CharacterBase c in characters)
        {
            AddDynamic(c.Collider);
        }
    }

    public BoxCollider Projected(BoxCollider original, Vector2 delta)
    {
        return new BoxCollider(
            original.Position + delta,
            original.Width,
            original.Height,
            original.Layer,
            original.Owner
        );
    }

    public bool WillCollide(BoxCollider moving, Vector2 delta, ICollider obstacle)
    {
        var projected = Projected(moving, delta);
        return projected.Intersects(obstacle);
    }

    private void HandleCharacterWithStatic(ICollider b, ICollider c)
    {
        // Ignore SPAWNENEMIES layer for characters
        if (b.Layer == CollisionLayer.SPAWNENEMIES) return;

        if (c.Layer == CollisionLayer.CHARACTER || c.Layer == CollisionLayer.PLAYER)
        {
            CharacterBase ch = (CharacterBase)c.Owner;
            Vector2 delta = ch.Transform.Position - ch.LastTransform.Position;

            if (delta.X != 0)
            {
                if (WillCollide((BoxCollider)c, new Vector2(delta.X, 0), b))
                {
                    ch.SetLastTransform();
                    ch.Transform.Position = new Vector2(
                        ch.LastTransform.Position.X,
                        ch.Transform.Position.Y
                    );
                }
            }

            ch.UpdateCollider();
            if (delta.Y != 0)
            {
                if (WillCollide((BoxCollider)c, new Vector2(0, delta.Y), b))
                {
                    ch.SetLastTransform();
                    ch.Transform.Position = new Vector2(
                        ch.Transform.Position.X,
                        ch.LastTransform.Position.Y
                    );
                }
            }
            ch.UpdateCollider();
        }
    }

    private void HandleProjectileWithStatic(ICollider b, ICollider c)
    {
        if (b.Layer == CollisionLayer.WALL && c.Layer == CollisionLayer.PROJECTILE)
        {
            if (c.Intersects(b))
            {
                ProjectileBase p = (ProjectileBase)c.Owner;
                _toRemoveColliders.Add(p.Collider);
            }
        }
    }

    private void HandleStatics(ICollider c, HashSet<ICollider> statics)
    {
        foreach (var b in statics)
        {
            HandleCharacterWithStatic(b, c);
            HandleProjectileWithStatic(b, c);
        }
    }
    private void HandleDynamics(ICollider c, HashSet<ICollider> dynamics)
    {
        foreach (var d in dynamics)
        {
            PickingUpItem(c, d);
            HandleCharacters(c, d);
        }
    }

    private void PickingUpItem(ICollider c, ICollider d)
    {
        if (c.Layer == CollisionLayer.PLAYER && d.Layer == CollisionLayer.ITEM && c.Intersects(d))
        {
            if (c.Owner is Player p && d.Owner is ItemBase i)
                OnItemPickup?.Invoke(p, i);
        }
    }
    public void OnRemoveItem(ItemBase item)
    {
        _toRemoveColliders.Add(item.Collider);
        allDynamics.Remove(item.Collider);
    }

    private void HandleCharacterCollision(ICollider c, ICollider d)
    {
        if (c == d || c.GetHashCode() > d.GetHashCode() || (d.Layer != CollisionLayer.CHARACTER && d.Layer != CollisionLayer.PLAYER))
        {
            return;
        }

        CharacterBase ch = (CharacterBase)c.Owner;
        Vector2 delta = ch.Transform.Position - ch.LastTransform.Position;

        if (!WillCollide((BoxCollider)c, delta, d))
        {
            return;
        }

        OnCharacterCollision?.Invoke((CharacterBase)c.Owner, (CharacterBase)d.Owner);
        Vector2 t = GetPenetrationVector(c, d);
        CharacterBase chd = (CharacterBase)d.Owner;


        if (t.LengthSquared() < 0.01f)
            return;
        ch.SetLastTransform();
        ch.Transform.Position -= t;
        ch.UpdateCollider();

        chd.SetLastTransform();
        chd.UpdateCollider();
    }

    private void HandleCharacterProjectiles(ICollider c, ICollider d)
    {
        if (d.Layer != CollisionLayer.PROJECTILE || !c.Intersects(d))
        {
            return;
        }
        ProjectileBase p = (ProjectileBase)d.Owner;

        // Make sure projectile cannot hit more than once
        if (p.HasHit)
        {
            _toRemoveColliders.Add(p.Collider);
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
        if (ch.IsDead)
        {
            _toRemoveColliders.Add(ch.Collider);
        }
        p.HasHit = true;
        _toRemoveColliders.Add(p.Collider);
    }

    private void HandleCharacters(ICollider c, ICollider d)
    {
        if (c.Layer != CollisionLayer.CHARACTER && c.Layer != CollisionLayer.PLAYER)
        {
            return;
        }
        HandleCharacterCollision(c, d);
        HandleCharacterProjectiles(c, d);
        // AOE damage handling
        if (d.Layer != CollisionLayer.AOE)
        {
            return;
        }
        AreaOfEffectBase aoe = (AreaOfEffectBase)d.Owner;

        // prevent hitting yourself
        if (aoe.Owner == c.Owner)
            return;

        if (c.Intersects(d))
        {
            CharacterBase ch = (CharacterBase)c.Owner;

            // Call proper AOE damage event
            OnAOEHit?.Invoke(ch, aoe);

            if (ch.IsDead)
            {
                _toRemoveColliders.Add(ch.Collider);
            }
        }
        return; // don't run projectile logic
    }

    private void HandleTerrain(ICollider c, HashSet<ICollider> statics)
    {
        if (c.Owner is not Player player)
            return;

        player.CurrentTerrain = null;

        foreach (var s in statics)
        {
            if (s.Layer == CollisionLayer.TERRAIN && s.Intersects(c))
            {
                player.CurrentTerrain = (TerrainCollider)s;
                return;
            }
        }
    }

    public void Update(HashSet<Player> players, HashSet<ItemBase> items, HashSet<ProjectileBase> projectiles, HashSet<AreaOfEffectBase> aoeAttacks, HashSet<CharacterBase> characters)
    {
        allDynamics.Clear();
        DynamicHash.Clear();
        Insertions(items, players, projectiles, aoeAttacks, characters);

        foreach (var c in allDynamics)
        {
            if (c.Layer != CollisionLayer.CHARACTER && c.Layer != CollisionLayer.PLAYER && c.Layer != CollisionLayer.PROJECTILE)
            {
                continue;
            }
            var dynamics = DynamicHash.QueryNearby(c.Position, 1);
            var statics  = StaticHash.QueryNearby(c.Position, 2);

            HandleDynamics(c, dynamics);
            HandleStatics(c, statics);
            HandleTerrain(c, statics);
        }

        HashSet<ProjectileBase> removeProjectiles = new();
        HashSet<ItemBase> removeItems = new();
        HashSet<CharacterBase> removeCharacters = new();

        foreach (var c in _toRemoveColliders)
        {
            switch (c.Owner)
            {
                case ProjectileBase p: removeProjectiles.Add(p); break;
                case ItemBase i: removeItems.Add(i); break;
                case CharacterBase ch: removeCharacters.Add(ch); break;
            }
        }
        foreach (ProjectileBase p in removeProjectiles)
        {
            if (_toRemoveColliders.Contains(p.Collider))
            {
                projectiles.Remove(p);
            }
        }

        foreach (ItemBase i in removeItems)
        {
            if (_toRemoveColliders.Contains(i.Collider))
            {
                items.Remove(i);
            }
        }

        foreach (CharacterBase ch in removeCharacters)
        {
            if (_toRemoveColliders.Contains(ch.Collider))
            {
                characters.Remove(ch);
            }
        }

        foreach (var c in _toRemoveColliders)
        {
            DynamicHash.Remove(c);
            allDynamics.Remove(c);
        }
        _toRemoveColliders.Clear();
    }

    // makes the hitboxes visible for when in the tech demo
    public void DrawHitboxes(SpriteBatch spriteBatch, Texture2D pixel,
                            Player player, HashSet<CharacterBase> characters,
                            HashSet<ItemBase> items, HashSet<ProjectileBase> projectiles, HashSet<AreaOfEffectBase> aoeAttacks)
    {
        foreach (var cell in StaticHash._cells)
        {
            foreach(var box in cell.Value.Colliders)
            {
                if (box.Layer != CollisionLayer.WALL)
                {
                    continue;
                }
                var rect = new Rectangle(
                    (int)box.Position.X,
                    (int)box.Position.Y,
                    box.Width,
                    box.Height
                );
                DrawRectOutline(spriteBatch, pixel, rect, Color.Red * 0.7f);
            }
        }

        // AOE hitboxes
        foreach (var aoe in aoeAttacks)
        {
            foreach (var hitbox in aoe.GetHitboxes())
            {
                Rectangle rect = GetColliderRectangle(hitbox);
                DrawRectOutline(spriteBatch, pixel, rect, Color.Red * 0.7f);
            }
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
            if (character?.Collider == null)
            {
                continue;
            }
            var charRect = GetColliderRectangle(character.Collider);
            DrawRectOutline(spriteBatch, pixel, charRect, Color.Red * 0.7f);
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
    private void LoadSpawnLayer(string layerName)
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

            BoxCollider spawnCollider = new BoxCollider(
                new Vector2(x, y),
                _cellSize,
                _cellSize,
                CollisionLayer.SPAWNENEMIES,
                this
            );

            StaticHash.Insert(spawnCollider);
        }
    }
}

