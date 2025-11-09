using System;
using System.Diagnostics.Metrics;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BikeWars.Content.src.utils.SaveLoadExample;

public class SaveLoad : Game
{
    // Save file path in the user's Documents folder
    readonly static string _savePath = Path.Combine(
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
            GameState state = new GameState {Counter = counter};
            string json = JsonSerializer.Serialize(state , new JsonSerializerOptions { WriteIndented = true });
            
            // get the folder where the JSON file will be saved
            // if it doesn't exist yet, it creates one
            string dir = Path.GetDirectoryName(_savePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            
            // write the JSON file and print a message in the console when it's saved successfully
            File.WriteAllText(_savePath, json);
            Console.WriteLine("Saved: " + _savePath);
        }
        
        // if the save isn't successful it will print a message in the console
        catch (Exception ex)
        {
            Console.WriteLine("Save failed: " + ex.Message);
        }
    }
    
    // transform the JSON object to a C# object
    public static bool LoadGame(out int counter)
    {
        counter = 0;
        try
        {
            if (!File.Exists(_savePath))
            {
                Console.WriteLine("No save file yet.");
                return false;
            }
           
            // read the contents of the JSON file into a string
            // and convert the JSON string back into a GameState object
            string json = File.ReadAllText(_savePath);
            GameState? state = JsonSerializer.Deserialize<GameState>(json);
            
            // if loading worked, continue counting from the saved number
            if (state != null)
            {
                counter = state.Counter;
                Console.WriteLine("Loaded. Counter=" + counter);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Load failed: " + ex.Message);
        }
        return false;
    }
}