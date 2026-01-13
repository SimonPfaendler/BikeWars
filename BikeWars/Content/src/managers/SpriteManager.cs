using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using BikeWars.Content.utils;
using BikeWars.Content.engine;

namespace BikeWars.Content.managers
{
    /// <summary>
    /// Verwaltet die Erstellung/Bereitstellung aller SpriteAnimation-Objekte für die Spielentitäten.
    /// - zentralisiert das Laden des Texture Atlas
    /// - Instanziierung der vollständigen Animationsobjekte
    /// - ruft die gespeicherten Frame-Koordinaten ab
    /// - nutzt ram cache
    /// </summary>
    public static class SpriteManager
    {
        // caching_construct, einmalig laden
        private static Texture2D _characterAtlas;
        // caching_construct, speichert fertig geladene animationen
        private static Dictionary<string, SpriteAnimation> _animationCache;
        // für nicht animierte Sprites: Kugel, Geld, etc.
        private static Dictionary<string, Texture2D> _textureCache;

        // Data-driven animation speeds
        private static readonly Dictionary<string, float> AnimationSpeeds = new Dictionary<string, float>
        {
            { "Hobo_Idle", 0.4f },
            { "Dog_Idle", 0.5f },
            { "KamikazeOpa_Death", 0.1f }
        };

        private const float DefaultSpeed = 0.15f;
        private const float Character1Speed = 0.16f;

        // Stellt gecachten Character Atlas zur verfügung für ghosttrail unteranderem
        public static Texture2D GetCharacterAtlas()
        {
            if (_characterAtlas == null)
            {
                throw new InvalidOperationException("character atlas is null");
            }
            return _characterAtlas;
        }

        // nicht animierte textures
        private static readonly IReadOnlyDictionary<string, string> SimpleTexturePaths = new Dictionary<string, string>
        {
            // PROJEKTILE
            { "Bullet", "assets/sprites/projectiles/bullet" },


            // HUD
            { "HUD_Sheet", "assets/sprites/HUD/Hud_sheet2" },

            // ITEMS
            { "Chest", "assets/sprites/chest_texture" },
            {"Chest_open", "assets/sprites/chest_open_texture"},
            { "Frelo", "assets/images/Frelo" },
            { "RacingBike", "assets/images/RacingBike" },
            { "XP_Beer", "assets/sprites/XP/xp_beer_texture" },
            { "XP_Money", "assets/sprites/XP/xp_money_texture" },
            { "EnergyGel", "assets/images/EnergyGel" },
            { "DogFood", "assets/images/DogFood" },
            // TRAM
            { "Tram", "assets/sprites/Tram_final" },
            //MAP OBJECTS
            { "Fahrradwerkstatt", "assets/MapObjects/Fahrradwerkstatt_Tile"},
            { "Dog_Bowl", "assets/MapObjects/Dog_Bowl"},
            { "Dog_Bowl_full", "assets/MapObjects/Dog_Bowl_full"}
        };

        // No single-image map sprites here; map sprites are loaded from atlas JSON.

        // Atlas regions for large map atlases (tilemap_1 etc.)
        private static Dictionary<string, Rectangle> _mapAtlasEntries = new Dictionary<string, Rectangle>();
        private static Texture2D _mapAtlasTexture;

        // liste aller animationen, die beim start gecached werden
        private static readonly List<string> AnimationKeys = new List<string>
        {
            // SPIELER 1
            "Character1_BikeDown",
            "Character1_BikeLeft",
            "Character1_BikeRight",
            "Character1_BikeUp",
            "Character1_WalkDown",
            "Character1_WalkLeft",
            "Character1_WalkRight",
            "Character1_WalkUp",
            "Character1_Idle",

            // SPIELER 2
            "Character2_BikeUp",
            "Character2_WalkDown",
            "Character2_WalkLeft",
            "Character2_WalkRight",
            "Character2_WalkUp",
            "Character2_Idle",

            // HOBO
            "Hobo_Idle",
            "Hobo_WalkLeft",
            "Hobo_WalkRight",
            "Hobo_WalkDown",
            "Hobo_WalkUp",

            // BIKE THIEF
            "BikeThief_Idle",
            "BikeThief_WalkLeft",
            "BikeThief_WalkRight",
            
            // DOG
            "Dog_Idle",
            "Dog_WalkLeft",
            "Dog_WalkRight",
            "Dog_WalkDown",
            "Dog_WalkUp",

            // KAMIKAZE OPA
            "KamikazeOpa_BikeDown",
            "KamikazeOpa_BikeLeft",
            "KamikazeOpa_BikeRight",
            "KamikazeOpa_BikeUp",
            "KamikazeOpa_Death",
        };

        private static float GetAnimationSpeed(string key)
        {
            if (AnimationSpeeds.TryGetValue(key, out float speed))
            {
                return speed;
            }
            if (key.StartsWith("Character1"))
            {
                return Character1Speed;
            }
            return DefaultSpeed;
        }

        /// <summary>
        /// Lädt Texture Atlas einmalig, caching
        /// </summary>
        public static void LoadContent(ContentManager content)
        {
            if (_animationCache != null)
            {
                return;
            }

            _characterAtlas = content.Load<Texture2D>("assets/sprites/characters/character_atlas");

            _animationCache = new Dictionary<string, SpriteAnimation>();

            _textureCache = new Dictionary<string, Texture2D>();

            // animationen laden und cachen
            foreach (var key in AnimationKeys)
            {
                var frames = SpriteFrameDictionary.GetFrames(key);
                float speed = GetAnimationSpeed(key);
                var animation = new SpriteAnimation(_characterAtlas, frames, speed);
                _animationCache.Add(key, animation);
            }

            foreach (var kv in SimpleTexturePaths)
            {
                var texture = content.Load<Texture2D>(kv.Value);
                _textureCache.Add(kv.Key, texture);
            }
            // Note: single-image map sprites were removed — map objects use atlas regions now.

            // Try to load map atlas JSON (tilemap_1_regions.json) — prefer Content/sprites, fallback to Content/assets/sprites
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string candidate1 = System.IO.Path.Combine(baseDir, "Content", "sprites", "tilemap_1_regions");
                string path = System.IO.File.Exists(candidate1) ? candidate1 : candidate1 + ".json";

                if (!System.IO.File.Exists(path))
                {
                    string candidate2 = System.IO.Path.Combine(baseDir, "Content", "assets", "sprites", "tilemap_1_regions");
                    path = System.IO.File.Exists(candidate2) ? candidate2 : candidate2 + ".json";
                }

                if (System.IO.File.Exists(path))
                {
                    var root = System.Text.Json.JsonSerializer.Deserialize<MapAtlasRoot>(System.IO.File.ReadAllText(path), new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (root?.frames != null && root.meta?.image != null)
                    {
                        // load atlas texture by name (strip extension)
                        string imageName = System.IO.Path.GetFileNameWithoutExtension(root.meta.image);
                        try
                        {
                            _mapAtlasTexture = content.Load<Texture2D>("assets/Map/" + imageName);
                        }
                        catch { _mapAtlasTexture = null; }

                        foreach (var f in root.frames)
                        {
                            if (f.filename != null && f.frame != null)
                            {
                                _mapAtlasEntries[f.filename] = new Rectangle(f.frame.x, f.frame.y, f.frame.w, f.frame.h);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Try to retrieve an atlas region from the loaded map atlas.
        /// Returns the atlas Texture2D (if loaded) and the source rectangle.
        /// </summary>
        public static bool TryGetMapAtlasRegion(string filename, out Texture2D atlas, out Rectangle rect)
        {
            atlas = null;
            rect = Rectangle.Empty;
            if (_mapAtlasTexture == null) return false;
            if (_mapAtlasEntries.TryGetValue(filename, out rect))
            {
                atlas = _mapAtlasTexture;
                return true;
            }
            return false;
        }

        /// <summary>
        /// SpriteAnimation aus Cache
        /// </summary>
        public static SpriteAnimation GetAnimation(string name)
        {
            if (_animationCache == null)
            {
                throw new InvalidOperationException("animation cache is null");
            }

            if (_animationCache.TryGetValue(name, out var animation))
            {
                return animation.Clone();
            }

            throw new KeyNotFoundException("animation '" + name + "' not found");
        }

        public static Texture2D GetTexture(string name)
        {
            if (_textureCache == null)
            {
                throw new InvalidOperationException("texture cache is null");
            }

            if (_textureCache.TryGetValue(name, out var texture))
            {
                return texture;
            }

            throw new KeyNotFoundException("texture '" + name + "' not found");
        }
    }
}

    // Structures for map atlas JSON deserialization
    internal class MapAtlasRoot { public List<MapAtlasFrame> frames { get; set; } public MapAtlasMeta meta { get; set; } }
    internal class MapAtlasFrame { public string filename { get; set; } public MapAtlasRect frame { get; set; } }
    internal class MapAtlasMeta { public string image { get; set; } }
    internal class MapAtlasRect { public int x { get; set; } public int y { get; set; } public int w { get; set; } public int h { get; set; } }