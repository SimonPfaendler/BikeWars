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

namespace BikeWars.Content.managers;
public class GameObjectManager
{
    public event Action<CharacterBase> OnCharacterDied;
    public event Action<CharacterBase, int> OnTookDamage;
    private Player _player1 {get; set;}
    public Player Player1{get => _player1; set => _player1 = value;}
    private Player _player2 {get; set;}
    public Player Player2{get => _player2; set => _player2 = value;}

    private HashSet<CharacterBase> _characters {get; set;}
    public HashSet<CharacterBase> Characters {get => _characters;}

    private HashSet<ItemBase> _items {get; set;}
    public HashSet<ItemBase> Items {get => _items; set => _items = value;}

    private HashSet<BoxCollider> _statics {get; set;}
    public HashSet<BoxCollider> Statics {get => _statics;}

    private HashSet<ProjectileBase> _projectiles {get; set;}
    public HashSet<ProjectileBase> Projectiles {get => _projectiles;}

    private HashSet<AreaOfEffectBase> _aoeAttacks = new();
    public HashSet<AreaOfEffectBase> AOEAttacks => _aoeAttacks;


    public ContentManager _contentManager {get; set;} // TODO do we need this one?

    private WorldAudioManager _worldAudioManager;

    public GameObjectManager(ContentManager content)
    {
        _contentManager = content;

        _characters = new HashSet<CharacterBase>();
        _items = new HashSet<ItemBase>();
        _statics = new HashSet<BoxCollider>();
        _projectiles = new HashSet<ProjectileBase>();
    }
    public GameObjectManager(ContentManager content, Player player1, Player player2)
    {
        Player1 = player1;
        Player2 = player2;
        _contentManager = content;

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
        }

        if (Player2 != null)
        {
            Player2.ShotBullet += () => OnPlayerShotBullet(Player2);
            Player2.Flamethrower += () => OnPlayerFlamethrower(Player2);
            Player2.IceTrail += () => OnPlayerIceTrail(Player2);
        }

    }
    public GameObjectManager(ContentManager content, HashSet<CharacterBase> characters, HashSet<ItemBase> items, HashSet<BoxCollider> statics, HashSet<ProjectileBase> projectiles) // TODO
    {
        _contentManager = content;
        _characters = characters;
        _items = items;
        _statics = statics;
        _projectiles = projectiles;
    }
    public void AddCharacter(CharacterBase character)
    {
        if (_worldAudioManager != null && character is IWorldAudioAware wa)
            wa.SetWorldAudioManager(_worldAudioManager);

        Characters.Add(character);
        character.Attributes.OnDied += HandleCharacterDeath;
        character.OnTookDamage += HandleTookDamage;
    }

    private void HandleCharacterDeath(CharacterBase c)
    {
        OnCharacterDied?.Invoke(c);
    }
    private void HandleTookDamage(CharacterBase c, int amount)
    {
        OnTookDamage?.Invoke(c, amount);
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
        foreach (BoxCollider s in Statics)
        {
            // s.LoadContent();
        }
    }

    public void Update(GameTime gameTime, Vector2 mouseWorldPos)
    {
        if (Player1 != null) Player1.Update(gameTime, mouseWorldPos);
        if (Player2 != null) Player2.Update(gameTime, mouseWorldPos);
        foreach (ProjectileBase p in Projectiles)
        {
            p.Update(gameTime);
        }
        foreach (CharacterBase c in Characters)
        {
            if (c.Movement != null)
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
        foreach(var aoe in _aoeAttacks)
        {
            aoe.Update(gameTime);
            if (aoe.IsExpired)
                _aoeAttacks.Remove(aoe);
        }
    }

    private void OnPlayerShotBullet(Player player)
    {
        Vector2 spawnPos = player.Transform.Position;
        Vector2 direction = player.GazeDirection;

        Bullet b = new Bullet(spawnPos, new Point(8, 8), player);
        b.Movement.Direction = direction; // Set the movement direction
        AddProjectile(b);
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

}