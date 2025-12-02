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
        public const string CarCrash = "car_crash";

        // Mapping ID -> Content-Path
        public static readonly IReadOnlyDictionary<string, string> SoundEffectPaths = new Dictionary<string, string>
        {
            { SoftClick, "assets/sounds/SoftClick" },
            { HandgunClick, "assets/sounds/HandgunClick" },
            { Walking, "assets/sounds/Walking" },
            { Driving, "assets/sounds/Bike_Driving" },
            {CarCrash, "assets/sounds/CarCrash"}
        };

        // Song IDs to be used in the code
        public const string MenuMusic = "Menu_music";
        public const string GameMusic = "Game_music";
        public static readonly IReadOnlyDictionary<string, string> SongPaths = new Dictionary<string, string>
        {
            { MenuMusic, "assets/sounds/Menu_music" },
            { GameMusic, "assets/sounds/Game_music" },

        };
    }
}