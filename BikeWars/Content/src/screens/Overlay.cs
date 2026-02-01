using Microsoft.Xna.Framework;
// klasse kan perspektivisch komplett entfernt werden, eigentlich bisschen überflüssig
namespace BikeWars.Content.src.screens.Overlay
{
    public class Overlay
    {
        private bool _isPaused = false;
        public bool IsPaused => _isPaused;

        public Overlay() {
        }

        public void SetPaused(bool paused, GameTime gameTime)
        {
            _isPaused = paused;
        }
    }
}