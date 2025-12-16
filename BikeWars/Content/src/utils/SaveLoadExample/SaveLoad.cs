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
        BIKETHIEF,
        CHEST,
        ENERGY_GEL,
        BEER,
        MONEY
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
        public float PlayerX { get; set; } = _worldBounds;
        public float PlayerY { get; set; } = _worldBounds;
        public List<ProjectileSaveModel> Projectiles {get; set;}
        public List<CharacterSaveModel> Characters {get; set;}
        public List<ItemSaveModel> Items {get; set;}
        public List<Statistic> Statistics{get; set;}
        public Statistic Statistic{get; set;}
        public int GameMode { get; set; } = 0;
    }

    public class BasicSaveModel
    {
        public TYPES Type {get; set;} // Type of the projectile. Like bullet

        public Vector2Save Position {get;set;}

        public PointSave Size {get;set;}

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
        public BasicSaveModel Basic {get;set;}
        public ProjectileSaveModel() {}

        public int Damage {get; set;}
        public bool HasHit {get; set;}

        public Vector2Save Direction {get; set;}
        public bool IsMoving {get; set;}
        public bool CanMove {get; set;}
        public float Rotation {get; set;}

        public ProjectileSaveModel(BasicSaveModel b, int damage, bool hasHit, Vector2 direction, bool isMoving, bool canMove, float rotation)
        {
            Basic = b;
            Damage = damage;
            HasHit = hasHit;
            Direction = new Vector2Save(direction);
            IsMoving = isMoving;
            CanMove = canMove;
            Rotation = rotation;
        }
    }

    public class CharacterSaveModel
    {
        public TYPES Type {get; set;} // Character Type Like Hobo

        public Vector2Save Position {get;set;}

        public PointSave Size {get;set;}

        public CharacterSaveModel() {}

        public CharacterSaveModel(TYPES type, Vector2 position, Point size)
        {
            Type = type;
            Position = new Vector2Save(position);
            Size = new PointSave(size);
        }
    }

    public class ItemSaveModel
    {
        public TYPES Type {get; set;} // Item Type Like Chest

        public Vector2Save Position {get;set;}

        public PointSave Size {get;set;}

        public ItemSaveModel() {}

        public ItemSaveModel(TYPES type, Vector2 position, Point size)
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
    // public static void SaveGame(int counter, Transform playerPosition, List<ProjectileBase> projectiles)
    public static void SaveGame(GameTimer gameTimer, GameObjectManager gameObjectManager, StatisticsManager statisticsManager, GameMode gameMode)
    {
        try
        {
            // serialize the current info into JSON text
            GameState state = new GameState
            {
                GameMode = (int)gameMode,
                GameTimerCurrentTime = gameTimer.CurrentTime,
                GameTimerTotalTime = gameTimer.TotalTime,
                IsGameTimerRunning = gameTimer.IsRunning,
                IsGameTimerPaused = gameTimer.IsPaused,

                PlayerX = gameObjectManager.Player1.Transform.Position.X,
                PlayerY = gameObjectManager.Player1.Transform.Position.Y,

                Projectiles = MakeProjectileSaveList(gameObjectManager.Projectiles),
                Characters = MakeCharacterSaveList(gameObjectManager.Characters),
                Items = MakeItemSaveList(gameObjectManager.Items),
                Statistics = statisticsManager.Statistics,
                Statistic = statisticsManager.Statistic
            };
            string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });

            // get the folder where the JSON file will be saved
            // if it doesn't exist yet, it creates one
            string dir = Path.GetDirectoryName(SAVE_PATH);
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
    public static void SaveNonGame(StatisticsManager statisticsManager)
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

                PlayerX = loadState.PlayerX,
                PlayerY = loadState.PlayerY,

                Projectiles = loadState.Projectiles,
                Characters = loadState.Characters,
                Items = loadState.Items,
                Statistics = statisticsManager.Statistics,
                Statistic = statisticsManager.Statistic
            };
            string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });

            // get the folder where the JSON file will be saved
            // if it doesn't exist yet, it creates one
            string dir = Path.GetDirectoryName(SAVE_PATH);
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
        Console.WriteLine("Loaded. Player Position=" + state.PlayerX + " " + state.PlayerY);
        return state;
    }

    private static ProjectileSaveModel MakeProjectileSaveModel(ProjectileBase projectile)
    {
        return projectile switch
        {
            Bullet b => new ProjectileSaveModel(new BasicSaveModel(TYPES.BULLET, projectile.Transform.Position, projectile.Transform.Size), b.Damage, b.HasHit, b.Movement.Direction, b.Movement.IsMoving, b.Movement.CanMove, b.Movement.Rotation),
        };
    }
    private static ItemSaveModel MakeItemSaveModel(ItemBase item)
    {
        return item switch
        {
            Chest c => new ItemSaveModel(TYPES.CHEST, item.Transform.Position, item.Transform.Size),
            Xp_Beer b => new ItemSaveModel(TYPES.BEER, item.Transform.Position, item.Transform.Size),
            Xp_Money b => new ItemSaveModel(TYPES.MONEY, item.Transform.Position, item.Transform.Size),
            EnergyGel e => new ItemSaveModel(TYPES.ENERGY_GEL, item.Transform.Position, item.Transform.Size)
        };
    }
    private static CharacterSaveModel MakeCharacterSaveModel(CharacterBase character)
    {
        return character switch
        {
            Hobo h => new CharacterSaveModel(TYPES.HOBO, character.Transform.Position, character.Transform.Size),
            BikeThief bt => new CharacterSaveModel(TYPES.BIKETHIEF, character.Transform.Position, character.Transform.Size),
            Dog dg => new CharacterSaveModel(TYPES.DOG, character.Transform.Position, character.Transform.Size)
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

    private static string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        return $"{minutes:00}:{secs:00}";
    }
}