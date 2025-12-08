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
        public const string GunShot = "gun_shot";
        public const string BulletHit = "bullet_hit";
        public const string Punch = "boxing_punch";
        public const string CarCrash = "car_crash";
        public const string Flamethrower = "flamethrower";
        public const string IceTrail = "ice_trail";

        // Mapping ID -> Content-Path
        public static readonly IReadOnlyDictionary<string, string> SoundEffectPaths = new Dictionary<string, string>
        {
            { SoftClick, "assets/sounds/SoftClick" },
            { HandgunClick, "assets/sounds/HandgunClick" },
            { Walking, "assets/sounds/Walking" },
            { Driving, "assets/sounds/Bike_Driving" },
            { GunShot, "assets/sounds/gun_shot" },
            { BulletHit, "assets/sounds/bullet_hit" },
            { Punch, "assets/sounds/boxing_punch" },
            { CarCrash, "assets/sounds/CarCrash"},
            { Flamethrower, "assets/sounds/flamethrower"},
            { IceTrail, "assets/sounds/ice_trail"}
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