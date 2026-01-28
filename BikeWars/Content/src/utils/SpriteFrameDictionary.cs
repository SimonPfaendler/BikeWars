using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using BikeWars.Content.engine;

namespace BikeWars.Content.utils
{
    /// <summary>
    /// Stellt ein statisches Dictionary bereit, das alle Rectangle-Koordinaten
    /// für die Animationen auf dem Character Atlas beinhaltet.
    /// Liest die Koordinaten automatisch aus dem TexturePacker JSON.
    /// 
    /// TO DO bei neuen Sprites:
    /// 1. Sprite im TexturePacker hinzufügen & JSON (character_atlas.json) updaten.
    /// 2. Hier in 'InitializeAnimations()' neuen Eintrag hinzufügen:
    ///    AnimationFrames["Name"] = GetFramesFromAtlas("Dateiname.png", cols, rows);
    /// 3. Falls Frame-Anzahl ungleich Grid (z.B. 3 Frames in 2x2 Grid), nutze .Take(n).ToList().
    /// 4. In der Entity-Klasse den neuen Key verwenden.
    /// </summary>
    public static class SpriteFrameDictionary
    {
        // Cache für die geladenen Atlas-Daten: Filename -> TexturePackerRect
        private static Dictionary<string, TexturePackerRect> _atlasEntries = new Dictionary<string, TexturePackerRect>();

        // ZENTRALES DICTIONARY
        private static readonly Dictionary<string, List<Rectangle>> AnimationFrames = new Dictionary<string, List<Rectangle>>();

        static SpriteFrameDictionary()
        {
            LoadAtlasData();
            InitializeAnimations();
        }

        private static void LoadAtlasData()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "assets", "sprites", "characters", "character_atlas.json");

            if (!File.Exists(path))
            {
                System.Diagnostics.Debug.WriteLine($"[SpriteFrameDictionary] ERROR: Atlas JSON not found at {path}");
                return;
            }

            try 
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var root = JsonSerializer.Deserialize<TexturePackerRoot>(File.ReadAllText(path), options);
                if (root?.Frames != null)
                {
                    foreach (var f in root.Frames)
                    {
                        if (f.Filename != null) _atlasEntries[f.Filename] = f.Frame;
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
            if (!_atlasEntries.TryGetValue(filename, out TexturePackerRect rect))
            {
                // Return empty list
                System.Diagnostics.Debug.WriteLine($"[SpriteFrameDictionary] Missing frame: {filename}");
                return new List<Rectangle>();
            }

            var frames = new List<Rectangle>();
            
             // Calculate frame size
            int cellW = rect.W / cols;
            int cellH = rect.H / rows;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // for TexturePackerGUI
                    int x = rect.X + (int)((double)rect.W / cols * c);
                    int y = rect.Y + (int)((double)rect.H / rows * r);
                    int w = (int)((double)rect.W / cols * (c + 1)) - (int)((double)rect.W / cols * c);
                    int h = (int)((double)rect.H / rows * (r + 1)) - (int)((double)rect.H / rows * r);

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
            AnimationFrames["Hobo_Throw"] = GetFramesFromAtlas("e1_drunkdude_longrange_attack.png", 6, 1);
            
            //e2
            var btWalkRight = GetFramesFromAtlas("e2_bikethief_walking_right.png", 2, 3);
            AnimationFrames["BikeThief_Idle"] = btWalkRight.Count > 0 ? new List<Rectangle> { btWalkRight[0] } : new List<Rectangle>();
            AnimationFrames["BikeThief_WalkLeft"] = GetFramesFromAtlas("e2_bikethief_walking_left.png", 2, 3);
            AnimationFrames["BikeThief_WalkRight"] = btWalkRight;
            
            // Dog
            AnimationFrames["Dog_Idle"] = GetFramesFromAtlas("e3_dog_walking_right.png", 1, 1);
            var dogLeft = GetFramesFromAtlas("e3_dog_walking_left.png", 1, 1);
            dogLeft.AddRange(GetFramesFromAtlas("e3_dog_walking_left_2.png", 1, 1));
            AnimationFrames["Dog_WalkLeft"] = dogLeft;
            var dogRight = GetFramesFromAtlas("e3_dog_walking_right.png", 1, 1);
            dogRight.AddRange(GetFramesFromAtlas("e3_dog_walking_right_2.png", 1, 1));
            AnimationFrames["Dog_WalkRight"] = dogRight;
            AnimationFrames["Dog_WalkUp"] = GetFramesFromAtlas("e3_dog_walking_up.png", 1, 1);
            
            var dogDown = GetFramesFromAtlas("e3_dog_walking_down_1.png", 1, 1);
            dogDown.AddRange(GetFramesFromAtlas("e3_dog_walking_down_2.png", 1, 1));
            AnimationFrames["Dog_WalkDown"] = dogDown;

            // c2:
            // Bike Animations
            var c2BikeUp = GetFramesFromAtlas("c2_bike_up.png", 6, 1);
            AnimationFrames["Character2_BikeUp"] = c2BikeUp;


             // Walk Animations
             AnimationFrames["Character2_WalkDown"] = GetFramesFromAtlas("c2_walk_down.png", 4, 3);
             AnimationFrames["Character2_WalkLeft"] = GetFramesFromAtlas("c2_walk_left.png", 8, 1);
             AnimationFrames["Character2_WalkRight"] = GetFramesFromAtlas("c2_walk_right.png", 8, 1);
             AnimationFrames["Character2_WalkUp"] = GetFramesFromAtlas("c2_walk_up.png", 4, 3);
             AnimationFrames["Character2_Idle"] = GetFramesFromAtlas("c2_idle.png", 14, 11).Take(104).ToList();
             // Idle for Character 1 (uses first frame of WalkDown)
            AnimationFrames["Character1_Idle"] = new List<Rectangle> { AnimationFrames["Character1_WalkDown"][0] };

            // Spezialattacken:
             AnimationFrames["Flamethrower_Attack"] = new List<Rectangle>();
             for (int i = 0; i < 10; i++) AnimationFrames["Flamethrower_Attack"].Add(new Rectangle(i * 32, 0, 32, 112));

             AnimationFrames["IceTrail_Attack"] = new List<Rectangle>();
             for (int i = 0; i < 5; i++) AnimationFrames["IceTrail_Attack"].Add(new Rectangle(i * 64, 0, 64, 60));

             // e4 (KamikazeOpa):
             // Movement (1 row, 6 columns)
             AnimationFrames["KamikazeOpa_BikeDown"] = GetFramesFromAtlas("e4_kamikazeopa_bike_down.png", 6, 1);
             AnimationFrames["KamikazeOpa_BikeLeft"] = GetFramesFromAtlas("e4_kamikazeopa_bike_left.png", 6, 1);
             AnimationFrames["KamikazeOpa_BikeRight"] = GetFramesFromAtlas("e4_kamikazeopa_bike_right.png", 6, 1);
             AnimationFrames["KamikazeOpa_BikeUp"] = GetFramesFromAtlas("e4_kamikazeopa_bike_up.png", 6, 1);

             // Death (1 row, 9 columns, 9 frames total)
             AnimationFrames["KamikazeOpa_Death"] = GetFramesFromAtlas("e4_kamikazeopa_death_animation.png", 9, 1);
             
             // Dozent
             AnimationFrames["Dozent_Idle"] = GetFramesFromAtlas("e5_dozent_attack.png", 1, 1);
             var dozentLeft = GetFramesFromAtlas("e5_dozent_left1.png", 1, 1);
             dozentLeft.AddRange(GetFramesFromAtlas("e5_dozent_left2.png", 1, 1));
             dozentLeft.AddRange(GetFramesFromAtlas("e5_dozent_left3.png", 1, 1));
             dozentLeft.AddRange(GetFramesFromAtlas("e5_dozent_left4.png", 1, 1));
             dozentLeft.AddRange(GetFramesFromAtlas("e5_dozent_left5.png", 1, 1));
             AnimationFrames["Dozent_WalkLeft"] = dozentLeft;
             var dozentRight = GetFramesFromAtlas("e5_dozent_right1.png", 1, 1);
             dozentRight.AddRange(GetFramesFromAtlas("e5_dozent_right2.png", 1, 1));
             dozentRight.AddRange(GetFramesFromAtlas("e5_dozent_right3.png", 1, 1));
             dozentRight.AddRange(GetFramesFromAtlas("e5_dozent_right4.png", 1, 1));
             dozentRight.AddRange(GetFramesFromAtlas("e5_dozent_right5.png", 1, 1));
             AnimationFrames["Dozent_WalkRight"] = dozentRight;
             
             var dozentUp = GetFramesFromAtlas("e5_dozent_up1.png", 1, 1);
             dozentUp.AddRange(GetFramesFromAtlas("e5_dozent_up2.png", 1, 1));
             AnimationFrames["Dozent_WalkUp"] = dozentUp;

             var dozentDown = GetFramesFromAtlas("e5_dozent_down1.png", 1, 1);
             dozentDown.AddRange(GetFramesFromAtlas("e5_dozent_down2.png", 1, 1));
             AnimationFrames["Dozent_WalkDown"] = dozentDown;
             
             // ravers
             AnimationFrames["Raver02_LeftUp"]  = GetFramesFromAtlas("raver02_left_up.png", 1, 1);
             AnimationFrames["Raver02_RightUp"] = GetFramesFromAtlas("raver02_right_up.png", 1, 1);

             AnimationFrames["Raver03_LeftUp"]  = GetFramesFromAtlas("raver03_left_up.png", 1, 1);
             AnimationFrames["Raver03_RightUp"] = GetFramesFromAtlas("raver03_right_up.png", 1, 1);

             AnimationFrames["Raver04_LeftUp"]  = GetFramesFromAtlas("raver04_left_up.png", 1, 1);
             AnimationFrames["Raver04_RightUp"] = GetFramesFromAtlas("raver04_right_up.png", 1, 1);

             AnimationFrames["Raver06_LeftUp"]  = GetFramesFromAtlas("raver06_left_up.png", 1, 1);
             AnimationFrames["Raver06_RightUp"] = GetFramesFromAtlas("raver06_right_up.png", 1, 1);
             
             // policeman
             var policeWalkRight = new List<Rectangle>();
             policeWalkRight.AddRange(GetFramesFromAtlas("policeman_walking_right_1.png", 1, 1));
             policeWalkRight.AddRange(GetFramesFromAtlas("policeman_walking_right_2.png", 1, 1));
             policeWalkRight.AddRange(GetFramesFromAtlas("policeman_walking_right_3.png", 1, 1));
             AnimationFrames["policeman_walking_right"] = policeWalkRight;
             
             var policeWalkLeft = new List<Rectangle>();
             policeWalkLeft.AddRange(GetFramesFromAtlas("policeman_walking_left_1.png", 1, 1));
             policeWalkLeft.AddRange(GetFramesFromAtlas("policeman_walking_left_2.png", 1, 1));
             policeWalkLeft.AddRange(GetFramesFromAtlas("policeman_walking_left_3.png", 1, 1));
             AnimationFrames["policeman_walking_left"] = policeWalkLeft;
             
             var policeWalkDown = new List<Rectangle>();
             policeWalkDown.AddRange(GetFramesFromAtlas("policeman_walking_down_1.png", 1, 1));
             policeWalkDown.AddRange(GetFramesFromAtlas("policeman_walking_down_2.png", 1, 1));
             policeWalkDown.AddRange(GetFramesFromAtlas("policeman_walking_down_3.png", 1, 1));
             AnimationFrames["policeman_walking_down"] = policeWalkDown;
             
             var policeWalkUp = new List<Rectangle>();
             policeWalkUp.AddRange(GetFramesFromAtlas("policeman_walking_up_1.png", 1, 1));
             policeWalkUp.AddRange(GetFramesFromAtlas("policeman_walking_up_2.png", 1, 1));
             policeWalkUp.AddRange(GetFramesFromAtlas("policeman_walking_up_3.png", 1, 1));
             AnimationFrames["policeman_walking_up"] = policeWalkUp;
             
             var policeAttackRight = new List<Rectangle>();
             policeAttackRight.AddRange(GetFramesFromAtlas("policeman_attack_right_1.png", 1, 1));
             policeAttackRight.AddRange(GetFramesFromAtlas("policeman_attack_right_2.png", 1, 1));
             policeAttackRight.AddRange(GetFramesFromAtlas("policeman_attack_right_3.png", 1, 1));
             policeAttackRight.AddRange(GetFramesFromAtlas("policeman_attack_right_4.png", 1, 1));
             policeAttackRight.AddRange(GetFramesFromAtlas("policeman_attack_right_5.png", 1, 1));
             AnimationFrames["policeman_attack_right"] = policeAttackRight;
             
             var policeAttackLeft = new List<Rectangle>();
             policeAttackLeft.AddRange(GetFramesFromAtlas("policeman_attack_left_1.png", 1, 1));
             policeAttackLeft.AddRange(GetFramesFromAtlas("policeman_attack_left_2.png", 1, 1));
             policeAttackLeft.AddRange(GetFramesFromAtlas("policeman_attack_left_3.png", 1, 1));
             policeAttackLeft.AddRange(GetFramesFromAtlas("policeman_attack_left_4.png", 1, 1));
             policeAttackLeft.AddRange(GetFramesFromAtlas("policeman_attack_left_5.png", 1, 1));
             AnimationFrames["policeman_attack_left"] = policeAttackLeft;
        }

        public static List<Rectangle> GetFrames(string key)
        {
            return AnimationFrames.TryGetValue(key, out var frames) ? frames : throw new KeyNotFoundException($"Key '{key}' not found.");
        }
    }
}