using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System; 

namespace BikeWars.Content.utils
{
    /// <summary>
    /// Stellt ein statisches Dictionary bereit, das alle Rectangle-Koordinaten 
    /// für die Animationen auf dem Character Atlas beinhaltet
    /// </summary>
    public static class SpriteFrameDictionary
    {
       
        // ZENTRALES DICTIONARY
        
        public static readonly Dictionary<string, List<Rectangle>> AnimationFrames = 
            new Dictionary<string, List<Rectangle>>()
        {
            
            // CHARACTER 1 (Spielfigur 1)
            
            // Character1_WalkDown: c1_move_down_1x2.png 
            { "Character1_WalkDown", new List<Rectangle> 
                {
                    new Rectangle(270, 0, 64, 64),
                    new Rectangle(270, 64, 64, 64)
                } 
            },
            
            // Character1_WalkLeft: c1_move_left_1x2.png
            { "Character1_WalkLeft", new List<Rectangle> 
                { 
                    new Rectangle(334, 0, 64, 64),
                    new Rectangle(334, 64, 64, 64)
                } 
            },
            
            // Character1_WalkRight: c1_move_right_1x2.png 
            { "Character1_WalkRight", new List<Rectangle> 
                {
                    new Rectangle(398, 0, 64, 64),
                    new Rectangle(398, 64, 64, 64)
                } 
            },
            
            // Character1_WalkUp: c1_move_up_1x2.png 
            { "Character1_WalkUp", new List<Rectangle> 
                {
                    new Rectangle(0, 128, 64, 64),
                    new Rectangle(0, 192, 64, 64)
                } 
            },
            
            // HOBO (Enemy 1)
            
            // Hobo_Idle: e1_drunkdude_standing.png 
            { "Hobo_Idle", new List<Rectangle>
                {
                    new Rectangle(0, 0, 40, 50) 
                }
            },

            // Hobo_WalkLeft: e1_drunkdude_walking_left.png 
            { "Hobo_WalkLeft", new List<Rectangle>
                {
                    new Rectangle(64, 128, 48, 63), 
                    new Rectangle(112, 128, 48, 63) 
                }
            },
            
            // Hobo_WalkRight: e1_drunkdude_walking_right.png 
            { "Hobo_WalkRight", new List<Rectangle>
                {
                    new Rectangle(190, 0, 40, 54), 
                    new Rectangle(230, 0, 40, 54)  
                }
            },
            
            // Hobo_WalkDown: e1_drunkdude_walking_down.png
            { "Hobo_WalkDown", new List<Rectangle> 
                {
                    // Reihe 1 
                    new Rectangle(0, 385, 51, 63),   // Frame 1
                    new Rectangle(51, 385, 51, 63),  // Frame 2
                    new Rectangle(102, 385, 51, 63), // Frame 3
        
                    // Reihe 2 
                    new Rectangle(0, 448, 51, 64),   // Frame 4 
                    new Rectangle(51, 448, 51, 64),  // Frame 5
                    new Rectangle(102, 448, 51, 64)  // Frame 6 
                }
            },

            //  Hobo_WalkUp: e1_drunkdude_walking_up.png
            { "Hobo_WalkUp", new List<Rectangle> 
                {
                    // Reihe 1 
                    new Rectangle(262, 128, 51, 63), // Frame 1 
                    new Rectangle(313, 128, 51, 63), // Frame 2 
        
                    // Reihe 2
                    new Rectangle(262, 191, 51, 64)  // Frame 3 
                }
            },
            
            // BIKE THIEF (Enemy 2)

            // BikeThief_Idle 
            { "BikeThief_Idle", new List<Rectangle> 
                {
                    new Rectangle(281, 385, 64, 61) 
                }
            },

            // BikeThief_WalkLeft: e2_bikethief_walking_left.png 
            { "BikeThief_WalkLeft", new List<Rectangle>
                {
                    // Zeile 0 (Y=385)
                    new Rectangle(153, 385, 64, 61), 
                    new Rectangle(217, 385, 64, 61), 
                    // Zeile 1 (Y=446)
                    new Rectangle(153, 446, 64, 61), 
                    new Rectangle(217, 446, 64, 61),
                    // Zeile 2 (Y=507)
                    new Rectangle(153, 507, 64, 62), 
                    new Rectangle(217, 507, 64, 62)
                }
            },

            // BikeThief_WalkRight: e2_bikethief_walking_right.png
            { "BikeThief_WalkRight", new List<Rectangle>
                {
                    // Zeile 0 (Y=385)
                    new Rectangle(281, 385, 64, 61), 
                    new Rectangle(345, 385, 64, 61), 
                    // Zeile 1 (Y=446)
                    new Rectangle(281, 446, 64, 61), 
                    new Rectangle(345, 446, 64, 61),
                    // Zeile 2 (Y=507)
                    new Rectangle(281, 507, 64, 62),
                    new Rectangle(345, 507, 64, 62)
                }
            },
            
            //DOG(enemy 3)
            // Dog_Idle: e3_dog_standing.png 
            { "Dog_Idle", new List<Rectangle>
                {
                    new Rectangle(372, 131, 42, 68) 
                }
            },

            // Dog_WalkLeft: e3_dog_walking_left.png 
            { "Dog_WalkLeft", new List<Rectangle>
                {
                    new Rectangle(326, 197, 55, 60), 
                    new Rectangle(392, 194, 55, 60) 
                }
            },
            
            // Dog_WalkRight: e3_dog_walking_right.png 
            { "Dog_WalkRight", new List<Rectangle>
                {
                    new Rectangle(78, 55, 58, 65), 
                    new Rectangle(4, 53, 58, 63)  
                }
            },
            
            // Hobo_WalkDown: e3_dog_walking_down.png
            { "Dog_WalkDown", new List<Rectangle> 
                {
                    new Rectangle(414, 129, 42, 74),
                    new Rectangle(372, 131, 42, 68) 
                }
            },

            //  Dog_WalkUp: e3_dog_walking_up.png
            { "Dog_WalkUp", new List<Rectangle> 
                {
                    new Rectangle(144, 54, 36, 60) 
                }
            },


            // Flamethrower_Attack: 10 Frames, 32x112 each, single row spritesheet
            {
                "Flamethrower_Attack", new List<Rectangle>
                {
                    new Rectangle(0 * 32, 0, 32, 112),   // Frame 1
                    new Rectangle(1 * 32, 0, 32, 112),   // Frame 2
                    new Rectangle(2 * 32, 0, 32, 112),   // Frame 3
                    new Rectangle(3 * 32, 0, 32, 112),   // Frame 4
                    new Rectangle(4 * 32, 0, 32, 112),   // Frame 5
                    new Rectangle(5 * 32, 0, 32, 112),   // Frame 6
                    new Rectangle(6 * 32, 0, 32, 112),   // Frame 7
                    new Rectangle(7 * 32, 0, 32, 112),   // Frame 8
                    new Rectangle(8 * 32, 0, 32, 112),   // Frame 9
                    new Rectangle(9 * 32, 0, 32, 112)    // Frame 10
                }
            },

            // IceTrail_Attack: 5 Frames, 64x60 each, single row spritesheet
            {
                "IceTrail_Attack", new List<Rectangle>
                {
                    new Rectangle(0 * 64, 0, 64, 60),   // Frame 1
                    new Rectangle(1 * 64, 0, 64, 60),   // Frame 2
                    new Rectangle(2 * 64, 0, 64, 60),   // Frame 3
                    new Rectangle(3 * 64, 0, 64, 60),   // Frame 4
                    new Rectangle(4 * 64, 0, 64, 60),   // Frame 5

                }
            },
        };
        
        // ZUGRUFFMETHODE
        
        /// <summary>
        /// Ruft die Liste der Rectangle Frames anhand eines Schlüssels ab.
        /// </summary>
        public static List<Rectangle> GetFrames(string key)
        {
            if (AnimationFrames.TryGetValue(key, out List<Rectangle> frames))
            {
                return frames;
            }
            throw new KeyNotFoundException($"Animation frames for key '{key}' not found in SpriteFrameDictionary.");
        }
    }
}