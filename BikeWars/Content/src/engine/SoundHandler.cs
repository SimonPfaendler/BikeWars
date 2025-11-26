using System;
using System.Collections.Generic;
using BikeWars.Content.components;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace BikeWars.Content.engine
{
    public class SoundHandler
    {
        public String WALKING_SOUND_PATH = "assets/sounds/Walking";
        public String DRIVING_SOUND_PATH = "assets/sounds/Bike_Driving";
        
        public String SOFT_CLICK_PATH = "assets/sounds/SoftClick";
        public String HANDGUN_CLICK_PATH = "assets/sounds/HandgunClick";

		public String GAME_MUSIC_PATH = "assets/sounds/Game_music";
		public String MENU_MUSIC_PATH = "assets/sounds/Menu_music";
        
        public SoundEffectInstance WalkingSoundInstance { get; set; }
        public SoundEffectInstance DrivingSoundInstance { get; set; }
        private SoundEffectInstance _gameMusicInstance;
        private SoundEffectInstance _menuMusicInstance;
        
        private SoundEffect _softClickSound;
        private SoundEffect _handgunClickSound;
        private SoundEffect _gameMusic;
        private SoundEffect _menuMusic;
        
        public SoundHandler()
        {
            
        }
        
        public void LoadContent(ContentManager content)
        {
            _softClickSound = content.Load<SoundEffect>(SOFT_CLICK_PATH);
            _handgunClickSound = content.Load<SoundEffect>(HANDGUN_CLICK_PATH);
            _gameMusic = content.Load<SoundEffect>(GAME_MUSIC_PATH);
            _menuMusic = content.Load<SoundEffect>(MENU_MUSIC_PATH);
            // creating Instance for looping
            _gameMusicInstance = _gameMusic.CreateInstance();
            _gameMusicInstance.IsLooped = true;
            
            _menuMusicInstance = _menuMusic.CreateInstance();
            _menuMusicInstance.IsLooped = true;
        }
        
        public void PlaySoftClick()
        {
            _softClickSound?.Play();
        }
        
        public void PlayHandgunClick()
        {
            _handgunClickSound?.Play();
        }

        public void PlayGameMusic()
        {
            _menuMusicInstance?.Stop();
            if (_gameMusicInstance?.State != SoundState.Playing)
                _gameMusicInstance?.Play();
        }

        public void PlayMenuMusic()
        {
            _gameMusicInstance?.Stop();
            if (_menuMusicInstance?.State != SoundState.Playing)
                _menuMusicInstance?.Play();
        }

        public void PlayButtonClick(ButtonAction buttonAction)
        {
            switch (buttonAction)
            {
                case ButtonAction.StartGame:
                    PlayHandgunClick();
                    break;
                default:
                    PlaySoftClick();
                    break;
            }
        }

        public void PlayGameMusic(bool GameScreenIsActive)
        {
            if (GameScreenIsActive)
            {
                PlayGameMusic();
            }

            else
            {
                PlayMenuMusic();
            }
        }

    }
}