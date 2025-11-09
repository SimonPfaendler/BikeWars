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
    // Stores the data that will be saved to and loaded from a JSON file.
    public class GameState
    {
        public int Counter { get; set; }
    }
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    // counter visual
    private SpriteFont _font;
    private Vector2 _counterPosition = new Vector2(20, 100);
    
    // counter logic
    int counter = 0;
    private float _timer = 0;
    
    // remembers the last pressed key
    private KeyboardState _prevKbState;
    
    // Save file path in the user's Documents folder
    readonly string _savePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
        "SaveData.json");
    public SaveLoad()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // auto load if a previous save exists
        LoadGame();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _font = Content.Load<SpriteFont>("Font");

    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState kbState = Keyboard.GetState();
        
        // counts the passed time between each frame and adds it to time
        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
    
        // increase the counter once per second (before, it increased every frame)
        if (_timer >= 1)
        {
            counter++;
            _timer = 0;
        }
        
        // save when S is pressed
        if (kbState.IsKeyDown(Keys.S) && !_prevKbState.IsKeyDown(Keys.S))
        {
            SaveGame();
        }
        
        // load when L is pressed
        if (kbState.IsKeyDown(Keys.L) && !_prevKbState.IsKeyDown(Keys.L))
        {
            LoadGame();
        }
        
        // exit without saving when esc is pressed
        if (kbState.IsKeyDown(Keys.Escape) && !_prevKbState.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // update _prevKb for the next frame
        _prevKbState = kbState;
        

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        // drawing the counter
        _spriteBatch.Begin();
        
        _spriteBatch.DrawString(_font, "Counter: " + counter, _counterPosition, Color.Black);
        _spriteBatch.DrawString(_font, "Press S = Save & Quit, Esc = Quit (no save)", 
            new Vector2(20, 200), Color.Black);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
    
    // save the counter in a JSON file
    private void SaveGame()
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
    private void LoadGame()
    {
        try
        {
            if (!File.Exists(_savePath))
            {
                Console.WriteLine("No save file yet.");
                return;
            }
           
            // read the contents of the JSON file into a string
            // and convert the JSON string back into a GameState object
            string json = File.ReadAllText(_savePath);
            GameState state = JsonSerializer.Deserialize<GameState>(json);
            
            // if loading worked, continue counting from the saved number
            if (state != null)
            {
                counter = state.Counter;
                _timer = 0;
                Console.WriteLine("Loaded. Counter=" + counter);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Load failed: " + ex.Message);
        }
    }
}