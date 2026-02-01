using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace BikeWars.Content.engine.Audio;
public class MusicManager
{
    private readonly Dictionary<string, Song> _songs = new();
    private string _currentSongId = null;
    public float MasterVolume { get; set; } = 0.0f;
    public float MusicVolume { get; set; } = 0.0f;

    public bool IsPlaying => MediaPlayer.State == MediaState.Playing;
    private float _fadeTargetVolume = 1f;
    private float _fadeCurrentVolume = 1f;
    private const float FADE_SPEED = 1.5f;
    private string _pendingSongId = null;
    private bool _isRepeatingPending = true;

    public string CurrentSong => _currentSongId;

    private readonly ContentManager _content;

    public MusicManager(ContentManager c)
    {
        _content = c;
    }

    // Load: used only once when starting the game: paths = ID -> content path
    public void Load(IReadOnlyDictionary<string, string> paths)
    {
        _songs.Clear();
        foreach (var kv in paths)
        {
            try
            {
                var song = _content.Load<Song>(kv.Value);
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
        if (!_songs.TryGetValue(id, out var song))
        {
            if (!AudioAssets.SongPaths.TryGetValue(id, out var path))
                return;

            try
            {
                song = _content.Load<Song>(path);
                _songs[id] = song;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MusicManager] Fehler beim Laden von {path}: {ex.Message}");
                return;
            }
        }

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

    public void Unload(string id)
    {
        if (_songs.TryGetValue(id, out var song))
        {
            song.Dispose();
            _songs.Remove(id);

            if (_currentSongId == id)
                _currentSongId = null;
        }
    }

    public void UnloadAll()
    {
        foreach (string id in _songs.Keys)
        {
            Unload(id);
        }
    }

    public void Resume()
    {
        if (MediaPlayer.State == MediaState.Paused)
            MediaPlayer.Resume();
    }

    public void Update(GameTime gameTime)
    {
        // keep volume in sync
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Fade if necessary
        if (Math.Abs(_fadeCurrentVolume - _fadeTargetVolume) > 0.01f)
        {
            if (_fadeCurrentVolume > _fadeTargetVolume)
                _fadeCurrentVolume -= FADE_SPEED * dt;
            else
                _fadeCurrentVolume += FADE_SPEED * dt;

            _fadeCurrentVolume = Math.Clamp(_fadeCurrentVolume, 0f, 1f);
        }
        // if Fade-Out done, change song and start Fade-In
        else if (_fadeTargetVolume == 0f && _pendingSongId != null)
        {
            Play(_pendingSongId, _isRepeatingPending);
            _pendingSongId = null;
            _fadeTargetVolume = 1f;
        }

        MediaPlayer.Volume = Math.Clamp(MasterVolume * MusicVolume * _fadeCurrentVolume, 0f, 1f);
    }
    public void PlayWithFade(string id, bool isRepeating = true)
    {
        if (_currentSongId == id) return;

        _pendingSongId = id;
        _isRepeatingPending = isRepeating;
        _fadeTargetVolume = 0f;
    }
}
