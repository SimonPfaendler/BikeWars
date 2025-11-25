using System.Collections.Generic;

namespace BikeWars.Content.engine.Audio
{
    public static class AudioAssets
    {
        // Sound IDs to be used in the code
        public const string SoftClick = "button_soft_click";
        public const string HandgunClick = "button_handgun_click";
        public const string Walking = "player_walking";
        public const string Driving = "bike_driving";

        // Mapping ID -> Content-Path
        public static readonly Dictionary<string, string> SoundEffectPaths = new()
        {
            { SoftClick, "assets/sounds/SoftClick" },
            { HandgunClick, "assets/sounds/HandgunClick" },
            { Walking, "assets/sounds/Walking" },
            { Driving, "assets/sounds/Bike_Driving" }
        };

        // Song IDs to be used in the code
        // TODO: Add Main theme
        //public const string MainTheme = "music_main_theme";
        //public static readonly Dictionary<string, string> SongPaths = new()
        //{
         //   { MainTheme, "assets/music/MainTheme" }
            // weitere Songs hier
        //};
    }
}