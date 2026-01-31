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
        public const string BaechleSplash = "Baechle_Splash";
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
        public const string BarkMadita = "bark_madita";
        public const string Miau = "miau";
        public const string WoodCrack = "wood_crack";
        public const string WoodDestroy = "wood_destroy";
        public const string RaverSound = "raver_audio";
        public const string ThrowObject = "throw_object";
        public const string Geisterfahrer = "geisterfahrer";
        public const string HaltStop = "halt_stop";
        public const string BikeThiefHit1 = "bike_thief_hit1";
        public const string BikeThiefHit2 = "bike_thief_hit2";
        public const string BikeThiefTalk =  "bike_thief_talk";
        public const string BikeThiefLaugh = "bike_thief_laugh";
        public const string DogHit1 = "dog_hit1";
        public const string DogHit2 = "dog_hit2";
        public const string OpaShout = "opa_shout";
        public const string Explosion = "explosion";
        public const string DozentTalk_1 = "dozent_talk_1";
        public const string DozentTalk_2 = "dozent_talk_2";
        public const string DozentTalk_3 = "dozent_talk_3";
        public const string DozentHit = "dozent_hit";
        public const string PolizistHit = "polizist_hit";
        public const string Jaegermeister = "jaegermeister";
        public const string HoboTalk1 = "hobo_talk_1";
        public const string HoboTalk2 = "hobo_talk_2";
        public const string HoboHit1 = "hobo_hit_1";
        public const string HoboHit2 = "hobo_hit_2";
        
        

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
            { BaechleSplash, "assets/sounds/Baechle_Splash" },
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
            { BarkMadita, "assets/sounds/Dogs/Bark_Madita"},
            { WoodCrack, "assets/sounds/wood_crack"},
            { WoodDestroy, "assets/sounds/wood_destroy"},
            { Miau, "assets/sounds/Dogs/Miau"},
            { RaverSound, "assets/sounds/raver_audio" },
            { ThrowObject, "assets/sounds/throw_object" },
            { Geisterfahrer, "assets/sounds/Geisterfahrer" },
            { HaltStop, "assets/sounds/HaltStop" },
            { BikeThiefHit1, "assets/sounds/BikeThiefHit1" },
            { BikeThiefHit2, "assets/sounds/BikeThiefHit2" },
            { BikeThiefTalk, "assets/sounds/BikeThiefTalk" },
            { BikeThiefLaugh, "assets/sounds/BikeThiefLaugh" },
            { DogHit1, "assets/sounds/DogHit1" },
            { DogHit2, "assets/sounds/DogHit2" },
            { OpaShout, "assets/sounds/OpaShout" },
            { Explosion, "assets/sounds/Explosion"},
            { DozentTalk_1, "assets/sounds/DozentTalk_1" },
            { DozentTalk_2, "assets/sounds/DozentTalk_2" },
            { DozentTalk_3, "assets/sounds/DozentTalk_3" },
            { DozentHit, "assets/sounds/DozentHit" },
            { PolizistHit, "assets/sounds/PolizistHit" },
            { Jaegermeister, "assets/sounds/Jaegermeister" },
            { HoboTalk1, "assets/sounds/HoboTalk1"},
            { HoboTalk2, "assets/sounds/HoboTalk2"},
            { HoboHit1, "assets/sounds/HoboHit1"},
            { HoboHit2, "assets/sounds/HoboHit2"}
            
        };

        // Song IDs to be used in the code
        public const string MenuMusic = "Menu_music";
        public const string GameMusic = "Game_music";
        public const string LatinMusic = "Latin_music";
        public const string Metal = "Metal";
        public const string SCHymne = "SCHymne";
        
        public static readonly IReadOnlyDictionary<string, string> SongPaths = new Dictionary<string, string>
        {
            { MenuMusic, "assets/sounds/Menu_music" },
            { GameMusic, "assets/sounds/Game_music" },
            { LatinMusic, "assets/sounds/LatinMusic"},
            { Metal, "assets/sounds/Metal" },
            { SCHymne, "assets/sounds/SCHymne" },
        };
    }
}