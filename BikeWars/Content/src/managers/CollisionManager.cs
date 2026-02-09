using System;
using System.Collections.Generic;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Entities.Characters.MapObjects;
using BikeWars.Content.components;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Entities;
using BikeWars.Content.entities.npcharacters;
using BikeWars.Content.entities.items;


namespace BikeWars.Content.managers;

public class CollisionManager
{
    // Events that can be followed by other classes
    public event Action<Player, ItemBase> OnItemPickup;
    public event Action<Player, TowerAlly> OnTowerInteraction;
    public event Action<Player, ObjectBase> OnObjectInteraction;
    public event Action<CharacterBase, ProjectileBase> OnProjectileHit;
    public event Action<CharacterBase, CharacterBase> OnCharacterCollision;

    public event Action<CharacterBase, AreaOfEffectBase> OnAOEHit;
    public event Action<CharacterBase, object> OnTramHit;
    public event Action<CharacterBase, object> OnBaechleHit;

    // car action
    public event Action<CharacterBase, object> OnCarHit;

    public event Action <DestructibleObject, ProjectileBase> OnProjectileHitDestructible;
    public event Action <DestructibleObject, AreaOfEffectBase> OnAOEHitDestructible;

    public List<TiledObjectInfo> ObjectSpawns { get; } = new();
    private readonly GameObjectManager _gameObjectManager;
    public GameObjectManager GameObjectManager => _gameObjectManager;

    private const string MAP = "assets/Map/Bike_Wars_Map";
    private const string TILED_MAP_LAYER = "Collision";

    public int _cellSize { get; set; }

    private SpatialHash _dynamicHash { get; set; }

    public SpatialHash DynamicHash
    {
        get => _dynamicHash;
        set => _dynamicHash = value;
    }

    public HashSet<ICollider> allDynamics = new();
    public HashSet<ICollider> activeDynamics = new();
    private SpatialHash _staticHash { get; set; }

    // Reusable query buffers
    private readonly List<ICollider> _nearbyDynamics = new(64);
    private readonly List<ICollider> _nearbyStatics  = new(64);

    // Reusable removal buffers
    private readonly List<ProjectileBase> _removeProjectiles = new(16);
    private readonly List<ItemBase> _removeItems = new(16);
    private readonly List<CharacterBase> _removeCharacters = new(16);
    private readonly List<Tower> _removeTowers = new(16);

    public SpatialHash StaticHash
    {
        get => _staticHash;
        set => _staticHash = value;
    }

    private TiledMap _tiledMap;

    public TiledMap TiledMap
    {
        get => _tiledMap;
        set => _tiledMap = value;
    }

    private HashSet<ICollider> _toRemoveColliders { get; set; }
    private HashSet<ICollider> _toRemoveStaticColliders { get; set; }
    private List<Rectangle> _toUpdateWalkableRects { get; set; }

    // the grid for the pathfinding
    public Node[,] PathGrid { get; private set; }

    // base (unpadded) walkability snapshot to support cheap local updates
    private bool[,] _baseWalkableGrid;

    // grid data for road tiles
    private bool[,] _roadGrid;
    private readonly HashSet<int> _carRoadGids = new()
    {
        2348
    };
    private readonly List<Point> _roadTiles = new();

    public CollisionManager(int cellSize, int worldBounds, GameObjectManager gameObjectManager)
    {
        _cellSize = cellSize;
        DynamicHash = new SpatialHash(cellSize, worldBounds);
        StaticHash = new SpatialHash(cellSize, worldBounds);
        _toRemoveColliders = new HashSet<ICollider>();
        _toRemoveStaticColliders = new HashSet<ICollider>();
        _toUpdateWalkableRects = new List<Rectangle>();
        _gameObjectManager = gameObjectManager;
    }

    public void LoadContent(ContentManager content)
    {
        TiledMap = content.Load<TiledMap>(MAP);
        var collisionLayer = TiledMap.GetLayer<TiledMapTileLayer>(TILED_MAP_LAYER);
        foreach (var tile in collisionLayer.Tiles)
        {
            if (tile.GlobalIdentifier == 0) continue;

            int x = tile.X * _cellSize;
            int y = tile.Y * _cellSize;

            BoxCollider box = new BoxCollider(new Vector2(x, y), _cellSize, _cellSize, CollisionLayer.WALL, this);
            StaticHash.Insert(box);
        }

        // build pathfinding grid based on collision layer
        int gridWidth = collisionLayer.Width;
        int gridHeight = collisionLayer.Height;
        PathGrid = new Node[gridWidth, gridHeight];
        _baseWalkableGrid = new bool[gridWidth, gridHeight];

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                var tile = collisionLayer.GetTile((ushort)x, (ushort)y);

                bool walkable = tile.GlobalIdentifier == 0;
                PathGrid[x, y] = new Node(x, y, walkable);
                _baseWalkableGrid[x, y] = walkable;
            }
        }
        LoadTerrainLayer("Streets", TerrainType.ROAD);
        LoadTerrainLayer("Floor", TerrainType.GRASS);
        LoadTerrainLayer("Baechle", TerrainType.BAECHLE);
        LoadSpawnLayer("Enemy_Spawn");
        LoadObjectLayer("BIke_Shops_Layer");
        // spawn shops/objects
        _gameObjectManager.SpawnFromTiledObjects(ObjectSpawns);
        // Also load destructible objects from a dedicated Tiled object layer
        LoadObjectLayer("Destructibles");
        _gameObjectManager.SpawnFromTiledObjects(ObjectSpawns);
        LoadTriggerLayer("AchievementsTrigger");
        _gameObjectManager.SpawnFromTiledObjects(ObjectSpawns);
        // spawn shops/objects
        LoadObjectLayer("Chests");
        _gameObjectManager.SpawnFromTiledObjects(ObjectSpawns);
        LoadObjectLayer("Dog-Bowl");
        _gameObjectManager.SpawnFromTiledObjects(ObjectSpawns);
        LoadObjectLayer("Musicians");
        _gameObjectManager.SpawnFromTiledObjects(ObjectSpawns);
        LoadObjectLayer("Tower");
        _gameObjectManager.SpawnFromTiledObjects(ObjectSpawns);

        // Load world border collision (non-destructible static colliders)
        LoadWorldBorderCollision();

        // Insert any statics registered by the GameObjectManager (e.g. destructibles)
        foreach (var s in _gameObjectManager.Statics)
        {
            StaticHash.Insert(s);
        }

        // Mark destructible items as non-walkable in the base grid, then pad once
        if (PathGrid != null && _baseWalkableGrid != null)
        {
            foreach (var obj in _gameObjectManager.Objects)
            {
                if (obj is DestructibleObject d)
                {
                    SetBaseWalkableForRect(d.Transform.Bounds, false);
                }
                else if (obj is BikeShop b)
                {
                    SetBaseWalkableForRect(b.Transform.Bounds, false);
                }
            }

            foreach (var tower in _gameObjectManager.Towers)
            {
                SetBaseWalkableForRect(tower.Transform.Bounds, false);
            }

            ApplyGlobalPaddingFromBase();
        }
    }

    // checks if a given grid position is a road tile
    public bool IsRoadTile(Point g)
    {
        if (_roadGrid == null) return false;

        int w = _roadGrid.GetLength(0);
        int h = _roadGrid.GetLength(1);

        if (g.X < 0 || g.X >= w || g.Y < 0 || g.Y >= h) return false;

        return _roadGrid[g.X, g.Y];
    }

    // returns true if the world position is on a road
    public bool IsRoadWorld(Vector2 worldPos)
    {
        int gid = GetTileGidAtWorld("Streets", worldPos);
        return IsRealRoadGid(gid);
    }

    // checks whether a tile id is one of the allowed road tiles
    private bool IsRealRoadGid(int gid)
    {
        return gid == 2348;
    }

    // converts a world position to grid coordinates and returns that tile’s GID
    public int GetTileGidAtWorld(string layerName, Vector2 worldPos)
    {
        var layer = TiledMap.GetLayer<TiledMapTileLayer>(layerName);

        if (layer == null)
            return 0;

        Point g = WorldToGrid(worldPos);

        if (g.X < 0 || g.X >= layer.Width || g.Y < 0 || g.Y >= layer.Height)
            return 0;

        var tile = layer.GetTile((ushort)g.X, (ushort)g.Y);
        return tile.GlobalIdentifier;
    }


    // random road spawn
    public Vector2 GetRandomRoadWorldCenter(Random rng)
    {
        if (_roadTiles.Count == 0)
            return Vector2.Zero;

        var g = _roadTiles[rng.Next(_roadTiles.Count)];
        return new Vector2(
            g.X * _cellSize + _cellSize / 2f,
            g.Y * _cellSize + _cellSize / 2f
        );
    }

    private void SetBaseWalkableForRect(Rectangle rect, bool walkable)
    {
        if (_baseWalkableGrid == null) return;

        int width = _baseWalkableGrid.GetLength(0);
        int height = _baseWalkableGrid.GetLength(1);

        int startX = Math.Max(0, rect.Left / _cellSize);
        int startY = Math.Max(0, rect.Top / _cellSize);
        int endX = Math.Min(width - 1, (rect.Right - 1) / _cellSize);
        int endY = Math.Min(height - 1, (rect.Bottom - 1) / _cellSize);

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                _baseWalkableGrid[x, y] = walkable;
            }
        }
    }

    private void ApplyGlobalPaddingFromBase(int padding = 1)
    {
        if (PathGrid == null || _baseWalkableGrid == null) return;

        int width = PathGrid.GetLength(0);
        int height = PathGrid.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                PathGrid[x, y].Walkable = _baseWalkableGrid[x, y];
            }
        }

        WallPadding(padding);
    }

    private void ApplyWalkabilityChange(Rectangle rect, bool walkable, bool fullRepad)
    {
        SetBaseWalkableForRect(rect, walkable);

        if (fullRepad)
        {
            ApplyGlobalPaddingFromBase();
        }
        else
        {
            UpdatePaddedRegion(rect);
        }
    }

    private void UpdatePaddedRegion(Rectangle rect, int padding = 1)
    {
        if (PathGrid == null || _baseWalkableGrid == null) return;

        int width = PathGrid.GetLength(0);
        int height = PathGrid.GetLength(1);
        int margin = padding * 2; // include neighbours that previously got padded

        int startX = Math.Max(0, rect.Left / _cellSize - margin);
        int startY = Math.Max(0, rect.Top / _cellSize - margin);
        int endX = Math.Min(width - 1, (rect.Right - 1) / _cellSize + margin);
        int endY = Math.Min(height - 1, (rect.Bottom - 1) / _cellSize + margin);

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                PathGrid[x, y].Walkable = _baseWalkableGrid[x, y];
            }
        }

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                if (!_baseWalkableGrid[x, y])
                {
                    PadLocalNeighbours(x, y, startX, startY, endX, endY, padding);
                }
            }
        }
    }

    private void PadLocalNeighbours(int x, int y, int startX, int startY, int endX, int endY, int padding)
    {
        for (int dy = -padding; dy <= padding; dy++)
        {
            for (int dx = -padding; dx <= padding; dx++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx < startX || nx > endX || ny < startY || ny > endY)
                    continue;

                if (nx < 0 || nx >= PathGrid.GetLength(0) || ny < 0 || ny >= PathGrid.GetLength(1))
                    continue;

                PathGrid[nx, ny].Walkable = false;
            }
        }
    }

    // takes a world position in pixels (Vector2) and returns which tile that position is inside
    public Point WorldToGrid(Vector2 worldPos)
    {
        int gridX = (int)(worldPos.X / _cellSize);
        int gridY = (int)(worldPos.Y / _cellSize);

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
    private bool[,] CopyWalkableGrid()
    {
        int width = PathGrid.GetLength(0);
        int height = PathGrid.GetLength(1);

        bool[,] inflated = new bool[width, height];

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

        int[,] dirs = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

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
        if (a is CircleCollider circle && b is BoxCollider box)
        {
            return CircleVsBox(circle, box);
        }

        if (a is CircleCollider c1 && b is CircleCollider c2)
            return CircleVsCircle(c1, c2);

        if (a is BoxCollider b1 && b is BoxCollider b2)
            return BoxVsBox(b1, b2);
        return Vector2.Zero;
    }

    private Vector2 CircleVsCircle(CircleCollider a, CircleCollider b)
    {
        Vector2 delta = b.Center() - a.Center();
        float dist = delta.Length();

        float penetration = (a.Radius + b.Radius) - dist;
        if (penetration <= 0)
            return Vector2.Zero;

        if (dist == 0)
            return new Vector2(0, -penetration);

        return Vector2.Normalize(delta) * penetration;
    }

    private Vector2 CircleVsBox(CircleCollider circle, BoxCollider box)
    {
        float dx = box.Center().X - circle.Center().X;
        float dy = box.Center().Y - circle.Center().Y;

        float px = (circle.Radius /2f + box.Width / 2f) - Math.Abs(dx);
        float py = (circle.Radius /2f+ box.Height / 2f) - Math.Abs(dy);

        if (px <= 0 || py <= 0)
            return Vector2.Zero;
        px = Math.Max(px, 0f);
        py = Math.Max(py, 0f);

        // Extra buffer to ensure characters are definitely separated
        float buffer = 1.0f;

        if (px < py)
            return new Vector2(Math.Sign(dx) * (px + buffer), 0);
        else
            return new Vector2(0, Math.Sign(dy) * (py + buffer));
    }

    private Vector2 BoxVsBox(BoxCollider A, BoxCollider B)
    {
        Vector2 aCenter = A.Center();
        Vector2 bCenter = B.Center();

        float dx = bCenter.X - aCenter.X;
        float dy = bCenter.Y - aCenter.Y;

        float px = (A.Width / 2f + B.Width / 2f) - Math.Abs(dx);
        float py = (A.Height / 2f + B.Height / 2f) - Math.Abs(dy);

        if (px <= 0 || py <= 0)
            return Vector2.Zero;

        if (px < py)
            return new Vector2(Math.Sign(dx) * px, 0);
        else
            return new Vector2(0, Math.Sign(dy) * py);
    }

    public void AddDynamic(ICollider c)
    {
        DynamicHash.Insert(c);
        allDynamics.Add(c);
    }

    public void Insertions(List<ItemBase> items, HashSet<Player> players, List<ProjectileBase> projectiles,
        List<AreaOfEffectBase> aoeAttacks, List<CharacterBase> characters, List<Tram> trams, List<Car> cars)
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

        foreach (ProjectileBase p in projectiles)
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

        foreach (CharacterBase c in characters)
        {
            AddDynamic(c.Collider);
            if (c.IsDead)
            {
                _toRemoveColliders.Add(c.Collider);
            }
        }


        foreach (var t in trams)
        {
            foreach (var col in t.Colliders)
            {
                AddDynamic(col);
            }
        }

        // handle car collisions
        foreach (var car in cars)
        {
            if (car?.Collider != null)
                AddDynamic(car.Collider);
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

    private void HandleCharacterWithStatic(ICollider b, ICollider c)
    {
        // Ignore SPAWNENEMIES layer for characters
        if (b.Layer == CollisionLayer.SPAWNENEMIES) return;
        if (c.Layer != CollisionLayer.CHARACTER && c.Layer != CollisionLayer.PLAYER) return;
        Vector2 penetration = GetPenetrationVector(c, b);
        if (penetration.LengthSquared() < 0.0001f)
            return;
        CharacterBase ch = (CharacterBase)c.Owner;
        ch.Transform.Position -= penetration;
        ch.UpdateCollider(c.Layer);
    }

    private void HandleProjectileWithStatic(ICollider b, ICollider c)
    {
        if (b.Layer == CollisionLayer.WALL && c.Layer == CollisionLayer.PROJECTILE)
        {
            if (!c.Intersects(b)) return;

            ProjectileBase p = (ProjectileBase)c.Owner;

            // prevent multiple hits from the same projectile in one frame
            if (p.HasHit)
            {
                return;
            }

            // If the wall belongs to a destructible map object, apply damage
            if (b.Owner is DestructibleObject destructible)
            {
                OnProjectileHitDestructible?.Invoke((DestructibleObject)b.Owner, p);
                _toRemoveColliders.Add(p.Collider);

                // if destroyed: defer static collider removal and defer path grid update
                if (destructible.Health <= 0)
                {
                    // defer path grid walkability update for this object's area
                    _toUpdateWalkableRects.Add(destructible.Transform.Bounds);

                    // queue static collider for removal at end of frame
                    _toRemoveStaticColliders.Add(b);

                    // remove drawable/game object now
                    _gameObjectManager.RemoveObject(destructible);

                    // Notify enemies to recalculate paths (they will see grid changes after deferred apply)
                    _gameObjectManager.NotifyPathGridChanged();
                }

                return;
            }

            // default: projectile hits a normal wall -> destroy projectile
            _toRemoveColliders.Add(p.Collider);
        }
    }

    private void HandleAOEWithStatic(ICollider b, ICollider c)
    {
        if (b.Layer != CollisionLayer.WALL || c.Layer != CollisionLayer.AOE)
        {
            return;
        }

        if (!c.Intersects(b))
        {
            return;
        }

        if (b.Owner is not DestructibleObject destructible)
        {
            return;
        }

        if (c.Owner is not AreaOfEffectBase aoe)
        {
            return;
        }

        if (!aoe.CanDamageObject(destructible))
        {
            return;
        }

        OnAOEHitDestructible?.Invoke((DestructibleObject)b.Owner, aoe);

        if (destructible.Health <= 0)
        {
            _toUpdateWalkableRects.Add(destructible.Transform.Bounds);
            _toRemoveStaticColliders.Add(b);
            _gameObjectManager.RemoveObject(destructible);
            _gameObjectManager.NotifyPathGridChanged();
        }
    }

    private void HandleStatics(ICollider c, List<ICollider> statics)
    {
        foreach (var b in statics)
        {
            HandleCharacterWithStatic(b, c);
            HandleProjectileWithStatic(b, c);
            HandleProjectileWithTower(b, c);
            HandleAOEWithStatic(b, c);
            HandleAOEWithTower(b, c);
        }
    }

    private void HandleAOEWithTower(ICollider towerCol, ICollider aoeCol)
    {
        if (towerCol.Layer != CollisionLayer.TOWER ||
            aoeCol.Layer != CollisionLayer.AOE)
            return;

        if (!towerCol.Intersects(aoeCol))
            return;

        Tower tower = (Tower)towerCol.Owner;
        AreaOfEffectBase aoe = (AreaOfEffectBase)aoeCol.Owner;

        if (!aoe.CanDamageObject(tower))
            return;

        tower.TakeDamage(aoe.Damage);
    }

    private void HandleDynamics(ICollider c, List<ICollider> dynamics)
    {
        foreach (var d in dynamics)
        {
            PickingUpItem(c, d);
            HandleInteractions(c, d);
            HandleTrigger(c, d);
            HandleInteractionsTower(c, d);
            HandleCharacters(c, d);
            HandleTowers(c, d);
            HandleTramCollision(c, d);
            HandleCarCollision(c, d);
        }
    }

    private void HandleTramCollision(ICollider c, ICollider d)
    {
        if (c.Layer == CollisionLayer.TRAM && (d.Layer == CollisionLayer.CHARACTER || d.Layer == CollisionLayer.PLAYER))
        {
            if (c.Intersects(d))
            {
                OnTramHit?.Invoke((CharacterBase)d.Owner, c.Owner);
            }
        }
    }

    // car collision
    private void HandleCarCollision(ICollider c, ICollider d)
    {
        if (c.Layer == CollisionLayer.CAR &&
            (d.Layer == CollisionLayer.CHARACTER || d.Layer == CollisionLayer.PLAYER))
        {
            if (c.Intersects(d))
            {
                OnCarHit?.Invoke((CharacterBase)d.Owner, c.Owner);
            }
        }

        if (d.Layer == CollisionLayer.CAR &&
            (c.Layer == CollisionLayer.CHARACTER || c.Layer == CollisionLayer.PLAYER))
        {
            if (c.Intersects(d))
            {
                OnCarHit?.Invoke((CharacterBase)c.Owner, d.Owner);
            }
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
        if (d.Layer != CollisionLayer.CHARACTER && d.Layer != CollisionLayer.PLAYER) return;
        if (c==d) return;
        if (c.GetHashCode() > d.GetHashCode()) return;
        Vector2 penetration = GetPenetrationVector(c, d);
        if (penetration.LengthSquared() < 0.0001f)
            return;
        Vector2 separation = penetration * 0.25f;

        const float SLOP = 0.01f;
        if (penetration.LengthSquared() < SLOP * SLOP)
            return;

        CharacterBase ch = (CharacterBase)c.Owner;
        CharacterBase chd = (CharacterBase)d.Owner;

        ch.Transform.Position -= separation;
        chd.Transform.Position += separation;
        OnCharacterCollision?.Invoke(ch, chd);

        ch.UpdateCollider(c.Layer);
        chd.UpdateCollider(d.Layer);
    }
    private void HandleCharacterProjectiles(ICollider c, ICollider d)
    {
        if (d.Layer != CollisionLayer.PROJECTILE || !c.Intersects(d)) return;
        ProjectileBase p = (ProjectileBase)d.Owner;

        // Make sure projectile cannot hit more than once
        if (p.HasHit)
        {
            _toRemoveColliders.Add(p.Collider);
            return;
        }

        // Ignore self-hit
        if (c.Owner == p.Owner) return;
        if (p.Owner is Tower && c.Owner is Player) return;
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

    private void HandleProjectileWithTower(ICollider towerCol, ICollider projCol)
    {
        if (towerCol.Layer != CollisionLayer.TOWER ||
            projCol.Layer != CollisionLayer.PROJECTILE)
            return;

        if (!towerCol.Intersects(projCol)) return;

        Tower tower = (Tower)towerCol.Owner;
        ProjectileBase p = (ProjectileBase)projCol.Owner;

        if (p.HasHit) return;

        // Don't let projectiles hit the tower that fired them
        if (p.Owner == tower) return;

        tower.TakeDamage(p.weaponAttributes.Damage);
        p.HasHit = true;

        _toRemoveColliders.Add(p.Collider);
    }

    private void HandleCharacters(ICollider c, ICollider d)
    {
        if (c.Layer != CollisionLayer.CHARACTER && c.Layer != CollisionLayer.PLAYER) return;
        HandleCharacterCollision(c, d);
        HandleCharacterProjectiles(c, d);
        // AOE damage handling
        if (d.Layer != CollisionLayer.AOE) return;
        AreaOfEffectBase aoe = (AreaOfEffectBase)d.Owner;

        // prevent hitting yourself
        if (aoe.Owner == c.Owner) return;

        if (c.Intersects(d))
        {
            CharacterBase ch = (CharacterBase)c.Owner;

            // Only apply damage if enough time has passed (once per DamageInterval)
            if (aoe.CanDamage(ch))
            {
                // Call proper AOE damage event
                OnAOEHit?.Invoke(ch, aoe);
            }

            if (ch.IsDead)
            {
                _toRemoveColliders.Add(ch.Collider);
            }
        }

        return; // don't run projectile logic
    }
    private void HandleTowers(ICollider c, ICollider d)
    {
        if (c.Layer != CollisionLayer.TOWER) return;

        HandleProjectileWithTower(c, d);
        // AOE damage handling
        if (d.Layer != CollisionLayer.AOE) return;

        AreaOfEffectBase aoe = (AreaOfEffectBase)d.Owner;
        if (aoe.Owner == c.Owner) return; // prevent hitting yourself

        if (c.Intersects(d))
        {
            CharacterBase ch = (CharacterBase)c.Owner;

            // Only apply damage if enough time has passed (once per DamageInterval)
            if (aoe.CanDamage(ch))
            {
                // Call proper AOE damage event
                OnAOEHit?.Invoke(ch, aoe);
            }

            if (ch.IsDead)
            {
                _toRemoveColliders.Add(ch.Collider);
            }
        }

        return; // don't run projectile logic
    }

    private void HandleTerrain(ICollider c, List<ICollider> statics)
    {
        if (c.Owner is not Player player) return;

        player.CurrentTerrain = null;

        foreach (var s in statics)
        {
            if (s.Layer == CollisionLayer.TERRAIN && s.Intersects(c))
            {
                player.CurrentTerrain = (TerrainCollider)s;

                if (player.CurrentTerrain.TerrainType == TerrainType.BAECHLE)
                {
                    OnBaechleHit?.Invoke(player, s.Owner);
                }
                return;
            }
        }
    }

    // This one will be used for checking with INTERACT CollisionLayers and with dynamic ones
    private void HandleInteractions(ICollider c, ICollider d)
    {
        if (c.Layer == CollisionLayer.PLAYER && d.Layer == CollisionLayer.INTERACT && c.Intersects(d))
        {
            if (c.Owner is Player p && d.Owner is ObjectBase i)
            {
                OnObjectInteraction?.Invoke(p, i);
            }
        }
    }

    // Use this if you need to handle a new Achievement or something similar. Like walking on something
    private void HandleTrigger(ICollider c, ICollider d)
    {
        if (c.Layer == CollisionLayer.PLAYER && d.Layer == CollisionLayer.TRIGGER && c.Intersects(d))
        {
            if (c.Owner is Player p && d.Owner is ObjectBase i)
            {
                OnObjectInteraction?.Invoke(p, i);
            }
        }
    }
    private void HandleInteractionsTower(ICollider c, ICollider d)
    {
        if (c.Layer == CollisionLayer.PLAYER && d.Layer == CollisionLayer.INTERACT && c.Intersects(d))
        {
            if (c.Owner is Player p && d.Owner is TowerAlly ta)
            {
                OnTowerInteraction?.Invoke(p, ta);
            }
        }
    }
    public void Update(HashSet<Player> players, List<ItemBase> items, List<ProjectileBase> projectiles,
        List<AreaOfEffectBase> aoeAttacks, List<CharacterBase> characters, List<Tram> trams, List<Car> cars, List<ObjectBase> objects, List<Tower> towers)
    {
        allDynamics.Clear();
        DynamicHash.Clear();
        Insertions(items, players, projectiles, aoeAttacks, characters, trams, cars);

        foreach (var c in allDynamics)
        {
            if (c.Layer != CollisionLayer.CHARACTER && c.Layer != CollisionLayer.PLAYER &&
                c.Layer != CollisionLayer.PROJECTILE && c.Layer != CollisionLayer.TRAM &&
                c.Layer != CollisionLayer.AOE && c.Layer != CollisionLayer.CAR)
            {
                continue;
            }
            _nearbyDynamics.Clear();
            _nearbyStatics.Clear();
            DynamicHash.QueryNearby(c.Position, 1, _nearbyDynamics);
            StaticHash.QueryNearby(c.Position, 2, _nearbyStatics);

            HandleTerrain(c, _nearbyStatics);
            HandleStatics(c, _nearbyStatics);
            HandleDynamics(c, _nearbyDynamics);
        }

        _removeProjectiles.Clear();
        _removeItems.Clear();
        _removeCharacters.Clear();
        _removeTowers.Clear();

        foreach (var c in _toRemoveColliders)
        {
            switch (c.Owner)
            {
                case ProjectileBase p: _removeProjectiles.Add(p); break;
                case ItemBase i: _removeItems.Add(i); break;
                case CharacterBase ch: _removeCharacters.Add(ch); break;
                case Tower t: _removeTowers.Add(t); break;
            }
        }

        foreach (ProjectileBase p in _removeProjectiles)
        {
            if (_toRemoveColliders.Contains(p.Collider))
            {
                projectiles.Remove(p);
            }
        }

        foreach (ItemBase i in _removeItems)
        {
            if (_toRemoveColliders.Contains(i.Collider))
            {
                items.Remove(i);
            }
        }

        foreach (CharacterBase ch in _removeCharacters)
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

        foreach (var c in _toRemoveStaticColliders)
        {
            StaticHash.Remove(c);
            _gameObjectManager.Statics.Remove((BoxCollider)c);
        }

        foreach (ICollider c in _removeTowers)
        {
            StaticHash.Remove(c);
            _gameObjectManager.Statics.Remove((BoxCollider)c);
        }

        // Apply deferred grid updates (localized to avoid full-grid stalls)
        foreach (var rect in _toUpdateWalkableRects)
        {
            ApplyWalkabilityChange(rect, true, fullRepad: false);
        }

        _toRemoveColliders.Clear();
        _toRemoveStaticColliders.Clear();
        _toUpdateWalkableRects.Clear();
    }

    // makes the hitboxes visible for when in the tech demo
    public void DrawHitboxes(SpriteBatch spriteBatch, Texture2D pixel,
        Player player, List<CharacterBase> characters,
        List<ItemBase> items, List<ProjectileBase> projectiles, List<AreaOfEffectBase> aoeAttacks, List<Tram> trams, List<Car> cars, List<ObjectBase> objects, List<Tower> towers)
    {
        foreach (var cell in StaticHash._cells)
        {
            foreach (var box in cell.Value.Colliders)
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
            var playerCirc = GetColliderCircle(player.Collider);
            DrawCircleOutline(spriteBatch, pixel, player.Collider.Center(), playerCirc.Radius, Color.Red * 0.7f);

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

            var charCirc = GetColliderCircle(character.Collider);
            DrawCircleOutline(spriteBatch, pixel, character.Collider.Center(), charCirc.Radius, Color.Red * 0.7f);
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

        //Objects hitboxes
        foreach (var obj in objects)
        {
            if (obj?.Collider != null)
            {
                var objRect = GetColliderRectangle(obj.Collider);
                DrawRectOutline(spriteBatch, pixel, objRect, Color.Red * 0.7f);
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

        // Tram hitboxes
        foreach (var tram in trams)
        {
            foreach (var collider in tram.Colliders)
            {
                var tramRect = GetColliderRectangle(collider);
                DrawRectOutline(spriteBatch, pixel, tramRect, Color.Red * 0.7f);
            }
        }

        // draw car hitboxes
        foreach (var car in cars)
        {
            if (car?.Collider == null) continue;
            var carRect = GetColliderRectangle(car.Collider);
            DrawRectOutline(spriteBatch, pixel, carRect, Color.Yellow * 0.7f);
        }

        foreach (var tower in towers)
        {
            if (tower?.Collider != null)
            {
                var interactRect = GetColliderRectangle(tower.Collider);
                DrawRectOutline(spriteBatch, pixel, interactRect, Color.Red * 0.7f);
            }

            // Draw collision hitbox if TowerAlly
            if (tower is TowerAlly towerAlly && towerAlly.CollisionCollider != null)
            {
                var collisionRect = GetColliderRectangle(towerAlly.CollisionCollider);
                DrawRectOutline(spriteBatch, pixel, collisionRect, Color.Red * 0.7f);

                // Draw attack range circle
                Vector2 center = towerAlly.Transform.Bounds.Center.ToVector2();
                DrawCircleOutline(spriteBatch, pixel, center, towerAlly.Attributes.AttackRange, Color.Orange * 0.5f);
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

    private Circle GetColliderCircle(ICollider collider)
    {
        if (collider is CircleCollider circle)
        {
            return new Circle(
                circle.Position.X,
                circle.Position.Y,
                circle.Radius
            );
        }

        return Circle.Empty;
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

    private void DrawCircleOutline(
        SpriteBatch spriteBatch,
        Texture2D pixel,
        Vector2 center,
        float radius,
        Color color,
        int segments = 32)
    {
        if (radius <= 0) return;

        Vector2 prevPoint = center + new Vector2(radius, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = MathHelper.TwoPi * i / segments;
            Vector2 nextPoint = center + new Vector2(
                MathF.Cos(angle) * radius,
                MathF.Sin(angle) * radius
            );
            DrawLine(spriteBatch, pixel, prevPoint, nextPoint, color);
            prevPoint = nextPoint;
        }
    }

    private void DrawLine(
        SpriteBatch spriteBatch,
        Texture2D pixel,
        Vector2 start,
        Vector2 end,
        Color color)
    {
        Vector2 edge = end - start;
        float angle = MathF.Atan2(edge.Y, edge.X);

        spriteBatch.Draw(
            pixel,
            new Rectangle(
                (int)start.X,
                (int)start.Y,
                (int)edge.Length(),
                1),
            null,
            color,
            angle,
            Vector2.Zero,
            SpriteEffects.None,
            0
        );
    }

    private void LoadTerrainLayer(string layerName, TerrainType type)
    {
        var layer = TiledMap.GetLayer<TiledMapTileLayer>(layerName);
        if (layer == null)
            return;

        // init road grid data
        if (type == TerrainType.ROAD && _roadGrid == null)
        {
            _roadGrid = new bool[layer.Width, layer.Height];
            _roadTiles.Clear();
        }

        foreach (var tile in layer.Tiles)
        {
            if (tile.GlobalIdentifier == 0)
                continue;

            int x = tile.X * _cellSize;
            int y = tile.Y * _cellSize;

            // remember road tiles
            int gid = tile.GlobalIdentifier;

            if (type == TerrainType.ROAD)
            {
                if (!IsRealRoadGid(gid))
                    continue;

                _roadGrid[tile.X, tile.Y] = true;
                _roadTiles.Add(new Point(tile.X, tile.Y));
            }

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

    private void LoadTriggerLayer(string layerName)
    {
        ObjectSpawns.Clear();
        var layer = TiledMap.GetLayer<TiledMapObjectLayer>(layerName);
        if (layer == null)
            return;

        foreach (var obj in layer.Objects)
        {
            var rect = new Rectangle(
                (int)obj.Position.X,
                (int)obj.Position.Y,
                (int)obj.Size.Width,
                (int)obj.Size.Height
            );
            ObjectSpawns.Add(new TiledObjectInfo(obj.Name, rect, obj.Properties));
        }
    }

    private void LoadObjectLayer(string layerName)
    {
        ObjectSpawns.Clear();

        var objLayer = TiledMap.GetLayer<TiledMapObjectLayer>(layerName);

        foreach (var obj in objLayer.Objects)
        {
            var rect = new Rectangle(
                (int)obj.Position.X,
                (int)obj.Position.Y,
                (int)obj.Size.Width,
                (int)obj.Size.Height
            );

            ObjectSpawns.Add(new TiledObjectInfo(obj.Name, rect, obj.Properties));
        }
    }
    public void Unload()
    {
        DynamicHash?.Clear();
        StaticHash?.Clear();

        allDynamics.Clear();
        activeDynamics.Clear();

        _nearbyDynamics.Clear();
        _nearbyStatics.Clear();

        _toRemoveColliders.Clear();
        _toRemoveStaticColliders.Clear();
        _toUpdateWalkableRects.Clear();

        _roadTiles.Clear();
        ObjectSpawns.Clear();
    }

    private void LoadWorldBorderCollision()
    {
        try
        {
            var objLayer = TiledMap.GetLayer<TiledMapObjectLayer>("worldborder_collision");
            if (objLayer == null) return;

            foreach (var obj in objLayer.Objects)
            {
                var rect = new Rectangle(
                    (int)obj.Position.X,
                    (int)obj.Position.Y,
                    (int)obj.Size.Width,
                    (int)obj.Size.Height
                );

                // Create a static collider for this world border area
                BoxCollider borderCollider = new BoxCollider(
                    new Vector2(rect.X, rect.Y),
                    rect.Width,
                    rect.Height,
                    CollisionLayer.WALL,
                    this
                );

                StaticHash.Insert(borderCollider);

                // Mark as non-walkable in the pathfinding grid
                SetBaseWalkableForRect(rect, false);
            }
        }
        catch
        {
            // Layer might not exist; continue without it
        }
    }

    public void DrawLayerDebug(
        SpriteBatch spriteBatch,
        Texture2D pixel,
        string layerName,
        Color color,
        float alpha = 0.35f)
    {
        if (TiledMap == null) return;
        var layer = TiledMap.GetLayer<TiledMapTileLayer>(layerName);
        if (layer == null) return;

        foreach (var tile in layer.Tiles)
        {
            if (tile.GlobalIdentifier == 0) continue;

            int x = tile.X * _cellSize;
            int y = tile.Y * _cellSize;

            spriteBatch.Draw(
                pixel,
                new Rectangle(x, y, _cellSize, _cellSize),
                color * alpha
            );
        }
    }

    public void RegisterStaticWorld(
        IEnumerable<ObjectBase> objects,
        IEnumerable<Tower> towers)
    {
        foreach (ObjectBase o in objects)
            StaticHash.Insert(o.Collider);

        foreach (Tower t in towers)
        {
            if (t is TowerAlly ta)
                StaticHash.Insert(ta.CollisionCollider);
            else
                StaticHash.Insert(t.Collider);
        }
    }
}
