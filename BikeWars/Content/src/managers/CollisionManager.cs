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
    private SpatialHash _staticHash {get; set;}
    public SpatialHash StaticHash {get => _staticHash; set => _staticHash = value;}

    private TiledMap _tiledMap;
    public TiledMap TiledMap {get => _tiledMap; set => _tiledMap = value;}
    private List<BoxCollider> _collisionBoxes {get; set;} // Mainly used for the static layout
    public List<BoxCollider> CollisionBoxes {get => _collisionBoxes; set => _collisionBoxes = value;}
    private List<ICollider> _toRemoveColliders {get; set;}

    // the grid for the pathfinding
    public Node[,] PathGrid { get; private set; }

    public CollisionManager(int cellSize, int worldBounds)
    {
        _cellSize = cellSize;
        DynamicHash = new SpatialHash(cellSize, worldBounds);
        StaticHash = new SpatialHash(cellSize, worldBounds);
        CollisionBoxes = new List<BoxCollider>();
        _toRemoveColliders = new List<ICollider>();
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

    private void Insertions(List<ItemBase> items, List<Player> players, List<ProjectileBase> projectiles, List<AreaOfEffectBase> aoeAttacks, List<CharacterBase> characters)
    {
        foreach (var c in items)
        {
            DynamicHash.Insert(c.Collider);
        }
        foreach (var p in players)
        {
            if (p != null) DynamicHash.Insert(p.Collider);
        }
        foreach(ProjectileBase p in projectiles)
        {
            DynamicHash.Insert(p.Collider);
        }
        foreach (var aoe in aoeAttacks)
        {
            foreach (var hitbox in aoe.GetHitboxes())
                DynamicHash.Insert(hitbox);
        }
        foreach(CharacterBase c in characters)
        {
            DynamicHash.Insert(c.Collider);
        }
    }

    private void HandleCharacterWithStatic(ICollider b, ICollider c)
    {
        // Ignore SPAWNENEMIES layer for characters
        if (b.Layer == CollisionLayer.SPAWNENEMIES) return;

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

    private void HandleStatics(ICollider c, List<ICollider> statics)
    {
        foreach (var b in statics)
        {
            HandleCharacterWithStatic(b, c);
            HandleProjectileWithStatic(b, c);
        }
    }
    private void HandleDynamics(ICollider c, List<ICollider> dynamics)
    {
        foreach (var d in dynamics)
        {
            PickingUpItem(c, d);
            HandleCharacters(c, d);
        }
    }

    private void PickingUpItem(ICollider c, ICollider d)
    {
        if (c.Layer == CollisionLayer.PLAYER)
        {
            if (d.Layer == CollisionLayer.ITEM)
            {
                if (c.Intersects(d))
                {
                    // Event for picking up items
                    OnItemPickup?.Invoke((Player)c.Owner, (ItemBase)d.Owner);
                }
            }
        }
    }
    public void OnRemoveItem(ItemBase item)
    {
        _toRemoveColliders.Add(item.Collider);
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

    // private void HandleCharacterProjectiles(ICollider c, ICollider d, List<ProjectileBase> toRemoveProjectiles, List<CharacterBase> toRemoveCharacters)
    private void HandleCharacterProjectiles(ICollider c, ICollider d)
    {
        if (d.Layer == CollisionLayer.PROJECTILE)
        {
            if (c.Intersects(d))
            {
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
            if(ch.IsDead)
                _toRemoveColliders.Add(ch.Collider);
                p.HasHit = true;
                _toRemoveColliders.Add(p.Collider);
            }
        }
    }

    private void HandleCharacters(ICollider c, ICollider d)
    {
        if (c.Layer == CollisionLayer.CHARACTER || c.Layer == CollisionLayer.PLAYER)
        {
            HandleCharacterCollision(c, d);
            HandleCharacterProjectiles(c, d);
            // AOE damage handling
            if (d.Layer == CollisionLayer.AOE)
            {
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
                        _toRemoveColliders.Add(ch.Collider);
                }

                return; // don't run projectile logic
            }

        }
    }

    private void HandleTerrain(ICollider c, List<ICollider> statics)
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



    public void Update(List<Player> players, List<ItemBase> items, List<ProjectileBase> projectiles, List<AreaOfEffectBase> aoeAttacks, List<CharacterBase> characters)
    {
        DynamicHash.Clear();
        Insertions(items, players, projectiles, aoeAttacks, characters);
        foreach (KeyValuePair<int, CellData> cell in DynamicHash._cells)
        {
            foreach(var c in cell.Value.Colliders)
            {
                List<ICollider> dynamics;
                if (c.Owner is AreaOfEffectBase aoe)
                {
                    dynamics = new List<ICollider>();
                    foreach (var hitbox in aoe.GetHitboxes())
                        dynamics.AddRange(DynamicHash.QueryNearby(hitbox.Position, 1));
                }
                else
                {
                    dynamics = DynamicHash.QueryNearby(c.Position, 1);
                }
                List<ICollider> statics = StaticHash.QueryNearby(c.Position, 1);
                HandleDynamics(c, dynamics);
                HandleStatics(c, statics);
                HandleTerrain(c, statics);
            }
        }
        foreach(ICollider c in _toRemoveColliders)
        {
            switch (c.Owner) {
                case ProjectileBase p:
                    projectiles.Remove(p);
                    break;
                case ItemBase i:
                    items.Remove(i);
                    break;
                case CharacterBase ch:
                    characters.Remove(ch);
                    break;
                default:
                    break;
            }
        }
        foreach (var aoe in aoeAttacks)
        {
            if (aoe.IsExpired)
                aoeAttacks.Remove(aoe);
        }
    }

    // makes the hitboxes visible for when in the tech demo
    // makes the hitboxes visible for when in the tech demo
public void DrawHitboxes(SpriteBatch spriteBatch, Texture2D pixel,
                         Player player, List<CharacterBase> characters,
                         List<ItemBase> items, List<ProjectileBase> projectiles, List<AreaOfEffectBase> aoeAttacks)
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

