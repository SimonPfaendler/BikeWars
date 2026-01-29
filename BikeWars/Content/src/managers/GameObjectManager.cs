#nullable enable

using System;
using System.Collections.Generic;
using BikeWars.Content.engine;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.entities.items;
using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Entities.Characters.MapObjects;
using BikeWars.Content.components;
using BikeWars.Content.engine.ui;
using BikeWars.Content.entities.MapObjects;
using BikeWars.Content.entities.projectiles;
using BikeWars.Entities;
using BikeWars.Utilities;
using System.Linq;

namespace BikeWars.Content.managers;
public class GameObjectManager
{
    public event Action<CharacterBase>? OnCharacterDied;
    public event Action<Tower>? OnTowerDied;
    public event Action<CharacterBase, int>? OnTookDamage;
    public event Action<Tower, int>? OnTowerTookDamage;
    private Player? _player1 {get; set;}
    public Player? Player1{get => _player1; set => _player1 = value;}
    private Player? _player2 {get; set;}
    public Player? Player2{get => _player2; set => _player2 = value;}

    private List<CharacterBase> _characters {get; set;}
    public List<CharacterBase> Characters {get => _characters;}

    private List<Tower> _towers {get; set;}
    public List<Tower> Towers {get => _towers;}


    private readonly List<ItemBase> _items = new();
    public List<ItemBase> Items => _items;
    private readonly List<ObjectBase> _objects = new();
    public List<ObjectBase> Objects => _objects;

    private List<BoxCollider> _statics {get; set;}
    public List<BoxCollider> Statics {get => _statics;}

    private List<ProjectileBase> _projectiles {get; set;}
    public List<ProjectileBase> Projectiles {get => _projectiles;}

    private List<AreaOfEffectBase> _aoeAttacks = new();

    public List<AreaOfEffectBase> AOEAttacks => _aoeAttacks;
    private HashSet<DamageNumber> _damageNumbers = new HashSet<DamageNumber>();
    private SpriteFont? _damageFont;

    private HashSet<Tram> _trams = new HashSet<Tram>();
    public HashSet<Tram> Trams => _trams;

    public ContentManager _contentManager {get; set;} // TODO do we need this one?

    private float _reviveCooldownTimer = 0f;
    private const float REVIVE_COOLDOWN = 60f;

    private WorldAudioManager? _worldAudioManager;
    public GameObjectManager(ContentManager content, Player? player1, Player? player2)
    {
        Player1 = player1;
        Player2 = player2;
        _contentManager = content;

        _characters = new List<CharacterBase>();
        _items = new List<ItemBase>();
        _statics = new List<BoxCollider>();
        _projectiles = new List<ProjectileBase>();
        _towers = new List<Tower>();

        if (Player1 != null)
        {
            Player1.ShotBullet += () => OnPlayerShotBullet(Player1);
            Player1.Flamethrower += () => OnPlayerFlamethrower(Player1);
            Player1.IceTrail += () => OnPlayerIceTrail(Player1);
            Player1.FireTrail += () => OnPlayerFireTrail(Player1);
            Player1.DamageCircle += () => OnPlayerDamageCircle(Player1);
            Player1.ThrowBook += target => OnPlayerThrowBook(Player1, target);
            Player1.ThrowBanana += target => OnPlayerThrowBanana(Player1, target);
            Player1.ThrowBottle += target => OnPlayerThrowBottle(Player1, target);
            Player1.ThrowBeer += target => OnPlayerThrowBeer(Player1, target);
            Player1.OnTookDamage += HandleTookDamage;
            Player1.Attributes.OnDied += HandlePlayerDied;
        }

        if (Player2 != null)
        {
            Player2.ShotBullet += () => OnPlayerShotBullet(Player2);
            Player2.Flamethrower += () => OnPlayerFlamethrower(Player2);
            Player2.IceTrail += () => OnPlayerIceTrail(Player2);
            Player2.FireTrail += () => OnPlayerFireTrail(Player2);
            Player2.DamageCircle += () => OnPlayerDamageCircle(Player2);
            Player2.ThrowBook += target => OnPlayerThrowBook(Player2, target);
            Player2.ThrowBanana += target => OnPlayerThrowBanana(Player2, target);
            Player2.ThrowBottle += target => OnPlayerThrowBottle(Player2, target);
            Player2.ThrowBeer += target => OnPlayerThrowBeer(Player2, target);
            Player2.OnTookDamage += HandleTookDamage;
            Player1.CanRevive = true;
            Player2.Attributes.OnDied += HandlePlayerDied;
        }
    }

    private void HandleRevival()
    {
        if (Player1 != null && Player2 != null)
        {
            float reviveDistance = 100f;
            if (Player1.IsDying && Player2.IsActionPressed(GameAction.REVIVE))
            {
                if (_reviveCooldownTimer <= 0f && Vector2.Distance(Player2.Transform.Position, Player1.Transform.Position) < reviveDistance)
                {
                    Player1.Revive();
                    _reviveCooldownTimer = REVIVE_COOLDOWN;
                }
            }
        }
    }
    public void AddTower(Tower tower)
    {
        Towers.Add(tower);
        tower.Attributes.OnDied += HandleTowerDeath;
        tower.OnTookDamage += HandleTowerTookDamage;
        if (tower is TowerAlly ta)
        {
            ta.OnShoot += OnTowerShotBullet;
            Statics.Add(ta.CollisionCollider);
            return;
        }
        Statics.Add(tower.Collider);
    }
    public void DestroyTower(Tower tower)
    {
        Towers.Remove(tower);
        NotifyPathGridChanged();
        if (tower is TowerAlly ta)
        {
            Statics.Remove(ta.CollisionCollider);
            return;
        }
        Statics.Remove(tower.Collider);
    }

    public void AddCharacter(CharacterBase character)
    {
        if (_worldAudioManager != null && character is IWorldAudioAware wa)
            wa.SetWorldAudioManager(_worldAudioManager);

        Characters.Add(character);
        character.Attributes.OnDied += HandleCharacterDeath;
        character.OnTookDamage += HandleTookDamage;
    }

    private Dictionary<CharacterBase, int> _pendingDamage = new Dictionary<CharacterBase, int>();
    private float _damageAggregationTimer = 0f;
    private const float AggregationInterval = 0.3f; // every 0.3s

    private void HandleCharacterDeath(CharacterBase c)
    {
        OnCharacterDied?.Invoke(c);
        if (_pendingDamage.ContainsKey(c)) _pendingDamage.Remove(c);
    }

    private void HandleTowerDeath(Tower t)
    {
        OnTowerDied?.Invoke(t);
    }
    private void HandleTookDamage(CharacterBase c, int amount)
    {
        // Aggregate damage
        if (!_pendingDamage.ContainsKey(c))
        {
            _pendingDamage[c] = 0;
        }
        _pendingDamage[c] += amount;

        OnTookDamage?.Invoke(c, amount);

        if (c is Player || c == Player1 || c == Player2)
        {
             OnScreenShakeRequested?.Invoke(5.5f, 0.2f);
        }
    }
    private void HandlePlayerDied(CharacterBase ch)
    {
        OnCharacterDied?.Invoke(ch);
    }
    private void HandleTowerTookDamage(Tower t, int amount)
    {
        OnTowerTookDamage?.Invoke(t, amount);
    }
    public void AddItem(ItemBase item)
    {
        Items.Add(item);
    }

    public void AddObject(ObjectBase obj)
    {
        _objects.Add(obj);
        if (obj.CollisionCollider != null)
            Statics.Add(obj.CollisionCollider);
    }

    public void RemoveObject(ObjectBase obj) => _objects.Remove(obj);

    public void AddStatic(BoxCollider stat)
    {
        Statics.Add(stat);
    }

    public void AddProjectile(ProjectileBase proj)
    {
        Projectiles.Add(proj);
    }

    public void AddAOE(AreaOfEffectBase aoe)
    {
        _aoeAttacks.Add(aoe);
    }

    public void AddTram(Tram tram)
    {
        _trams.Add(tram);
        tram.RequestScreenShake += RequestScreenShake;
    }

    public void LoadContent(ContentManager content)
    {
        foreach (AreaOfEffectBase a in _aoeAttacks)
        {
            a.LoadContent(content);
        }
        _damageFont = content.Load<SpriteFont>("assets/fonts/Arial"); // Using existing Arial font for now
    }

    public void Draw(SpriteBatch spriteBatch)
    {

        if (Player1 != null && !Player1.IsDead) Player1.Draw(spriteBatch);
        if (Player2 != null && !Player2.IsDead) Player2.Draw(spriteBatch);
        foreach (CharacterBase c in Characters)
        {
            c.Draw(spriteBatch);
        }
        foreach (Tower t in Towers)
        {
            t.Draw(spriteBatch);
        }
        foreach (ItemBase i in Items)
        {
            i.Draw(spriteBatch);
        }
        foreach (var o in Objects)
        {
            o.Draw(spriteBatch);
        }
        foreach (ProjectileBase p in Projectiles)
        {
            p.Draw(spriteBatch);
        }
        foreach (AreaOfEffectBase aoe in _aoeAttacks)
        {
            aoe.Draw(spriteBatch);
        }

        foreach (var dn in _damageNumbers)
        {
            if (_damageFont != null)
                dn.Draw(spriteBatch, _damageFont);
        }
        foreach (var tram in _trams)
        {
            tram.Draw(spriteBatch);
        }

        foreach (BoxCollider s in Statics)
        {
            // s.LoadContent();
        }
    }

    public void Update(GameTime gameTime, Vector2 mouseWorldPos)
    {
        if (Player1 != null && !Player1.IsDead) Player1.Update(gameTime, mouseWorldPos);
        if (Player2 != null && !Player2.IsDead) Player2.Update(gameTime, mouseWorldPos);
        foreach (CharacterBase c in Characters)
        {
            if (c.Movement != null && Player1 != null)
            {
                c.Movement.PlayerPosition = Player1.Transform.Position;
                c.Movement.EnemyPosition = c.Transform.Position;
            }
            c.Update(gameTime);
            c.UpdateCollider();
        }
        foreach (Tower t in Towers)
        {
            t.Update(gameTime, Characters.ToList());
        }
        foreach (ItemBase i in Items)
        {
            i.Update(gameTime);
        }
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            var item = _items[i];
            item.Update(gameTime);

            if (item is Beer b)
            {
                if (b.IsExpired)
                {
                    _items.RemoveAt(i);
                }
            }
        }

        foreach (ObjectBase o in Objects)
        {
            o.Update(gameTime);
        }

        foreach (ProjectileBase p in Projectiles)
        {
            p.Update(gameTime);
        }

        for (int i = Projectiles.Count - 1; i >= 0; i--)
        {
            var lp = Projectiles[i];
            lp.Update(gameTime);

            if (lp is ThrowObject to)
            {
                if (to.IsFinished)
                {
                    Projectiles.RemoveAt(i);
                }
            }
        }
        for (int i = _aoeAttacks.Count - 1; i >= 0; i--)
        {
            var aoe = _aoeAttacks[i];
            aoe.Update(gameTime);
            if (aoe.IsExpired)
            {
                _aoeAttacks.RemoveAt(i);
            }
        }
        foreach (var tram in _trams)
        {
            tram.Update(gameTime);
        }
        _trams.RemoveWhere(t => t.IsExpired);
        _damageNumbers.RemoveWhere(dn =>
        {
            dn.Update(gameTime);
            return dn.IsExpired;
        });

        // Flush Aggregated Damage
        _damageAggregationTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_damageAggregationTimer <= 0f)
        {
            foreach (var kvp in _pendingDamage)
            {
                var character = kvp.Key;
                var totalDamage = kvp.Value;

                // Only spawn if damage > 0 and character is valid
                if (totalDamage > 0)
                {
                    bool isCrit = false;
                    SpawnDamageNumber(character.Transform.Position, totalDamage, isCrit);
                }
            }
            _pendingDamage.Clear();
            _damageAggregationTimer = AggregationInterval;
        }

        if (_reviveCooldownTimer > 0f)
        {
            _reviveCooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        DogBowl.UpdateBowl(gameTime);
        HandleRevival();
    }

    private void OnPlayerShotBullet(Player player)
    {
        Vector2 spawnPos = player.Transform.Position;
        Vector2 direction = player.GazeDirection;

        Bullet b = new Bullet(spawnPos, new Point(10, 10), player, player.AttributesOfCurrentWeapon());
        b.Movement.Direction = direction; // Set the movement direction
        AddProjectile(b);

        OnScreenShakeRequested?.Invoke(1.5f, 0.05f);
    }

    private void OnTowerShotBullet(Tower tower)
    {
        Vector2 spawnPos = tower.Transform.Bounds.Center.ToVector2();
        WeaponAttributes wa = new WeaponAttributes();
        wa.Damage = 10;
        wa.Speed = 100f;
        Bullet b = new Bullet(spawnPos, new Point(10, 10), tower, wa);

        // Bullet direction is the tower's gaze direction.
        b.Movement.Direction = tower.GazeDirection;
        b.Movement.IsMoving = true;
        b.Movement.Speed = 250f;

        AddProjectile(b);

        OnScreenShakeRequested?.Invoke(1.5f, 0.05f);
    }

    private Vector2 RotateVector(Vector2 vec, float angle)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);
        return new Vector2(
            vec.X * cos - vec.Y * sin,
            vec.X * sin + vec.Y * cos
        );
}



    private void OnPlayerFlamethrower(Player player)
    {
        Vector2 direction = player.GazeDirection;
        Flamethrower f = new Flamethrower(player, direction);
        f.LoadContent(_contentManager);
        AddAOE(f);
    }



    private void OnPlayerIceTrail(Player player)
    {
        Vector2 direction = player.GazeDirection;
        IceTrail ice = new IceTrail(player, direction);
        ice.LoadContent(_contentManager);
        AddAOE(ice);
    }

    private void OnPlayerFireTrail(Player player)
    {
        Vector2 direction = player.GazeDirection;
        FireTrail fire = new FireTrail(player, direction);
        fire.LoadContent(_contentManager);
        AddAOE(fire);
    }

    public event Action<float, float>? OnScreenShakeRequested;

    private void OnPlayerDamageCircle(Player player)
    {
        Vector2 direction = player.GazeDirection;
        DamageCircle dc = new DamageCircle(
            player.Transform,
            owner: player,
            damagePlayers: false
        );
        dc.LoadContent(_contentManager);
        AddAOE(dc);

        // Shake screen on cast
        //OnScreenShakeRequested?.Invoke(6f, 0.8f);
        OnScreenShakeRequested?.Invoke(7f, 2.0f);

    }

    private void OnPlayerThrowBook(Player player, Vector2 target)
    {
        Vector2 spawnPos = player.Transform.Bounds.Center.ToVector2();
        Vector2 toTarget = target - spawnPos;
        float distance = toTarget.Length();
        if (distance > Player.ThrowRange)
        {
            target = distance > 0.001f
                ? spawnPos + Vector2.Normalize(toTarget) * Player.ThrowRange
                : spawnPos;
        }
        var book = new ThrowBook(spawnPos, target, player);
        AddProjectile(book);
    }

    private void OnPlayerThrowBanana(Player player, Vector2 target)
    {
        Vector2 spawnPos = player.Transform.Bounds.Center.ToVector2();
        Vector2 toTarget = target - spawnPos;
        float distance = toTarget.Length();
        if (distance > Player.ThrowRange)
        {
            target = distance > 0.001f
                ? spawnPos + Vector2.Normalize(toTarget) * Player.ThrowRange
                : spawnPos;
        }
        var banana = new ThrowBanana(spawnPos, target, player);
        AddProjectile(banana);
    }

    private void OnPlayerThrowBottle(Player player, Vector2 target)
    {
        Vector2 spawnPos = player.Transform.Bounds.Center.ToVector2();
        Vector2 toTarget = target - spawnPos;
        float distance = toTarget.Length();
        if (distance > Player.ThrowRange)
        {
            target = distance > 0.001f
                ? spawnPos + Vector2.Normalize(toTarget) * Player.ThrowRange
                : spawnPos;
        }
        var bottle = new ThrowBottle(spawnPos, target, player);
        AddProjectile(bottle);
    }

    public void OnEnemyThrowBottle(CharacterBase enemy, Vector2 target)
    {
        Vector2 spawnPos = enemy.Transform.Bounds.Center.ToVector2();
        Vector2 toTarget = target - spawnPos;
        float distance = toTarget.Length();
        if (distance > Player.ThrowRange)
        {
            target = distance > 0.001f
                ? spawnPos + Vector2.Normalize(toTarget) * Player.ThrowRange
                : spawnPos;
        }
        var beer = new ThrowBeer(spawnPos, target, enemy, emitLandingEvent: false);
        AddProjectile(beer);
    }


    private void OnPlayerThrowBeer(Player player, Vector2 target)
    {
        Vector2 spawnPos = player.Transform.Bounds.Center.ToVector2();
        Vector2 toTarget = target - spawnPos;
        float distance = toTarget.Length();
        if (distance > Player.ThrowRange)
        {
            target = distance > 0.001f
                ? spawnPos + Vector2.Normalize(toTarget) * Player.ThrowRange
                : spawnPos;
        }
        var beer = new ThrowBeer(spawnPos, target, player);
        beer.OnBeerLanded += pos =>
        {
            SpawnLandedBeer(pos);
        };

        AddProjectile(beer);
    }

    public Beer SpawnLandedBeer(Vector2 pos)
    {
        var beer = new Beer(pos, new Point(32, 32));
        beer.LandedBeer(pos);
        AddItem(beer);
        return beer;
    }


    public void RequestScreenShake(float intensity, float duration)
    {
        OnScreenShakeRequested?.Invoke(intensity, duration);
    }

    public void SetWorldAudioManager(WorldAudioManager worldAudioManager)
    {
        _worldAudioManager = worldAudioManager;

        if (Player1 is IWorldAudioAware pa)
            pa.SetWorldAudioManager(worldAudioManager);
        if (Player2 is IWorldAudioAware pa2)
            pa2.SetWorldAudioManager(worldAudioManager);

        foreach (var c in Characters)
        {
            if (c is IWorldAudioAware wa)
                wa.SetWorldAudioManager(worldAudioManager);
        }

        foreach (Tower t in Towers)
        {
            if (t is IWorldAudioAware wa)
                wa.SetWorldAudioManager(worldAudioManager);
        }
    }

    // Notify characters that the path grid changed (force enemies to recalculate paths)
    public void NotifyPathGridChanged()
    {
        foreach (var c in Characters)
        {
            try
            {
                var em = c.Movement;
                if (em != null)
                {
                    em.ForceRepath();
                }
            }
            catch
            {
                // ignore
            }
        }
    }

    public void SpawnXp(CharacterBase character)
    {
        ItemBase xp;
        // spawns Xp_Item at the location of character
        // function is used in CharacterBase.cs
        Vector2 pos = character.Transform.Position;
        xp = new Xp_Money(pos, new Point(16, 16));
        AddItem(xp);

        if (RandomUtil.NextDouble() <= 0.05) // 5% chance to drop an energy gel
        {
            EnergyGel energyGel = new EnergyGel(pos, new Point(32, 32));
            AddItem(energyGel);
        }
    }

    public void SpawnDamageNumber(Vector2 position, int amount, bool isCrit = false)
    {
        // Calculate direction away from Player1
        Vector2 direction = Vector2.Zero;
        if (Player1 != null)
        {
            direction = position - Player1.Transform.Position;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }
            else
            {
                direction = new Vector2(0, -1); // Default up if on top of player
            }
        }
        else
        {
             direction = new Vector2(0, -1);
        }

        // Create a velocity: Move OUT and UP
        // Randomize slightly for "juice"
        float angle = (float)(RandomUtil.NextDouble() * 0.5f - 0.25f); // +/- ~15 degrees variation

        // Rotate direction slightly
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);
        Vector2 rotatedDir = new Vector2(direction.X * cos - direction.Y * sin, direction.X * sin + direction.Y * cos);

        float speed = 200f;
        float upSpeed = 100f;

        Vector2 velocity = rotatedDir * speed + new Vector2(0, -upSpeed);

        if (isCrit) velocity *= 1.5f; // Bigger pop for crits

        _damageNumbers.Add(new DamageNumber(position, amount, isCrit, velocity));
    }
    public void Remove(ItemBase item)
    {
        _items.Remove(item);
    }

    public void SpawnFromTiledObjects(IEnumerable<TiledObjectInfo> spawns)
    {
        foreach (var spawn in spawns)
        {
            var tower = CreateTowerFromTiled(spawn);
            if (tower != null)
            {
                AddTower(tower);
                continue;
            }
            var created = CreateFromTiled(spawn);
            if (created == null)
            {
                continue;
            }
            AddObject(created);
            switch (created) {
                case BikeShop bs: // It works but Bikeshop shouldn't be a item.
                    AddStatic(bs.CollisionCollider);
                    break;
                case DestructibleObject:
                    AddStatic(created.Collider);
                    break;
            }
        }
    }
    private Tower? CreateTowerFromTiled(TiledObjectInfo spawn)
    {
        var start = new Vector2(spawn.Rect.X, spawn.Rect.Y);
        var size  = new Point(spawn.Rect.Width, spawn.Rect.Height);

        if (!spawn.Properties.ContainsKey("type"))
        {
            return null;
        }
        string type = spawn.Properties["type"];
        return type switch
        {
            "tower" => new TowerAlly(start, size),
            _ => null
        };
    }
    // spawn = properties
    private ObjectBase? CreateFromTiled(TiledObjectInfo spawn)
    {
        var start = new Vector2(spawn.Rect.X, spawn.Rect.Y);
        var size  = new Point(spawn.Rect.Width, spawn.Rect.Height);

        if (!spawn.Properties.ContainsKey("type"))
        {
            return null;
        }
        string type = spawn.Properties["type"];
        switch (type)
        {
            case "Bike_Shop":
                return new BikeShop(start, size);
            case "Destructible":
                return new DestructibleObject(start, size, spawn);
            case "AchievementTrigger":
                return new AchievementTrigger(spawn.Name, start, size, spawn); // Name or something like that
            case "chest":
            {
                spawn.Properties.TryGetValue("item", out string? itemString);

                Chest.ChestItemType? item = itemString switch
                {
                    "Energygel" => Chest.ChestItemType.Energygel,
                    "Frelo" => Chest.ChestItemType.Frelo,
                    "Racingbike" => Chest.ChestItemType.Racingbike,
                    "DogFood" => Chest.ChestItemType.DogFood,
                    "DopingSpritze" => Chest.ChestItemType.DopingSpritze,
                    "Beer" => Chest.ChestItemType.Beer,
                    "Flame" => Chest.ChestItemType.Flame,
                    "Ice" => Chest.ChestItemType.Ice,
                    _ => null
                };

                return new Chest(start, size, item);
            }
            case "dog-bowl":
                return new DogBowl(start, size);
            case "musicians":
                return new Musicians(start, size);
            default:
                return null;
        }
    }
    public void Unload()
    {
        OnTookDamage = null;
        OnCharacterDied = null;
        OnScreenShakeRequested = null;

        Characters.Clear();
        Items.Clear();
        Projectiles.Clear();
        AOEAttacks.Clear();
        Trams.Clear();

        Statics.Clear();
    }
}
