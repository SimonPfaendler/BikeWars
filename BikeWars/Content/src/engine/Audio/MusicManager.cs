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
        public float MasterVolume { get; set; } = 0.25f;
        public float MusicVolume { get; set; } = 0.5f;
        
        public bool IsPlaying => MediaPlayer.State == MediaState.Playing;

        public string CurrentSong => _currentSongId;

        
        // Load: used only once when starting the game: paths = ID -> content path
        public void Load(ContentManager content, IReadOnlyDictionary<string, string> paths)
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
            if (!_songs.TryGetValue(id, out var song) || song == null)
                return;

            if (_currentSongId == id && MediaPlayer.State == MediaState.Playing)
                return;

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
            if (MediaPlayer.State == MediaState.Playing)
                MediaPlayer.Pause();
        }


        public void Resume()
        {
            if (MediaPlayer.State == MediaState.Paused)
                MediaPlayer.Resume();
        }


        public void Update(GameTime gameTime)
        {
            // keep volume in sync
            MediaPlayer.Volume = Math.Clamp(MasterVolume * MusicVolume, 0f, 1f);
        }
    }
}