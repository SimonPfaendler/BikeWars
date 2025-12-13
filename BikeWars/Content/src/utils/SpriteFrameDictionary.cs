using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BikeWars.Content.utils
{
    /// <summary>
    /// Stellt ein statisches Dictionary bereit, das alle Rectangle-Koordinaten
    /// für die Animationen auf dem Character Atlas beinhaltet.
    /// Liest die Koordinaten automatisch aus dem TexturePacker JSON.
    /// 
    /// TO DO bei neuen Sprites:
    /// 1. Sprite im TexturePacker hinzufügen & JSON (character_atlas_koordinaten) updaten.
    /// 2. Hier in 'InitializeAnimations()' neuen Eintrag hinzufügen:
    ///    AnimationFrames["Name"] = GetFramesFromAtlas("Dateiname.png", cols, rows);
    /// 3. Falls Frame-Anzahl ungleich Grid (z.B. 3 Frames in 2x2 Grid), nutze .Take(n).ToList().
    /// 4. In der Entity-Klasse den neuen Key verwenden.
    /// </summary>
    public static class SpriteFrameDictionary
    {
        // Cache für die geladenen Atlas-Daten: Filename -> AtlasRect
        private static Dictionary<string, AtlasRect> _atlasEntries = new Dictionary<string, AtlasRect>();

        // ZENTRALES DICTIONARY
        public static readonly Dictionary<string, List<Rectangle>> AnimationFrames = new Dictionary<string, List<Rectangle>>();

        static SpriteFrameDictionary()
        {
            LoadAtlasData();
            InitializeAnimations();
        }

        private static void LoadAtlasData()
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "assets", "sprites", "characters", "character_atlas_koordinaten");
            string path = File.Exists(basePath) ? basePath : basePath + ".json";

            if (!File.Exists(path))
            {
                System.Diagnostics.Debug.WriteLine($"[SpriteFrameDictionary] ERROR: Atlas JSON not found at {basePath}");
                return;
            }

            try 
            {
                var root = JsonSerializer.Deserialize<AtlasRoot>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (root?.frames != null)
                {
                    foreach (var f in root.frames)
                    {
                        if (f.filename != null) _atlasEntries[f.filename] = f.frame;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SpriteFrameDictionary] ERROR: {ex.Message}");
            }
        }

        private static List<Rectangle> GetFramesFromAtlas(string filename, int cols, int rows)
        {
            if (!_atlasEntries.TryGetValue(filename, out AtlasRect rect))
            {
                // Return empty list
                System.Diagnostics.Debug.WriteLine($"[SpriteFrameDictionary] Missing frame: {filename}");
                return new List<Rectangle>();
            }

            var frames = new List<Rectangle>();
            
             // Calculate frame size
            int cellW = rect.w / cols;
            int cellH = rect.h / rows;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // for TexturePackerGUI
                    int x = rect.x + (int)((double)rect.w / cols * c);
                    int y = rect.y + (int)((double)rect.h / rows * r);
                    int w = (int)((double)rect.w / cols * (c + 1)) - (int)((double)rect.w / cols * c);
                    int h = (int)((double)rect.h / rows * (r + 1)) - (int)((double)rect.h / rows * r);

                    frames.Add(new Rectangle(x, y, w, h));
                }
            }
            return frames;
        }

        private static void InitializeAnimations()
        {
            // Spielfiguren:
            
            // c1:
            // Bike Animations
            AnimationFrames["Character1_BikeDown"] = GetFramesFromAtlas("c1_bike_down.png", 1, 2);
            AnimationFrames["Character1_BikeLeft"] = GetFramesFromAtlas("c1_bike_left.png", 1, 2);
            AnimationFrames["Character1_BikeRight"] = GetFramesFromAtlas("c1_bike_right.png", 1, 2);
            AnimationFrames["Character1_BikeUp"] = GetFramesFromAtlas("c1_bike_up.png", 1, 2);

            // Walk Animations
            AnimationFrames["Character1_WalkDown"] = GetFramesFromAtlas("c1_walking_down.png", 2, 2);
            AnimationFrames["Character1_WalkLeft"] = GetFramesFromAtlas("c1_walking_left.png", 2, 2);
            AnimationFrames["Character1_WalkRight"] = GetFramesFromAtlas("c1_walking_right.png", 2, 2);
            AnimationFrames["Character1_WalkUp"] = GetFramesFromAtlas("c1_walking_up.png", 2, 2);

            // Enemies
            
            //e1:
            AnimationFrames["Hobo_Idle"] = GetFramesFromAtlas("e1_drunkdude_standing.png", 1, 1);
            // Fix: Take(3).ToList() hilft wenn eine animation eine ungerade frameanzahl hat (z.B. 2x2 grid assumed aber nur 3 frames) 
            AnimationFrames["Hobo_WalkLeft"] = GetFramesFromAtlas("e1_drunkdude_walking_left.png", 2, 2).Take(3).ToList();
            AnimationFrames["Hobo_WalkRight"] = GetFramesFromAtlas("e1_drunkdude_walking_right.png", 2, 2).Take(3).ToList();
            AnimationFrames["Hobo_WalkDown"] = GetFramesFromAtlas("e1_drunkdude_walking_down.png", 3, 2);
            AnimationFrames["Hobo_WalkUp"] = GetFramesFromAtlas("e1_drunkdude_walking_up.png", 2, 2).Take(3).ToList();
            
            //e2
            var btWalkRight = GetFramesFromAtlas("e2_bikethief_walking_right.png", 2, 3);
            AnimationFrames["BikeThief_Idle"] = btWalkRight.Count > 0 ? new List<Rectangle> { btWalkRight[0] } : new List<Rectangle>();
            AnimationFrames["BikeThief_WalkLeft"] = GetFramesFromAtlas("e2_bikethief_walking_left.png", 2, 3);
            AnimationFrames["BikeThief_WalkRight"] = btWalkRight;
            
            // Spezialattacken:
             AnimationFrames["Flamethrower_Attack"] = new List<Rectangle>();
             for (int i = 0; i < 10; i++) AnimationFrames["Flamethrower_Attack"].Add(new Rectangle(i * 32, 0, 32, 112));

             AnimationFrames["IceTrail_Attack"] = new List<Rectangle>();
             for (int i = 0; i < 5; i++) AnimationFrames["IceTrail_Attack"].Add(new Rectangle(i * 64, 0, 64, 60));
        }

        public static List<Rectangle> GetFrames(string key)
        {
            return AnimationFrames.TryGetValue(key, out var frames) ? frames : throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        // JSON Data Structures
        private class AtlasRoot { public List<AtlasFrame> frames { get; set; } }
        private class AtlasFrame { public string filename { get; set; } public AtlasRect frame { get; set; } }
        private class AtlasRect { public int x { get; set; } public int y { get; set; } public int w { get; set; } public int h { get; set; } }
    }
}