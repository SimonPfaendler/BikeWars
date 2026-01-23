using System;
using BikeWars.Content.engine.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BikeWars.Content.components
{
    public class MenuButton
    {
        private readonly Texture2D _texture;
        private readonly SpriteFont _font;
        private readonly string _text;
        private readonly AudioService _audioService;

        private Rectangle _originalBounds;
        private Rectangle _hoverBounds;
        private Rectangle _collisionBounds;

        private bool _isHovered;
        private bool _isSelected;

        private Color _textColor;
        private Color _backgroundColor = Color.White;

        private const float HOVER_SCALE = 1.10f;
        private const float SELECT_SCALE = 1.18f;
        private const int BORDER_THICKNESS = 4;

        public event Action<int> Clicked;

        public int Id { get; }

        public MenuButton(
            int id,
            Texture2D texture,
            Rectangle bounds,
            string text,
            SpriteFont font,
            AudioService audioService,
            Color? textColor = null)
        {
            Id = id;
            _texture = texture;
            _originalBounds = bounds;
            _collisionBounds = bounds;
            _text = text;
            _font = font;
            _audioService = audioService;
            _textColor = textColor ?? Color.Black;

            CalculateHoverBounds();
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => _isSelected = value;
        }

        public bool IsHovered => _isHovered;
        public Rectangle Bounds => _collisionBounds;
        public string Text => _text;

        public void Update(MouseState mouseState, GameTime gameTime)
        {
            _isHovered = _collisionBounds.Contains(mouseState.Position);
        }

        public bool ShowSelection { get; set; } = true;

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawBounds = _originalBounds;

            if ((_isSelected && ShowSelection) || _isHovered)
                drawBounds = GetScaledBounds(_originalBounds, _isSelected ? SELECT_SCALE : HOVER_SCALE);

            spriteBatch.Draw(_texture, drawBounds, _backgroundColor);

            if (_isSelected && ShowSelection)
                DrawBorder(spriteBatch, drawBounds, BORDER_THICKNESS, Color.Gold);

            DrawText(spriteBatch, drawBounds);
        }


        public bool IsClicked(MouseState currentMouseState, MouseState previousMouseState)
        {
            bool isClicked =
                _collisionBounds.Contains(currentMouseState.Position) &&
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

        public void TriggerClick()
        {
            Clicked?.Invoke(Id);
        }

        public bool Contains(Point point)
        {
            return _collisionBounds.Contains(point);
        }

        private void CalculateHoverBounds()
        {
            _hoverBounds = GetScaledBounds(_originalBounds, HOVER_SCALE);
        }

        private Rectangle GetScaledBounds(Rectangle r, float scale)
        {
            int w = (int)(r.Width * scale);
            int h = (int)(r.Height * scale);

            return new Rectangle(
                r.X - (w - r.Width) / 2,
                r.Y - (h - r.Height) / 2,
                w,
                h
            );
        }

        private void DrawText(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Vector2 textSize = _font.MeasureString(_text);
            float scale = 1.5f;

            Vector2 position = new Vector2(
                bounds.X + (bounds.Width - textSize.X * scale) / 2,
                bounds.Y + (bounds.Height - textSize.Y * scale) / 2
            );

            spriteBatch.DrawString(
                _font,
                _text,
                position,
                _textColor,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f
            );
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, int thickness, Color color)
        {
            spriteBatch.Draw(RenderPrimitives.Pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            spriteBatch.Draw(RenderPrimitives.Pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            spriteBatch.Draw(RenderPrimitives.Pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            spriteBatch.Draw(RenderPrimitives.Pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }
    }
}
