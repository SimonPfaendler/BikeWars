using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine.Audio
{
    public class WorldAudioManager
    {
        private Point _audibleAreaSize;
        private Vector2 _listenerPosition;

        public WorldAudioManager(Rectangle initialViewRect)
        {
            _audibleAreaSize = new Point(initialViewRect.Width, initialViewRect.Height);
        }

        public void UpdateListenerPosition(Vector2 position)
        {
            _listenerPosition = position;
        }

        public bool IsAudible(Vector2 soundSourcePosition)
        {
            int left = (int)(_listenerPosition.X - _audibleAreaSize.X / 2);
            int top = (int)(_listenerPosition.Y - _audibleAreaSize.Y / 2);

            Rectangle currentAudibleRect = new Rectangle(left, top, _audibleAreaSize.X, _audibleAreaSize.Y);

            return currentAudibleRect.Contains(soundSourcePosition);
        }

        public float GetVolumeFor(Vector2 worldPosition)
        {
            // Optional: Hier könnte man später Distanz-basiertes Fading einbauen
            return IsAudible(worldPosition) ? 1f : 0f;
        }
    }
}