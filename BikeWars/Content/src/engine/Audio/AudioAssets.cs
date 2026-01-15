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
        public const string CarHorn = "car_horn";
        public const string TrainHorn = "train_horn";
        public const string TrainHit = "TrainHitSlime";
        public const string ShortPain = "short_pain";
        public const string Slurp = "slurp";
        public const string Relief = "relief";
        public const string DamageCircle = "damage_circle";
        public const string VinylStop = "vinyl_stop";
        public const string BarkBora = "bark_bora";
        public const string BarkClemens = "bark_clemens";
        public const string BarkCarlota = "bark_carlota";
        public const string BarkGiulla = "bark_giulla";
        public const string BarkSimon = "bark_simon";
        public const string BarkSimon2 = "bark_simon_2";
        public const string BarkFritz = "bark_fritz";
        public const string Miau = "miau";
        

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
            { IceTrail, "assets/sounds/ice_trail"},
            { CarHorn, "assets/sounds/CarHorn"},
            { TrainHorn, "assets/sounds/train-horn"},
            { TrainHit, "assets/sounds/TrainHitSlime" },
            { ShortPain, "assets/sounds/short_pain"},
            { Slurp, "assets/sounds/Slurp"},
            { Relief, "assets/sounds/Relief"},
            { DamageCircle, "assets/sounds/damage_circle"},
            { VinylStop, "assets/sounds/VinylStop"},
            { BarkBora, "assets/sounds/Dogs/Bark_Bora"},
            { BarkClemens, "assets/sounds/Dogs/Bark_Clemens"},
            { BarkCarlota, "assets/sounds/Dogs/Bark_Carlota"},
            { BarkGiulla, "assets/sounds/Dogs/Bark_Giulla"},
            { BarkSimon, "assets/sounds/vBark_Simon"},
            { BarkSimon2, "assets/sounds/Dogs/Bark_Simon_2"},
            { BarkFritz, "assets/sounds/Dogs/Bark_Fritz"},
            { Miau, "assets/sounds/Dogs/Miau"},
            
        };

        // Song IDs to be used in the code
        public const string MenuMusic = "Menu_music";
        public const string GameMusic = "Game_music";
        public const string LatinMusic = "Latin_music";
        public const string Metal = "Metal";
        public static readonly IReadOnlyDictionary<string, string> SongPaths = new Dictionary<string, string>
        {
            { MenuMusic, "assets/sounds/Menu_music" },
            { GameMusic, "assets/sounds/Game_music" },
            { LatinMusic, "assets/sounds/LatinMusic"},
            { Metal, "assets/sounds/Metal" },

        };
    }
}