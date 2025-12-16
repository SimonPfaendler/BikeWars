using BikeWars.Content.engine.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace BikeWars.Content.components
{
    public class MenuButton
    {
        private readonly Texture2D _texture;
        private Rectangle _drawBounds;
        private Rectangle _collisionBounds;
        private readonly string _text;
        private readonly SpriteFont _font;
        private Color _textColor;
        
        private Rectangle _originalBounds;
        private Rectangle _hoverBounds;
        private bool _isHovered;
        private const float HOVER_SCALE = 1.2f;
        public int Id { get; private set; }
        private readonly AudioService _audioService;
        private Color _backgroundColor = Color.White;

        
        public MenuButton(int id, Texture2D texture, Rectangle bounds, string text, SpriteFont font, AudioService audioService, Color? textColor = null)
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
            _audioService = audioService;
        }
        
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _drawBounds, _backgroundColor);

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
                if (Id == (int)ButtonAction.StartGame)
                    _audioService.Sounds.Play(AudioAssets.HandgunClick);
                else
                    _audioService.Sounds.Play(AudioAssets.SoftClick);
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