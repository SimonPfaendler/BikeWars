using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BikeWars.Content.components
{
    public class MenuButton
    {
        private Texture2D _texture;
        private Rectangle _drawBounds;
        private Rectangle _collisionBounds;
        private string _text;
        private SpriteFont _font;
        private Color _textColor;
        
        private Rectangle _originalBounds;
        private Rectangle _hoverBounds;
        private bool _isHovered;
        private const float HOVER_SCALE = 1.2f;
        public int Id { get; private set; }
        
        public MenuButton(int id, Texture2D texture, Rectangle bounds, string text, SpriteFont font, Color? textColor = null)
        {
            Id = id;
            _texture = texture;
            _originalBounds = bounds;
            _drawBounds = bounds;
            _collisionBounds = bounds;
            _text = text;
            _font = font;
            _textColor = textColor ?? Color.Black;
            
            CalculateHoverBounds();
            _drawBounds = _originalBounds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _drawBounds, Color.White);

            Vector2 textSize = _font.MeasureString(_text);
            float scale = 1.5f;
            Vector2 scaledTextSize = textSize * scale;

            Vector2 textPosition = new Vector2(
                _drawBounds.X + (_drawBounds.Width - scaledTextSize.X) / 2,
                _drawBounds.Y + (_drawBounds.Height - scaledTextSize.Y) / 2
            );
            
            spriteBatch.DrawString(_font, _text, textPosition, _textColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
        
        public void Update(MouseState mouseState)
        {
            // make button bigger when the mouse is in its bounds
            _isHovered = _collisionBounds.Contains(mouseState.Position);
            
            _drawBounds = _isHovered ? _hoverBounds : _originalBounds;
        }

        public bool IsClicked(MouseState currentMouseState, MouseState previousMouseState)
        {
            bool isClicked = _collisionBounds.Contains(currentMouseState.Position) &&
                             currentMouseState.LeftButton == ButtonState.Pressed &&
                             previousMouseState.LeftButton == ButtonState.Released;
           
            if (isClicked)
            {
                Game1.SoundHandler.PlayButtonClick((ButtonAction)Id);
            }
    
            return isClicked;
        }

        public bool Contains(Point point)
        {
            return _collisionBounds.Contains(point);
        }
        
        private void CalculateHoverBounds()
        {
            int hoverWidth = (int)(_originalBounds.Width * HOVER_SCALE);
            int hoverHeight = (int)(_originalBounds.Height * HOVER_SCALE);
            
            int hoverX = _originalBounds.X - (hoverWidth - _originalBounds.Width) / 2;
            int hoverY = _originalBounds.Y - (hoverHeight - _originalBounds.Height) / 2;
            
            _hoverBounds = new Rectangle(hoverX, hoverY, hoverWidth, hoverHeight);
        }

        public Rectangle Bounds => _collisionBounds; 
        public string Text => _text;
        public bool IsHovered => _isHovered;
    }
}