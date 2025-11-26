using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.engine.Audio
{
    public class SoundManager
    {
        private readonly Dictionary<string, SoundEffect> _sfx = new();
        private readonly List<SoundEffectInstance> _activeInstances = new();

        // dictionary for looped instances (walking, driving, etc.)
        private readonly Dictionary<string, SoundEffectInstance> _loopInstances = new();

        public float MasterVolume { get; set; } = 1f;
        public float SfxVolume { get; set; } = 1f;

        // Load: used only once when starting the game: paths = ID -> content path
        public void Load(ContentManager content, Dictionary<string, string> paths)
        {
            _sfx.Clear();
            foreach (var kv in paths)
            {
                try
                {
                    var effect = content.Load<SoundEffect>(kv.Value);
                    _sfx[kv.Key] = effect;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SoundManager] Fehler beim Laden von {kv.Value}: {ex.Message}");
                }
            }
        }

        // one-shot Play
        public void Play(string id)
        {
            if (!_sfx.TryGetValue(id, out var sfx) || sfx == null) return;

            var inst = sfx.CreateInstance();
            inst.Volume = Math.Clamp(MasterVolume * SfxVolume, 0f, 1f);
            inst.IsLooped = false;
            inst.Play();
            _activeInstances.Add(inst);
        }

        // Looping sound
        public void PlayLoop(string id)
        {
            if (!_sfx.TryGetValue(id, out var sfx) || sfx == null)
                return;

            if (_loopInstances.TryGetValue(id, out var existing))
            {
                if (existing.State == SoundState.Stopped)
                {
                    existing.Dispose();
                    _loopInstances.Remove(id);
                }
                else
                    return;
            }

            var inst = sfx.CreateInstance();
            inst.IsLooped = true;
            inst.Volume = Math.Clamp(MasterVolume * SfxVolume, 0f, 1f);
            inst.Play();
            _loopInstances[id] = inst;
        }

        
        public void StopLoop(string id)
        {
            if (_loopInstances.TryGetValue(id, out var inst))
            {
                inst.Stop();
                inst.Dispose();
                _loopInstances.Remove(id);
            }
        }

        
        public void PauseLoop(string id)
        {
            if (_loopInstances.TryGetValue(id, out var inst))
            {
                if (inst.State == SoundState.Playing)
                    inst.Pause();
            }
        }


        public void ResumeLoop(string id)
        {
            if (!_loopInstances.TryGetValue(id, out var inst))
            {
                PlayLoop(id);
                return;
            }

            if (inst.State == SoundState.Paused)
                inst.Resume();
        }
        
        public void PauseAll()
        {
            // One-shot sounds
            foreach (var inst in _activeInstances)
            {
                if (inst.State == SoundState.Playing)
                    inst.Pause();
            }

            // Loop sounds
            foreach (var inst in _loopInstances.Values)
            {
                if (inst.State == SoundState.Playing)
                    inst.Pause();
            }
        }
        
        public void ResumeAll()
        {
            // One-shot sounds
            foreach (var inst in _activeInstances)
            {
                if (inst.State == SoundState.Paused)
                    inst.Resume();
            }

            // Loop sounds
            foreach (var inst in _loopInstances.Values)
            {
                if (inst.State == SoundState.Paused)
                    inst.Resume();
            }
        }




        
        public void Update(GameTime gameTime)
        {
            // remove stopped one-shot sounds
            _activeInstances.RemoveAll(i =>
            {
                if (i.State == SoundState.Stopped)
                {
                    i.Dispose();
                    return true;
                }
                return false;
            });

            // Update Volumes for Loop-Instruments
            foreach (var kv in _loopInstances.ToList())
            {
                var inst = kv.Value;
                if (inst.State == SoundState.Stopped)
                {
                    inst.Dispose();
                    _loopInstances.Remove(kv.Key);
                }
                else
                {
                    inst.Volume = Math.Clamp(MasterVolume * SfxVolume, 0f, 1f);
                }
            }
        }
        
        public void StopAll()
        {
            foreach (var inst in _activeInstances)
            {
                try { inst.Stop(); inst.Dispose(); } catch { }
            }
            _activeInstances.Clear();

            foreach (var kv in _loopInstances)
            {
                try { kv.Value.Stop(); kv.Value.Dispose(); } catch { }
            }
            _loopInstances.Clear();
        }
    }
}
