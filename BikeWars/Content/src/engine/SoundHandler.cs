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
        
        public SoundEffectInstance WalkingSoundInstance { get; set; }
        public SoundEffectInstance DrivingSoundInstance { get; set; }
        
        private SoundEffect _softClickSound;
        private SoundEffect _handgunClickSound;
        
        public SoundHandler()
        {
            
        }
        
        public void LoadContent(ContentManager content)
        {
            _softClickSound = content.Load<SoundEffect>(SOFT_CLICK_PATH);
            _handgunClickSound = content.Load<SoundEffect>(HANDGUN_CLICK_PATH);
        }
        
        public void PlaySoftClick()
        {
            _softClickSound?.Play();
        }
        
        public void PlayHandgunClick()
        {
            _handgunClickSound?.Play();
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

    }
}