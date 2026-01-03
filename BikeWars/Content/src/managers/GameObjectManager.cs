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
using BikeWars.Content.engine.ui;


namespace BikeWars.Content.managers;
public class GameObjectManager
{
    public event Action<CharacterBase>? OnCharacterDied;
    public event Action<CharacterBase, int>? OnTookDamage;
    private Player? _player1 {get; set;}
    public Player? Player1{get => _player1; set => _player1 = value;}
    private Player? _player2 {get; set;}
    public Player? Player2{get => _player2; set => _player2 = value;}

    private HashSet<CharacterBase> _characters {get; set;}
    public HashSet<CharacterBase> Characters {get => _characters;}

    private readonly HashSet<ItemBase> _items = new();
    public HashSet<ItemBase> Items => _items;

    private HashSet<BoxCollider> _statics {get; set;}
    public HashSet<BoxCollider> Statics {get => _statics;}

    private HashSet<ProjectileBase> _projectiles {get; set;}
    public HashSet<ProjectileBase> Projectiles {get => _projectiles;}

    private HashSet<AreaOfEffectBase> _aoeAttacks = new();

    
    public HashSet<AreaOfEffectBase> AOEAttacks => _aoeAttacks;
    
    private HashSet<DamageNumber> _damageNumbers = new HashSet<DamageNumber>();
    private SpriteFont? _damageFont;


    public ContentManager _contentManager {get; set;} // TODO do we need this one?

    private WorldAudioManager? _worldAudioManager;

    public GameObjectManager(ContentManager content, Player? player1, Player? player2)
    {
        Player1 = player1;
        Player2 = player2;
        _contentManager = content;
        
        _characters = new HashSet<CharacterBase>();
        _items = new HashSet<ItemBase>();
        _statics = new HashSet<BoxCollider>();
        _projectiles = new HashSet<ProjectileBase>();

        if (Player1 != null)
        {
            Player1.ShotBullet += () => OnPlayerShotBullet(Player1);
            Player1.Flamethrower += () => OnPlayerFlamethrower(Player1);
            Player1.IceTrail += () => OnPlayerIceTrail(Player1);
            Player1.DamageCircle += () => OnPlayerDamageCircle(Player1);
            Player1.OnTookDamage += HandleTookDamage;
        }

        if (Player2 != null)
        {
            Player2.ShotBullet += () => OnPlayerShotBullet(Player2);
            Player2.Flamethrower += () => OnPlayerFlamethrower(Player2);
            Player2.IceTrail += () => OnPlayerIceTrail(Player2);
            Player2.DamageCircle += () => OnPlayerDamageCircle(Player2);
            Player2.OnTookDamage += HandleTookDamage;
        }

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

    public void AddItem(ItemBase item)
    {
        Items.Add(item);
    }

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
        if (Player1 != null) Player1.Draw(spriteBatch);
        if (Player2 != null) Player2.Draw(spriteBatch);
        foreach (CharacterBase c in Characters)
        {
            c.Draw(spriteBatch);
        }
        foreach (ItemBase i in Items)
        {
            i.Draw(spriteBatch);
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

        foreach (BoxCollider s in Statics)
        {
            // s.LoadContent();
        }
    }

    public void Update(GameTime gameTime, Vector2 mouseWorldPos)
    {
        if (Player1 != null) Player1.Update(gameTime, mouseWorldPos);
        if (Player2 != null) Player2.Update(gameTime, mouseWorldPos);
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
        foreach (ItemBase i in Items)
        {
            i.Update(gameTime);
        }
        foreach (ProjectileBase p in Projectiles)
        {
            p.Update(gameTime);
        }
        _aoeAttacks.RemoveWhere(aoe =>
        {
            aoe.Update(gameTime);
            return aoe.IsExpired;
        });

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
    }

    private void OnPlayerShotBullet(Player player)
    {
        Vector2 spawnPos = player.Transform.Position;
        Vector2 direction = player.GazeDirection;

        Bullet b = new Bullet(spawnPos, new Point(10, 10), player);
        b.Movement.Direction = direction; // Set the movement direction
        AddProjectile(b);

        OnScreenShakeRequested?.Invoke(0.75f, 0.05f);
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

    public event Action<float, float>? OnScreenShakeRequested;

    private void OnPlayerDamageCircle(Player player)
    {
        Vector2 direction = player.GazeDirection;
        DamageCircle dc = new DamageCircle(player);
        dc.LoadContent(_contentManager);
        AddAOE(dc);
        
        // Shake screen on cast
        //OnScreenShakeRequested?.Invoke(6f, 0.8f);
        OnScreenShakeRequested?.Invoke(7f, 2.0f);

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
    }

    public void SpawnXp(CharacterBase character)
    {
        ItemBase xp;
        // spawns Xp_Item at the location of character
        // function is used in CharacterBase.cs
        Vector2 pos = character.Transform.Position;

        if (character is Hobo)
            xp = new Xp_Beer(pos, new Point(16, 16));
        // other cases will be added later
        else
            xp = new Xp_Money(pos, new Point(16, 16));
        AddItem(xp);

        Random rnd = new Random();
        if (rnd.NextDouble() <= 0.05) // 5% chance to drop an energy gel
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
        Random rnd = new Random();
        float angle = (float)(rnd.NextDouble() * 0.5f - 0.25f); // +/- ~15 degrees variation
        
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
            var created = CreateFromTiled(spawn);
            if (created != null)
            {
                // Destructible objects should both be drawable (items) and registered as statics
                if (created is BikeWars.Entities.Characters.MapObjects.DestructibleObject)
                {
                    AddItem(created);
                    if (created.Collider is BoxCollider box)
                        AddStatic(box);
                }
                else
                {
                    AddItem(created);
                }
            }
        }
    }
    
    private ItemBase? CreateFromTiled(TiledObjectInfo spawn)
    {
        var start = new Vector2(spawn.Rect.X, spawn.Rect.Y);
        var size  = new Point(spawn.Rect.Width, spawn.Rect.Height);
        
        string type = spawn.Properties["type"];

        switch (type)
        {
            case "Bike_Shop":
                return new BikeShop(start, size, spawn);
            case "Destructible":
                return new BikeWars.Entities.Characters.MapObjects.DestructibleObject(start, size, spawn);

            default:
                return null;
        }
    }

}
