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
        private readonly List<ManagedInstance> _activeInstances = new();

        // dictionary for looped instances (walking, driving, etc.)
        private readonly Dictionary<string, SoundEffectInstance> _loopInstances = new();

        public float MasterVolume { get; set; } = 1f;
        public float SfxVolume { get; set; } = 1f;
        
        private const int MaxTotalSounds = 15;
        private const int MaxPerType = 3;
        
        private int ActiveSoundCount =>
            _activeInstances.Count + _loopInstances.Count;

        // internal wrapper to store id + instance
        private class ManagedInstance
        {
            public string Id;
            public SoundEffectInstance Instance;

            public ManagedInstance(string id, SoundEffectInstance instance)
            {
                Id = id;
                Instance = instance;
            }
        }

        // Load: used only once when starting the game: paths = ID -> content path
        public void Load(ContentManager content, IReadOnlyDictionary<string, string> paths)
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
        
        private int CountInstancesOf(string id)
        {
            int count = _activeInstances.Count(i => i.Id == id);
            if (_loopInstances.ContainsKey(id))
                count++;
            return count;
        }

        private void EnforceLimits()
        {
            // While we already reached the global capacity, drop oldest one-shot instances (FIFO).
            while (ActiveSoundCount >= MaxTotalSounds)
            {
                if (_activeInstances.Count > 0)
                {
                    var oldest = _activeInstances[0];
                    try
                    {
                        oldest.Instance.Stop();
                        oldest.Instance.Dispose();
                    }
                    catch { }
                    _activeInstances.RemoveAt(0);
                }
                else
                {
                    // if there are no one-shot instances to drop, we can't free more (loop instances are kept)
                    break;
                }
            }
        }

        // one-shot Play
        public void Play(string id)
        {
            if (!_sfx.TryGetValue(id, out var sfx) || sfx == null)
                return;

            if (CountInstancesOf(id) >= MaxPerType)
                return;

            EnforceLimits();

            var inst = sfx.CreateInstance();
            inst.Volume = Math.Clamp(MasterVolume * SfxVolume, 0f, 1f);
            inst.IsLooped = false;
            inst.Play();

            _activeInstances.Add(new ManagedInstance(id, inst));
        }

        // Looping sound
        public void PlayLoop(string id)
        {
            if (!_sfx.TryGetValue(id, out var sfx) || sfx == null)
                return;
            
            if (CountInstancesOf(id) >= MaxPerType)
                return;

            EnforceLimits();

            if (_loopInstances.TryGetValue(id, out var existing))
            {
                if (existing.State == SoundState.Stopped)
                {
                    try { existing.Dispose(); } catch { }
                    _loopInstances.Remove(id);
                }
                else
                    return; // already playing
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
                try { inst.Stop(); inst.Dispose(); } catch { }
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
            foreach (var managed in _activeInstances)
            {
                try
                {
                    if (managed.Instance.State == SoundState.Playing)
                        managed.Instance.Pause();
                }
                catch { }
            }

            // Loop sounds
            foreach (var inst in _loopInstances.Values)
            {
                try
                {
                    if (inst.State == SoundState.Playing)
                        inst.Pause();
                }
                catch { }
            }
        }
        
        public void ResumeAll()
        {
            // One-shot sounds
            foreach (var managed in _activeInstances)
            {
                try
                {
                    if (managed.Instance.State == SoundState.Paused)
                        managed.Instance.Resume();
                }
                catch { }
            }

            // Loop sounds
            foreach (var inst in _loopInstances.Values)
            {
                try
                {
                    if (inst.State == SoundState.Paused)
                        inst.Resume();
                }
                catch { }
            }
        }

        public void Update(GameTime gameTime)
        {
            // remove stopped one-shot sounds
            _activeInstances.RemoveAll(mi =>
            {
                try
                {
                    if (mi.Instance.State == SoundState.Stopped)
                    {
                        mi.Instance.Dispose();
                        return true;
                    }
                }
                catch { }
                return false;
            });

            // Update Volumes and cleanup loop instances
            foreach (var kv in _loopInstances.ToList())
            {
                var inst = kv.Value;
                if (inst.State == SoundState.Stopped)
                {
                    try { inst.Dispose(); } catch { }
                    _loopInstances.Remove(kv.Key);
                }
                else
                {
                    try { inst.Volume = Math.Clamp(MasterVolume * SfxVolume, 0f, 1f); } catch { }
                }
            }
        }
        
        public void StopAll()
        {
            // Stop & dispose one-shot instances
            foreach (var managed in _activeInstances)
            {
                try { managed.Instance.Stop(); managed.Instance.Dispose(); } catch { }
            }
            _activeInstances.Clear();

            // Stop & dispose loop instances
            foreach (var kv in _loopInstances.ToList())
            {
                try { kv.Value.Stop(); kv.Value.Dispose(); } catch { }
            }
            _loopInstances.Clear();
        }
    }
}
