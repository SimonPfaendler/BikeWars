using System;
using System.Diagnostics.Metrics;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BikeWars.Content.src.utils.SaveLoadExample;

public static class SaveLoad
{
    // save file path in the user's Documents folder
    private static readonly string SAVE_PATH = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "SaveData.json");

    // Stores the data that will be saved to and loaded from a JSON file.
    public class GameState
    {
        public int Counter { get; set; }
    }



    // save the counter in a JSON file
    public static void SaveGame(int counter)
    {
        try
        {
            // serialize the current counter into JSON text
            GameState state = new GameState { Counter = counter };
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
    public static int LoadGame()
    {
        if (!File.Exists(SAVE_PATH))
        {
            Console.WriteLine("No save file yet — starting with default values.");
            return 0;
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
        return state.Counter;
    }

}