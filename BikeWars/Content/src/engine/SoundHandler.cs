using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace BikeWars.Content.engine
{
    public class SoundHandler
    {
        public String WALKING_SOUND_PATH = "assets/sounds/Walking";
        public SoundEffectInstance WalkingSoundInstance { get; set; }
        public SoundHandler()
        {
            
        }
    }
}
