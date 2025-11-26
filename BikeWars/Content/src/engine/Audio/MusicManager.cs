using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace BikeWars.Content.engine.Audio
{
    public class MusicManager
    {
        private readonly Dictionary<string, Song> _songs = new();
        private string _currentSongId = null;
        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 1f;
        
        // Load: used only once when starting the game: paths = ID -> content path
        public void Load(ContentManager content, Dictionary<string, string> paths)
        {
            _songs.Clear();
            foreach (var kv in paths)
            {
                try
                {
                    var song = content.Load<Song>(kv.Value);
                    _songs[kv.Key] = song;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MusicManager] Fehler beim Laden von {kv.Value}: {ex.Message}");
                }
            }

            MediaPlayer.Volume = Math.Clamp(MasterVolume * MusicVolume, 0f, 1f);
        }

        public void Play(string id, bool isRepeating = true)
        {
            if (!_songs.TryGetValue(id, out var song) || song == null) return;
            _currentSongId = id;
            MediaPlayer.IsRepeating = isRepeating;
            MediaPlayer.Volume = Math.Clamp(MasterVolume * MusicVolume, 0f, 1f);
            MediaPlayer.Play(song);
        }

        public void Stop()
        {
            MediaPlayer.Stop();
            _currentSongId = null;
        }

        public void Pause()
        {
            MediaPlayer.Pause();
        }

        public void Resume()
        {
            MediaPlayer.Resume();
        }

        public void Update(GameTime gameTime)
        {
            // keep volume in sync
            MediaPlayer.Volume = Math.Clamp(MasterVolume * MusicVolume, 0f, 1f);
        }
    }
}