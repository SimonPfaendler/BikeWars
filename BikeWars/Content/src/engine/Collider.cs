using Microsoft.Xna.Framework;

namespace BikeWars.Content
{
    public class Collider
    {
        private int width { get; set; }
        private int height { get; set; }

        private Vector2 _position;

        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                UpdateBox();
            }
        }
        private Rectangle collisionBox { get; set; }
        
        public Collider(Vector2 pos, int w, int h)
        {
            width = w;
            height = h;
            _position = pos;
        }

        public void SetSize(int w, int h)
        {
            width = w;
            height = h;
            UpdateBox();
        }
        private void UpdateBox()
        {
            collisionBox = new Rectangle((int)_position.X, (int)_position.Y, width, height);
        }
        public void setCollisionBox(Rectangle rect)
        {
            collisionBox = rect;
        }
        public virtual void Update(GameTime gameTime)
        {
            // Logik hier einfügen, z.B. Position verändern
        }
    }
}