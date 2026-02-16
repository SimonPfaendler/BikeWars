#nullable enable
using System;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.entities.items;
using BikeWars.Content.managers;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using BikeWars.Content.entities.MapObjects;
using BikeWars.Entities.Characters.MapObjects;
using BikeWars.Entities;
using System.Linq;
using BikeWars.Content.entities.npcharacters;
using BikeWars.Content.entities.projectiles;
using BikeWars.Content.components;

namespace BikeWars.Content.src.utils.SaveLoadExample;
public static class SaveLoad
{
    private static int _worldBounds = 11200 / 2;
    public enum TYPES
    {
        BULLET,
        HOBO,
        DOG,
        DOG_FOOD,
        POLICE_MAN,
        DOZENT,
        KAMIKAZE_OPA,
        BIKETHIEF,
        CHEST,
        ENERGY_GEL,
        ENERGY_BAR,
        DOPING,
        BEER,
        MONEY,
        FRELO,
        RACINGBIKE,
        BIKESHOP,
        DOGBOWL,
        DESTRUCTIBLE,
        TRIGGER,
        MUSICIANS,
        THROWBEER,
        THROWBOTTLE,
        THROWBOOK,
        THROWBANANA,
        FLAMETHROWERFIRE,
        ICETRAIL,
        FIRETRAIL,
        BELL
    }
    // save file path in the user's Documents folder
    private static readonly string SAVE_PATH = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "SaveData.json");

    // Stores the data that will be saved to and loaded from a JSON file.
    // The data should have a default set, in case the information isn't in the JSON file yet
    public class GameState
    {
        public float GameTimerCurrentTime { get; set; } = 120f;
        public float GameTimerTotalTime { get; set; } = 120f;
        public bool IsGameTimerRunning {get; set;} = true;
        public bool IsGameTimerPaused { get; set; } = false;
        public List<PlayerSaveModel> Players {get;set;} = new();
        public List<ProjectileSaveModel> Projectiles {get; set;} = new();
        public List<AOESaveModel> AOESaves {get; set;} = new();
        public List<CharacterSaveModel> Characters {get; set;} = new();
        public List<ItemSaveModel> Items {get; set;} = new();
        public List<ObjectSaveModel> Objects { get; set; } = new();
        public List<CarSaveModel> Cars {get;set;} = new();
        public List<TramSaveModel> Trams {get;set;} = new();
        public List<Statistic> Statistics{get; set;} = new();
        public Statistic Statistic{get; set;} = new();
        public List<Achievement> Achievements{get; set;} = new();
        public int GameMode { get; set; } = 0;
    }

    public class BasicSaveModel
    {
        public TYPES Type {get; set;} // Type of the projectile. Like bullet

        public Vector2Save Position {get;set;} = new();

        public PointSave Size {get;set;} = new();

        public BasicSaveModel()
        {

        }
        public BasicSaveModel(TYPES type, Vector2 position, Point size)
        {
            Type = type;
            Position = new Vector2Save(position);
            Size = new PointSave(size);
        }
    }
    public class ProjectileSaveModel
    {
        public BasicSaveModel Basic {get;set;} = new();
        public ProjectileSaveModel() {}

        public WeaponAttributes? WeaponAttributes {get; set;}
        public bool HasHit {get; set;}

        public Vector2Save Direction {get; set;} = new();
        public bool IsMoving {get; set;}
        public bool CanMove {get; set;}
        public float Rotation {get; set;}

        public Vector2Save Target {get; set;}

        public ProjectileSaveModel(BasicSaveModel b, bool hasHit, Vector2 direction, bool isMoving, bool canMove, float rotation, WeaponAttributes weaponAttributes, Vector2 target)
        {
            Basic = b;
            WeaponAttributes = weaponAttributes;
            HasHit = hasHit;
            Direction = new Vector2Save(direction);
            IsMoving = isMoving;
            CanMove = canMove;
            Rotation = rotation;
            Target = new Vector2Save(target);
        }
    }

    public class AOESaveModel
    {
        public BasicSaveModel Basic {get;set;} = new();
        public AOESaveModel() {}

        public WeaponAttributes? WeaponAttributes {get; set;}
        public bool HasHit {get; set;}

        public Vector2Save Direction {get; set;} = new();
        public Vector2Save Position {get; set;} = new();
        public PointSave Size {get; set;} = new();
        // public bool IsMoving {get; set;}
        // public bool CanMove {get; set;}
        // public float Rotation {get; set;}

        // public Vector2Save Target {get; set;}

        // public AOESaveModel(BasicSaveModel b, bool hasHit, Vector2 direction, bool isMoving, bool canMove, float rotation, WeaponAttributes weaponAttributes, Vector2 target)
        public AOESaveModel(BasicSaveModel b, Vector2 direction, Vector2 position, Point size)
        {
            Basic = b;
            // WeaponAttributes = weaponAttributes;
            // HasHit = hasHit;
            Direction = new Vector2Save(direction);
            Position = new Vector2Save(position);
            Size = new PointSave(size);
        }
    }

    public class CharacterSaveModel
    {
        public TYPES Type {get; set;} // Character Type Like Hobo

        public Vector2Save Position { get; set; } = new();

        public PointSave Size {get;set;}  = new();

        public float Radius {get;set;}

        public CharacterSaveModel() {}

        public CharacterSaveModel(TYPES type, Vector2 position, Point size)
        {
            Type = type;
            Position = new Vector2Save(position);
            Size = new PointSave(size);
        }

        public CharacterSaveModel(TYPES type, Vector2 position, float radius)
        {
            Type = type;
            Position = new Vector2Save(position);
            Radius = radius;
        }
    }

    public class PlayerSaveModel
    {
        public uint PlayerNumber {get;set;}
        public Vector2Save Position { get; set; } = new();

        public PointSave RenderSize {get;set;}  = new();

        public float Radius {get;set;}

        public PlayerSaveModel() {}

        public PlayerSaveModel(uint playerNumber, Vector2 position, float radius, Point renderSize)
        {
            PlayerNumber = playerNumber;
            Position = new Vector2Save(position);
            Radius = radius;
            RenderSize = new PointSave(renderSize);
        }
    }

    public class ItemSaveModel
    {
        public TYPES Type {get; set;} // Item Type Like Chest

        public Vector2Save Position {get;set;}  = new();

        public PointSave Size {get;set;}  = new();

        public bool? IsOpen { get; set; }

        public string? Item { get; set; }

        public ItemSaveModel() { }

        public ItemSaveModel(TYPES type, Vector2 position, Point size)
        {
            Type = type;
            Position = new Vector2Save(position);
            Size = new PointSave(size);
        }
    }

    public class DestructibleObjectSaveModel
    {
        public string Name {get;set;}
        public Vector2Save Position {get;set;} = new();
        public PointSave Size {get;set;} = new();
        public int Health {get; set;}
        public string SpriteKey {get; set;}
    }
    public class CarSaveModel
    {
        public TYPES Type { get; set; }
        public Vector2Save Position { get; set; } = new();
        public PointSave Size { get; set; } = new();
        public Random Rng {get;set;}
        public string SideKey { get; set;}
        public string UpKey { get; set;}

        public CarSaveModel() { }
        public CarSaveModel(TYPES type, Vector2 position, Point size, Random rng, string sideKey, string upKey)
        {
            Type = type;
            Position = new Vector2Save(position);
            Size = new PointSave(size);
            Rng = rng;
            SideKey = sideKey;
            UpKey = upKey;
        }
    }
    public class TramSaveModel
    {
        public Vector2Save StartPosition { get; set; } = new();
        public Vector2Save Direction { get; set; } = new();
        public float Rotation { get; set; } = new();
        public TramSaveModel() { }
        public TramSaveModel(Vector2 startPosition, Vector2 direction, float rotation)
        {
            StartPosition = new Vector2Save(startPosition);
            Direction = new Vector2Save(direction);
            Rotation = rotation;
        }
    }
    public class ObjectSaveModel
    {
        public TYPES Type { get; set; }
        public Vector2Save Position { get; set; } = new();
        public PointSave Size { get; set; } = new();

        public bool? IsOpen { get; set; }
        public string? Item { get; set; }
        public bool? IsFull { get; set; }
        public AchievementIds? Id { get; set;} // Trigger

        public int? Health {get; set;} // Destructible
        public string? SpriteKey {get;set;} // Destructible

        public ObjectSaveModel() { }
        public ObjectSaveModel(TYPES type, Vector2 position, Point size)
        {
            Type = type;
            Position = new Vector2Save(position);
            Size = new PointSave(size);
        }
    }

    public class PointSave
    {
        public int X { get; set; }
        public int Y { get; set; }
        public PointSave() {}
        public PointSave(Point p)
        {
            X = p.X;
            Y = p.Y;
        }
        public Point ToPoint() => new Point(X, Y);
    }

    public class Vector2Save
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2Save() {}
        public Vector2Save(Vector2 v)
        {
            X = v.X;
            Y = v.Y;
        }
        public Vector2 ToVector2() => new Vector2(X, Y);
    }

    // save the counter in a JSON file
    public static void SaveGame(GameTimer gameTimer, GameObjectManager gameObjectManager, StatisticsManager statisticsManager, AchievementsManager achievementsManager, GameMode gameMode)
    {
        try
        {
            float playerX = _worldBounds;
            float playerY = _worldBounds;
            if (gameObjectManager.Player1 != null)
            {
                playerX = gameObjectManager.Player1.Transform.Position.X;
                playerY = gameObjectManager.Player1.Transform.Position.Y;
            }
            List<PlayerSaveModel> playerList = new List<PlayerSaveModel>();
            if (gameObjectManager.Player1 != null)
            {
                playerList.Add(MakePlayerSaveModel(1, gameObjectManager.Player1));
            }
            if (gameObjectManager.Player2 != null)
            {
                playerList.Add(MakePlayerSaveModel(2, gameObjectManager.Player2));
            }
            // serialize the current info into JSON text
            GameState state = new GameState
            {
                GameMode = (int)gameMode,
                GameTimerCurrentTime = gameTimer.CurrentTime,
                GameTimerTotalTime = gameTimer.TotalTime,
                IsGameTimerRunning = gameTimer.IsRunning,
                IsGameTimerPaused = gameTimer.IsPaused,

                Players = playerList,
                Projectiles = MakeProjectileSaveList(gameObjectManager.Projectiles),
                // AOESaves = MakeAOESaveList(gameObjectManager.AOEAttacks),
                Characters = MakeCharacterSaveList(gameObjectManager.Characters),
                Items = MakeItemSaveList(gameObjectManager.Items),
                Objects = MakeObjectSaveList(gameObjectManager.Objects),
                Cars = MakeCarsSaveList(gameObjectManager.Cars),
                Trams = MakeTramsSaveList(gameObjectManager.Trams.ToList()),
                Statistics = statisticsManager.Statistics,
                Statistic = statisticsManager.Statistic,
                Achievements = achievementsManager.Achievements.Values.ToList(),
            };
            string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });

            // get the folder where the JSON file will be saved
            // if it doesn't exist yet, it creates one
            string? dir = Path.GetDirectoryName(SAVE_PATH);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            // write the JSON file and print a message in the console when it's saved successfully
            File.WriteAllText(SAVE_PATH, json);
            Console.WriteLine("Saved: " + SAVE_PATH);
        }

        // if the save isn't successful it will print a message in the console
        catch (Exception ex)
        {
            Console.WriteLine("Save failed: " + ex.Message);
        }
    }

    // Use this if you just want to add other data like statistics and you still need the
    // last save in the game to load it again. That's why we need LoadGame here
    public static void SaveNonGame(StatisticsManager statisticsManager, AchievementsManager achievementsManager)
    {
        GameState loadState = LoadGame();
        try
        {
            // serialize the current info into JSON text
            GameState state = new GameState
            {
                GameMode = loadState.GameMode,
                GameTimerCurrentTime = loadState.GameTimerCurrentTime,
                GameTimerTotalTime = loadState.GameTimerTotalTime,
                IsGameTimerRunning = loadState.IsGameTimerRunning,
                IsGameTimerPaused = loadState.IsGameTimerPaused,
                Players = loadState.Players,
                Projectiles = loadState.Projectiles,
                Characters = loadState.Characters,
                Items = loadState.Items,
                Objects = loadState.Objects,
                Statistics = statisticsManager.Statistics,
                Statistic = statisticsManager.Statistic,
                Achievements = achievementsManager.Achievements.Values.ToList(),
            };
            string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            // get the folder where the JSON file will be saved
            // if it doesn't exist yet, it creates one
            string? dir = Path.GetDirectoryName(SAVE_PATH);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            // write the JSON file and print a message in the console when it's saved successfully
            File.WriteAllText(SAVE_PATH, json);
            Console.WriteLine("Saved: " + SAVE_PATH);
        }

        // if the save isn't successful it will print a message in the console
        catch (Exception ex)
        {
            Console.WriteLine("Save failed: " + ex.Message);
        }
    }

    // transform the JSON object to a C# object
    public static GameState LoadGame()
    {
        if (!File.Exists(SAVE_PATH))
        {
            Console.WriteLine("No save file yet — starting with default values.");
            return new GameState();
        }

        // read the contents of the JSON file into a string
        // and convert the JSON string back into a GameState object
        string json = File.ReadAllText(SAVE_PATH);
        GameState? state = JsonSerializer.Deserialize<GameState>(json);

        // if deserialization failed, throw an error
        if (state == null)
        {
            throw new InvalidDataException("Failed to deserialize save data.");
        }

        Console.WriteLine($"Timer loaded: {FormatTime(state.GameTimerCurrentTime)}");
        return state;
    }

    private static ProjectileSaveModel MakeProjectileSaveModel(ProjectileBase projectile)
    {
        return projectile switch
        {
            Bullet b => new ProjectileSaveModel(new BasicSaveModel(TYPES.BULLET, projectile.Transform.Position, projectile.Transform.Size), b.HasHit, b.Movement.Direction, b.Movement.IsMoving, b.Movement.CanMove, b.Movement.Rotation, b.weaponAttributes, Vector2.Zero),
            ThrowBeer tb => new ProjectileSaveModel(new BasicSaveModel(TYPES.THROWBEER, projectile.Transform.Position, projectile.Transform.Size), tb.HasHit, Vector2.Zero, true, true, 0, tb.weaponAttributes, tb._target),
            ThrowBanana banana => new ProjectileSaveModel(new BasicSaveModel(TYPES.THROWBANANA, projectile.Transform.Position, projectile.Transform.Size), banana.HasHit, Vector2.Zero, true, true, 0, banana.weaponAttributes, banana._target),
            ThrowBook book => new ProjectileSaveModel(new BasicSaveModel(TYPES.THROWBOOK, projectile.Transform.Position, projectile.Transform.Size), book.HasHit, Vector2.Zero, true, true, 0, book.weaponAttributes, book._target),
            ThrowBottle tbo => new ProjectileSaveModel(new BasicSaveModel(TYPES.THROWBOTTLE, projectile.Transform.Position, projectile.Transform.Size), tbo.HasHit, Vector2.Zero, true, true, 0, tbo.weaponAttributes, tbo._target),
            _ => throw new NotSupportedException($"Projectile type {projectile.GetType().Name} is not supported for saving.")
        };
    }

    private static TramSaveModel MakeTramSaveModel(Tram tram)
    {
        return new TramSaveModel(tram.Position, tram.Velocity, tram.Rotation);
    }

    // private static AOESaveModel MakeAOESaveModel(AreaOfEffectBase aoe)
    // {
    //     return aoe switch
    //     {
    //         // Bullet b => new ProjectileSaveModel(new BasicSaveModel(TYPES.BULLET, projectile.Transform.Position, projectile.Transform.Size), b.weaponAttributes.Damage, b.HasHit, b.Movement.Direction, b.Movement.IsMoving, b.Movement.CanMove, b.Movement.Rotation),
    //         // Flamethrower f => new AOESaveModel(new BasicSaveModel(TYPES.FLAMETHROWERFIRE, Vector2.Zero, new Point(0,0)), f._dir, f.Position, f.Size),
    //         // ThrowBeer tb => new ProjectileSaveModel(new BasicSaveModel(TYPES.THROWBEER, projectile.Transform.Position, projectile.Transform.Size), tb.HasHit, tb.Movement.Direction, tb.Movement.IsMoving, tb.Movement.CanMove, tb.Movement.Rotation, tb.weaponAttributes, Vector2.Zero),
    //         // ThrowBanana banana => new ProjectileSaveModel(new BasicSaveModel(TYPES.THROWBANANA, projectile.Transform.Position, projectile.Transform.Size), banana.HasHit, banana.Movement.Direction, banana.Movement.IsMoving, banana.Movement.CanMove, banana.Movement.Rotation, banana.weaponAttributes, Vector2.Zero),
    //         // ThrowBook book => new ProjectileSaveModel(new BasicSaveModel(TYPES.THROWBOOK, projectile.Transform.Position, projectile.Transform.Size), book.HasHit, book.Movement.Direction, book.Movement.IsMoving, book.Movement.CanMove, book.Movement.Rotation, book.weaponAttributes),
    //         // ThrowBottle tbo => new ProjectileSaveModel(new BasicSaveModel(TYPES.THROWBOTTLE, projectile.Transform.Position, projectile.Transform.Size), tbo.HasHit, tbo.Movement.Direction, tbo.Movement.IsMoving, tbo.Movement.CanMove, tbo.Movement.Rotation, tbo.weaponAttributes),
    //         _ => throw new NotSupportedException($"Projectile type {aoe.GetType().Name} is not supported for saving.")
    //     };
    // }
    private static ItemSaveModel MakeItemSaveModel(ItemBase item)
    {
        return item switch
        {
            Beer b => new ItemSaveModel(TYPES.BEER, item.Transform.Position, item.Transform.Size),
            Xp_Money b => new ItemSaveModel(TYPES.MONEY, item.Transform.Position, item.Transform.Size),
            EnergyGel e => new ItemSaveModel(TYPES.ENERGY_GEL, item.Transform.Position, item.Transform.Size),
            DopingSpritze e => new ItemSaveModel(TYPES.DOPING, item.Transform.Position, item.Transform.Size),
            EnergyBar eb => new ItemSaveModel(TYPES.ENERGY_BAR, item.Transform.Position, item.Transform.Size),
            Frelo f => new ItemSaveModel(TYPES.FRELO, item.Transform.Position, item.Transform.Size),
            DogFood df => new ItemSaveModel(TYPES.DOG_FOOD, item.Transform.Position, item.Transform.Size),
            RacingBike r => new ItemSaveModel(TYPES.RACINGBIKE, item.Transform.Position, item.Transform.Size),
            _ => throw new NotSupportedException($"Item type {item.GetType().Name} is not supported for saving.")
        };
    }

    private static CarSaveModel MakeCarSaveModel(Car carBase)
    {
        return new CarSaveModel(0, carBase.Transform.Position, carBase.Transform.Size, carBase._rng, carBase.SideKey, carBase.UpKey);
    }
    private static ObjectSaveModel MakeObjectSaveModel(ObjectBase obj)
    {
        return obj switch
        {
            Chest c => new ObjectSaveModel(TYPES.CHEST, obj.Transform.Position, obj.Transform.Size)
            {
                IsOpen = c.Open,
            },
            DogBowl db => new ObjectSaveModel(TYPES.DOGBOWL, obj.Transform.Position, obj.Transform.Size)
            {
                IsFull = db.Full,
            },
            BikeShop bs => new ObjectSaveModel(TYPES.BIKESHOP, obj.Transform.Position, obj.Transform.Size),
            DestructibleObject dObj => new ObjectSaveModel(TYPES.DESTRUCTIBLE, obj.Transform.Position, obj.Transform.Size)
            {
                Health = dObj.Health,
                SpriteKey = dObj.SpriteKey,
            },
            AchievementTrigger at => new ObjectSaveModel(TYPES.TRIGGER, obj.Transform.Position, obj.Transform.Size)
            {
                Id = at.Id,
            },
            Musicians mu => new ObjectSaveModel(TYPES.MUSICIANS, obj.Transform.Position, obj.Transform.Size),
            _ => throw new NotSupportedException($"Object type {obj.GetType().Name} is not supported for saving.")
        };
    }

    private static PlayerSaveModel MakePlayerSaveModel(uint playerNumber, Player player)
    {
        return new PlayerSaveModel(playerNumber, player.Transform.Position, player.Transform.Radius, player.RenderTransform.Size);
    }

    private static CharacterSaveModel MakeCharacterSaveModel(CharacterBase character)
    {
        return character switch
        {
            Hobo h => new CharacterSaveModel(TYPES.HOBO, character.Transform.Position, character.Transform.Radius),
            BikeThief bt => new CharacterSaveModel(TYPES.BIKETHIEF, character.Transform.Position, character.Transform.Radius),
            Dog dg => new CharacterSaveModel(TYPES.DOG, character.Transform.Position, character.Transform.Radius),
            PoliceMan pm => new CharacterSaveModel(TYPES.POLICE_MAN, character.Transform.Position, character.Transform.Radius),
            Dozent dz => new CharacterSaveModel(TYPES.DOZENT, character.Transform.Position, character.Transform.Radius),
            KamikazeOpa opa => new CharacterSaveModel(TYPES.KAMIKAZE_OPA, character.Transform.Position, character.Transform.Radius),
            _ => throw new NotSupportedException($"Character type {character.GetType().Name} is not supported for saving.")
        };
    }
    private static List<ProjectileSaveModel> MakeProjectileSaveList(List<ProjectileBase> pList)
    {
        List<ProjectileSaveModel> crtList = new List<ProjectileSaveModel>();
        foreach (var p in pList)
        {
            crtList.Add(MakeProjectileSaveModel(p));
        }
        return crtList;
    }

    // private static List<AOESaveModel> MakeAOESaveList(List<AreaOfEffectBase> aoeList)
    // {
    //     List<AOESaveModel> crtList = new List<AOESaveModel>();
    //     foreach (var aoe in aoeList)
    //     {
    //         crtList.Add(MakeAOESaveModel(aoe));
    //     }
    //     return crtList;
    // }
    private static List<CharacterSaveModel> MakeCharacterSaveList(List<CharacterBase> pList)
    {
        List<CharacterSaveModel> crtList = new List<CharacterSaveModel>();
        foreach (var p in pList)
        {
            crtList.Add(MakeCharacterSaveModel(p));
        }
        return crtList;
    }

    private static List<ItemSaveModel> MakeItemSaveList(List<ItemBase> pList)
    {
        List<ItemSaveModel> crtList = new List<ItemSaveModel>();
        foreach (var p in pList)
        {
            crtList.Add(MakeItemSaveModel(p));
        }
        return crtList;
    }

    private static List<ObjectSaveModel> MakeObjectSaveList(List<ObjectBase> list)
    {
        List<ObjectSaveModel> set = new List<ObjectSaveModel>();
        foreach (var o in list)
            set.Add(MakeObjectSaveModel(o));
        return set;
    }

    private static List<CarSaveModel> MakeCarsSaveList(List<Car> list)
    {
        List<CarSaveModel> set = new List<CarSaveModel>();
        foreach (var c in list)
            set.Add(MakeCarSaveModel(c));
        return set;
    }

    private static List<TramSaveModel> MakeTramsSaveList(List<Tram> list)
    {
        List<TramSaveModel> set = new List<TramSaveModel>();
        foreach (var c in list)
            set.Add(MakeTramSaveModel(c));
        return set;
    }

    private static string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        return $"{minutes:00}:{secs:00}";
    }
    
    // Settings made persistent by the following classes/functions
    
    public class SettingsData
    {
        // includes all settings that should be made persistent
        public float MusicVolume { get; set; } = 1.0f;
        public float SfxVolume { get; set; } = 1.0f;
    }
    
    // path where the settings.json file is stored
    private static readonly string SETTINGS_PATH = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "Settings.json");
    
    public static void SaveSettings(float music, float sfx)
    {
        try
        {
            var settings = new SettingsData { MusicVolume = music, SfxVolume = sfx };
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SETTINGS_PATH, json);
        }
        catch (Exception ex) { Console.WriteLine("Settings save failed: " + ex.Message); }
    }
    
    public static SettingsData LoadSettings()
    {
        // Load with standard values, if file doesn't exist
        if (!File.Exists(SETTINGS_PATH)) return new SettingsData();
        try
        {
            string json = File.ReadAllText(SETTINGS_PATH);
            return JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
        }
        catch { return new SettingsData(); }
    }
    
}