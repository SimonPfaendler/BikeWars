using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using BikeWars.Content.utils;

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
            { "Frelo", "assets/images/Frelo" },
            { "RacingBike", "assets/images/RacingBike" },
            { "XP_Beer", "assets/sprites/XP/xp_beer_texture" },
            { "XP_Money", "assets/sprites/XP/xp_money_texture" },
            { "EnergyGel", "assets/images/EnergyGel" },
            //MAP OBJECTS
            { "Fahrradwerkstatt", "assets/Map/Fahrradwerkstatt_Tile"}
        };
        // hier sollte irgendwann auch auf atlas strukturen gewechselt werden

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

            //DOG
            "Dog_Idle",
            "Dog_WalkLeft",
            "Dog_WalkRight",
            "Dog_WalkDown",
            "Dog_WalkUp",
        };

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

                float speed = 0.15f;

                // "ausnahmen", kann man später evtl abändern um die zyklomatische komplexität zu verringern
                if (key.Contains("Hobo_Idle"))
                {
                    speed = 0.4f;
                }
                if (key.Contains("Character1"))
                {
                    speed = 0.16f;
                }

                if (key.Contains("Dog_Idle"))
                {
                    speed = 0.5f;
                }

                var animation = new SpriteAnimation(_characterAtlas, frames, speed);
                _animationCache.Add(key, animation);
            }

            foreach (var kv in SimpleTexturePaths)
            {
                var texture = content.Load<Texture2D>(kv.Value);
                _textureCache.Add(kv.Key, texture);
            }
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

    //  abspielen Sprite-Animation
    public class SpriteAnimation
    {
        private readonly Texture2D _sheet;
        private readonly List<Rectangle> _frames;
        private readonly float _secondsPerFrame;
        private int _frameIndex;
        private float _timer;

        public SpriteAnimation(Texture2D sheet, List<Rectangle> frames, float secondsPerFrame)
        {
            _sheet = sheet;
            _frames = frames;
            _secondsPerFrame = secondsPerFrame;
            _frameIndex = 0;
            _timer = 0f;
        }

        public void Update(GameTime gameTime, bool isMoving)
        {
            if (isMoving)
            {
                _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_timer >= _secondsPerFrame)
                {
                    _timer -= _secondsPerFrame;
                    _frameIndex = (_frameIndex + 1) % _frames.Count;
                }
            }
            else
            {
                _frameIndex = 0;
                _timer = 0f;
            }
        }

        public Rectangle GetCurrentFrame()
        {
            return _frames[_frameIndex];
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Point size, float rotation, Color? color = null)
        {
            Rectangle source = _frames[_frameIndex];
            Rectangle dest = new Rectangle(
                (int)MathF.Round(position.X),
                (int)MathF.Round(position.Y),
                size.X,
                size.Y
            );
            spriteBatch.Draw(_sheet, dest, source, color ?? Color.White, rotation: rotation, new Vector2(source.Width / 2f, source.Height / 2f), SpriteEffects.None, layerDepth:0f);
        }

        public SpriteAnimation Clone()
        {
            return new SpriteAnimation(_sheet, _frames, _secondsPerFrame);
        }
    }
}