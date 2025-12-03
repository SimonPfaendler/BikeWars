using System;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.entities.items;
using BikeWars.Content.managers;
using BikeWars.Entities.Characters;

namespace BikeWars.Content.src.utils.SaveLoadExample;

public static class SaveLoad
{
    private static int _worldBounds = 11200 / 2;
    public enum TYPES
    {
        BULLET,
        HOBO,
        BIKETHIEF,
        CHEST
    }
    // save file path in the user's Documents folder
    private static readonly string SAVE_PATH = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "SaveData.json");

    // Stores the data that will be saved to and loaded from a JSON file.
    // The data should have a default set, in case the information isn't in the JSON file yet
    public class GameState
    {
        public int Counter { get; set; } = 0;
        public float PlayerX { get; set; } = _worldBounds;
        public float PlayerY { get; set; } = _worldBounds;
        public List<ProjectileSaveModel> Projectiles {get; set;}
        public List<CharacterSaveModel> Characters {get; set;}
        public List<ItemSaveModel> Items {get; set;}
    }

    public class ProjectileSaveModel
    {
        public TYPES Type {get; set;} // Type of the projectile. Like bullet

        public Vector2Save Position {get;set;}

        public PointSave Size {get;set;}

        public ProjectileSaveModel() {}

        public ProjectileSaveModel(TYPES type, Vector2 position, Point size)
        {
            Type = type;
            Position = new Vector2Save(position);
            Size = new PointSave(size);
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
    public static void SaveGame(int counter, GameObjectManager gameObjectManager)
    {
        try
        {
            // serialize the current info into JSON text
            GameState state = new GameState
            {
                Counter = counter,
                PlayerX = gameObjectManager.Player1.Transform.Position.X,
                PlayerY = gameObjectManager.Player1.Transform.Position.Y,
                Projectiles = MakeProjectileSaveList(gameObjectManager.Projectiles),
                Characters = MakeCharacterSaveList(gameObjectManager.Characters),
                // Items = MakeItemSaveList(gameObjectManager.Items),
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

        Console.WriteLine("Loaded. Counter=" + state.Counter);
        Console.WriteLine("Loaded. Player Position=" + state.PlayerX + " " + state.PlayerY);
        return state;
    }

    private static ProjectileSaveModel MakeProjectileSaveModel(ProjectileBase projectile)
    {
        return projectile switch
        {
            Bullet b => new ProjectileSaveModel(TYPES.BULLET, projectile.Transform.Position, projectile.Transform.Size),
        };
    }
    private static ItemSaveModel MakeItemSaveModel(ItemBase item)
    {
        return item switch
        {
            Chest c => new ItemSaveModel(TYPES.CHEST, item.Transform.Position, item.Transform.Size)
        };
    }
    private static CharacterSaveModel MakeCharacterSaveModel(CharacterBase character)
    {
        return character switch
        {
            Hobo h => new CharacterSaveModel(TYPES.HOBO, character.Transform.Position, character.Transform.Size),
            BikeThief bt => new CharacterSaveModel(TYPES.BIKETHIEF, character.Transform.Position, character.Transform.Size)
        };
    }
    private static List<ProjectileSaveModel> MakeProjectileSaveList(List<ProjectileBase> pList)
    {
        List<ProjectileSaveModel> crtList = [];
        foreach (var p in pList)
        {
            crtList.Add(MakeProjectileSaveModel(p));
        }
        return crtList;
    }
    private static List<CharacterSaveModel> MakeCharacterSaveList(List<CharacterBase> pList)
    {
        List<CharacterSaveModel> crtList = [];
        foreach (var p in pList)
        {
            crtList.Add(MakeCharacterSaveModel(p));
        }
        return crtList;
    }

    private static List<ItemSaveModel> MakeItemSaveList(List<ItemBase> pList)
    {
        List<ItemSaveModel> crtList = [];
        foreach (var p in pList)
        {
            crtList.Add(MakeItemSaveModel(p));
        }
        return crtList;
    }
}