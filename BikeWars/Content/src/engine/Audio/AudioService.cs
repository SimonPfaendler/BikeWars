using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace BikeWars.Content.engine.Audio
{
    public class AudioService
    {
        public SoundManager Sounds { get; }
        public MusicManager Music { get; }

        public AudioService()
        {
            Sounds = new SoundManager();
            Music = new MusicManager();
        }
        
        public void LoadContent(ContentManager content)
        {
            Sounds.Load(content, AudioAssets.SoundEffectPaths);
            //Music.Load(content, AudioAssets.SongPaths);
        }
        
        public void Update(GameTime gameTime)
        {
            Sounds.Update(gameTime);
            //Music.Update(gameTime);
        }
    }
}