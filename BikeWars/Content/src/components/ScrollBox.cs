using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.managers;
using System;

namespace BikeWars.Content.components
{
    public class ScrollBox : IScreen
    {
        private float _scrollOffset = 0f;
        private RasterizerState _scissorRaster {get; set;}
        public RasterizerState ScissorRaster {get => _scissorRaster; set => _scissorRaster = value;}

        private const float SCROLL_SPEED_KEYBOARD = 5f;
        private const float SCROLL_SPEED_MOUSE = 12f;

        private Rectangle _scrollArea {get; set;}
        public Rectangle ScrollArea {get => _scrollArea; set => _scrollArea = value;}

        private SpriteBatch _spriteBatch;

        private readonly Action<SpriteBatch, Vector2> _drawContent; // This one is necessary so we can tell the parent what we want to draw
        private readonly Func<float> _getContentHeight; // Necessary to get the maximum of scrolling

        private Color _backgroundColor = new Color(255, 255, 255, 128);
        private Color _borderColor = Color.White;
        private int _borderThickness = 3;

        private int _padding = 5; // For the content. We can adapt it if we need like multiple directions.

        protected Texture2D _backgroundTexture;
        protected SpriteFont _font;
        public ScreenManager ScreenManager { get; set; }

        private int _fadeHeight = 20; // Fading effect
        private Color _fadeColor = Color.White * 0.3f;

        public bool UpdateLower => throw new System.NotImplementedException();

        public bool DrawLower => throw new System.NotImplementedException();

        public ScrollBox(Texture2D background, SpriteFont font, Rectangle area, SpriteBatch batch, Action<SpriteBatch, Vector2> drawContent, Func<float> getContentHeight)
        {
            _backgroundTexture = background;
            _font = font;
            ScrollArea = area;

            ScissorRaster = new RasterizerState();
            ScissorRaster.MultiSampleAntiAlias = false;
            ScissorRaster.ScissorTestEnable = true;
            _spriteBatch = batch;
            _drawContent = drawContent;
            _getContentHeight = getContentHeight;

        }

        public void Update()
        {
            if (InputHandler.IsHeld(GameAction.UI_DOWN))
            {
                _scrollOffset += SCROLL_SPEED_KEYBOARD;
            }
            if (InputHandler.IsHeld(GameAction.UI_UP))
            {
                _scrollOffset -= SCROLL_SPEED_KEYBOARD;
            }
            _scrollOffset -= InputHandler.Mouse.ScrollDelta / SCROLL_SPEED_MOUSE;
            float maxScroll = Math.Max(0, _getContentHeight() - ScrollArea.Height);
            _scrollOffset = MathHelper.Clamp(_scrollOffset, 0, maxScroll);
        }

        private void DrawScrollFade(SpriteBatch sb)
        {
            float contentHeight = _getContentHeight();
            float maxScroll = Math.Max(0, contentHeight - ScrollArea.Height);

            Texture2D bg = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
            bg.SetData(new[] { Color.Black });

            if (_scrollOffset > 0)
            {
                // Fade upwards
                sb.Draw(bg,
                        new Rectangle(ScrollArea.X, ScrollArea.Y, ScrollArea.Width, _fadeHeight),
                        _fadeColor);
            }

            if (_scrollOffset < maxScroll)
            {
                // Fade downwards
                sb.Draw(bg,
                        new Rectangle(ScrollArea.X, ScrollArea.Bottom - _fadeHeight, ScrollArea.Width, _fadeHeight),
                        _fadeColor);
            }
        }
        public virtual void Draw()
        {
            Game1.Instance.GraphicsDevice.ScissorRectangle = _scrollArea;
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                ScissorRaster
            );

            // Background
            _spriteBatch.Draw(
                _backgroundTexture,
                ScrollArea,
                _backgroundColor
            );

            // content
            _drawContent?.Invoke(
                _spriteBatch,
                new Vector2(_scrollArea.X + _padding, _scrollArea.Y - _scrollOffset + _padding)
            );
            _spriteBatch.End();
            // Border outside the Scissors
            _spriteBatch.Begin();
            DrawBorder(_spriteBatch, ScrollArea, _borderColor, _borderThickness);
            DrawScrollFade(_spriteBatch);
            _spriteBatch.End();
        }

        private void DrawBorder(SpriteBatch sb, Rectangle rect, Color color, int thickness)
        {
            Texture2D pixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            // Oben
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Unten
            sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            // Links
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Rechts
            sb.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        public void Update(GameTime gameTime)
        {
            throw new System.NotImplementedException();
        }

        public void Draw(GameTime gameTime)
        {
            throw new System.NotImplementedException();
        }
    }
}