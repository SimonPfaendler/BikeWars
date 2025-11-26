using System;
using System.Collections.Generic;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.screens;
using Microsoft.Xna.Framework;
using BikeWars.Content.engine.Audio;
// The screen manager handles all existing screens:
// -> decides which screen are drawn
// -> decide which screens are getting updated
namespace BikeWars.Content.managers
{
    public class ScreenManager
    {
        // The Screen Stack contains all Screens that are in use at the moment
        // The manager handles Draw and Update for them using the stack
        private List<IScreen> _mScreenStack = new List<IScreen>();
        
        public event Action<IScreen> OnScreenAdded;
        public event Action<IScreen> OnScreenRemoved; 
        public event Action OnReturnToMainMenu;
        
        private AudioService _audio;
        private string _currentMusic;
        private float _currentVolume = -1f;

        public void AddScreen(IScreen screen)
        {
            screen.ScreenManager = this;
            _mScreenStack.Add(screen);
            OnScreenAdded?.Invoke(screen);
        }

        public void RemoveScreen(IScreen screen)
        {
            _mScreenStack.Remove(screen);
            OnScreenRemoved?.Invoke(screen);
        }

        public void ReturnToMainMenu()
        {
            _mScreenStack.Clear();
            OnReturnToMainMenu?.Invoke();
        }
        
        // used for the Game/ Menu music
        public bool GameScreenIsActive()
        {
            if (_mScreenStack.Count == 0)
                return false;

            return _mScreenStack[_mScreenStack.Count - 1] is GameScreen;
        }
        
        private void UpdateMusic()
        {
            if (_audio == null || _mScreenStack.Count == 0)
                return;

            var top = _mScreenStack[_mScreenStack.Count - 1];
            
            if (Math.Abs(_currentVolume - top.MusicVolume) > 0.01f)
            {
                _audio.Music.MusicVolume = top.MusicVolume;
                _currentVolume = top.MusicVolume;
            }
            
            if (top.DesiredMusic == null)
                return;
            
            if (_currentMusic != top.DesiredMusic)
            {
                _audio.Music.Play(top.DesiredMusic);
                _currentMusic = top.DesiredMusic;
            }
        }


        public void Draw(GameTime gameTime)
        {
            int lowestScreenToBeDrawn = _mScreenStack.Count - 1;
            for (int i = _mScreenStack.Count - 1; i >= 0; i--)
            {
                IScreen screen = _mScreenStack[i];
                if (screen.DrawLower)
                {
                    lowestScreenToBeDrawn = i - 1;
                }
                else
                {
                    break;
                }
            }
            
            for (int i = lowestScreenToBeDrawn; i < _mScreenStack.Count; i++)
            {
                IScreen screen = _mScreenStack[i];
                screen.Draw(gameTime);
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int i = _mScreenStack.Count - 1; i >= 0; i--)
            {
                IScreen screen = _mScreenStack[i];
                screen.Update(gameTime);
                if (!screen.UpdateLower)
                {
                    break;
                }
            }
            UpdateMusic();
        }
        public void SetAudio(AudioService audio)
        {
            _audio = audio;
        }

    }
}