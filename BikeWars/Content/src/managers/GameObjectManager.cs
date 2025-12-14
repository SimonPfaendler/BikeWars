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

    private List<CharacterBase> _characters {get; set;}
    public List<CharacterBase> Characters {get => _characters;}

    private List<ItemBase> _items {get; set;}
    public List<ItemBase> Items {get => _items; set => _items = value;}

    private List<BoxCollider> _statics {get; set;}
    public List<BoxCollider> Statics {get => _statics;}

    private List<ProjectileBase> _projectiles {get; set;}
    public List<ProjectileBase> Projectiles {get => _projectiles;}

    private List<AreaOfEffectBase> _aoeAttacks = new();
    public List<AreaOfEffectBase> AOEAttacks => _aoeAttacks;


    public ContentManager _contentManager {get; set;} // TODO do we need this one?

    private WorldAudioManager _worldAudioManager;

    public GameObjectManager(ContentManager content)
    {
        _contentManager = content;

        _characters = new List<CharacterBase>();
        _items = new List<ItemBase>();
        _statics = new List<BoxCollider>();
        _projectiles = new List<ProjectileBase>();
    }
    public GameObjectManager(ContentManager content, Player player1, Player player2)
    {
        Player1 = player1;
        Player2 = player2;
        _contentManager = content;

        _contentManager = content;
        _characters = new List<CharacterBase>();
        _items = new List<ItemBase>();
        _statics = new List<BoxCollider>();
        _projectiles = new List<ProjectileBase>();

        Player1.ShotBullet += OnPlayerShotBullet;
        Player1.Flamethrower += OnPlayerFlamethrower;
        Player1.IceTrail += OnPlayerIceTrail;

    }
    public GameObjectManager(ContentManager content, List<CharacterBase> characters, List<ItemBase> items, List<BoxCollider> statics, List<ProjectileBase> projectiles) // TODO
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
        Player1.Draw(spriteBatch); // Maybe put it in characters too?
        // Player2.Draw(spriteBatch);
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
        Player1.Update(gameTime, mouseWorldPos);
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
        for (int i = _aoeAttacks.Count - 1; i >= 0; i--)
        {
            var aoe = _aoeAttacks[i];
            aoe.Update(gameTime);

            if (aoe.IsExpired)
                _aoeAttacks.Remove(aoe);
        }
        foreach (BoxCollider s in Statics)
        {
            // s.LoadContent();
        }
    }

    private void OnPlayerShotBullet()
    {
        Vector2 spawnPos = Player1.Transform.Position;
        Vector2 direction = Player1.GazeDirection;

        Bullet b = new Bullet(spawnPos, new Point(8, 8), Player1);
        b.Movement.Direction = direction; // Set the movement direction
        AddProjectile(b);
    }

        private void OnPlayerFlamethrower()
    {
        Vector2 direction = Player1.GazeDirection;
        Flamethrower f = new Flamethrower(Player1, direction);
        f.LoadContent(_contentManager);
        AddAOE(f);
    }

        private void OnPlayerIceTrail()
    {
        Vector2 direction = Player1.GazeDirection;
        IceTrail ice = new IceTrail(Player1, direction);
        ice.LoadContent(_contentManager);
        AddAOE(ice);
    }

    public void SetWorldAudioManager(WorldAudioManager worldAudioManager)
    {
        _worldAudioManager = worldAudioManager;

        if (Player1 is IWorldAudioAware pa)
            pa.SetWorldAudioManager(worldAudioManager);

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