using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.engine.Audio;
public class AudioService
{
    public SoundManager Sounds { get; }
    public MusicManager Music { get; }

    // Speichert, welche Sounds schon geladen wurden
    private HashSet<string> _loadedSounds = new HashSet<string>();
    private HashSet<string> _loadedMusic = new HashSet<string>();

    private ContentManager _content;
    public AudioService()
    {
        Sounds = new SoundManager();
        Music = new MusicManager();
    }

    public void LoadContent(ContentManager content)
    {
        _content = content;
        // Sounds.Load(content, AudioAssets.SoundEffectPaths);
        // Music.Load(content, AudioAssets.SongPaths);
    }

    public void PlaySound(string soundName)
    {
        if (!_loadedSounds.Contains(soundName))
        {
            var soundDict = new Dictionary<string, string> { { soundName, AudioAssets.SoundEffectPaths[soundName] } };

            Sounds.Load(_content, soundDict);
            _loadedSounds.Add(soundName);
        }

        Sounds.Play(soundName);
    }


    public void PlayMusic(string musicName, bool loop = true)
    {
        if (!_loadedMusic.Contains(musicName))
        {
            // Baue ein Dictionary mit nur diesem Song
            var songDict = new Dictionary<string, string> { { musicName, AudioAssets.SongPaths[musicName] } };

            Music.Load(_content, songDict);
            _loadedMusic.Add(musicName);
        }

        Music.Play(musicName, loop);
    }

    public void StopAll()
    {
        Sounds.StopAll();
        Music.Stop();

        // Give memory free
        // Sounds.UnloadAll();
        Music.UnloadAll();

        _loadedSounds.Clear();
        _loadedMusic.Clear();
    }

    public void Update(GameTime gameTime)
    {
        Sounds.Update(gameTime);
        Music.Update(gameTime);
    }
}