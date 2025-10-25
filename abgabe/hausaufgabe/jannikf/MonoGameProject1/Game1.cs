using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameProject1
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _gfx;
        private SpriteBatch _sb;

        private Texture2D _texBackground;
        private Texture2D _texLogo;
        private SoundEffect _sfxHit;
        private SoundEffect _sfxMiss;

        private Vector2 _screenCenter;
        private float _orbitAngle;
        private float _orbitSpeed = 0.8f;
        private float _logoScale;
        private float _logoRotation;
        private float _spinSpeed = 1.6f;
        private float _orbitRadius;
        private Vector2 _logoPos;
        private Vector2 _logoOrigin;

        private Color[] _logoPixels;

        private MouseState _mousePrev;

        public Game1()
        {
            _gfx = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            var bounds = Window.ClientBounds;
            _screenCenter = new Vector2(bounds.Width * 0.5f, bounds.Height * 0.5f);
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);

            _texBackground = Content.Load<Texture2D>("Background");
            _texLogo       = Content.Load<Texture2D>("Unilogo");
            _sfxHit        = Content.Load<SoundEffect>("logo_hit");
            _sfxMiss       = Content.Load<SoundEffect>("logo_miss");

            _logoOrigin = new Vector2(_texLogo.Width * 0.5f, _texLogo.Height * 0.5f);
            _logoPixels = new Color[_texLogo.Width * _texLogo.Height];
            _texLogo.GetData(_logoPixels);

            var w = Window.ClientBounds.Width;
            var h = Window.ClientBounds.Height;
            float targetMax = MathF.Min(w, h) * 0.25f;
            _logoScale = targetMax / MathF.Max(_texLogo.Width, _texLogo.Height);

            float halfW = w * 0.5f;
            float halfH = h * 0.5f;
            float halfLogoW = (_texLogo.Width  * _logoScale) * 0.5f;
            float halfLogoH = (_texLogo.Height * _logoScale) * 0.5f;
            float pad = 2f;
            _orbitRadius = MathF.Min(halfW - halfLogoW - pad, halfH - halfLogoH - pad);
            if (_orbitRadius < 0f) _orbitRadius = 0f;

            _orbitAngle = 0f;
            UpdateLogoPosition(0f);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _orbitAngle += _orbitSpeed * dt;
            UpdateLogoPosition(dt);

            _logoRotation += _spinSpeed * dt;

            var mouseNow = Mouse.GetState();
            bool justClicked = mouseNow.LeftButton == ButtonState.Pressed &&
                               _mousePrev.LeftButton == ButtonState.Released;

            if (justClicked)
            {
                bool hit = HitTestPixelPerfect(mouseNow.Position);
                if (hit) _sfxHit?.Play(); else _sfxMiss?.Play();
            }

            _mousePrev = mouseNow;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            _sb.Draw(_texBackground, destinationRectangle: new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height),
                     color: Color.White);

            _sb.Draw(_texLogo,
                     position: _logoPos,
                     sourceRectangle: null,
                     color: Color.White,
                     rotation: _logoRotation,
                     origin: _logoOrigin,
                     scale: _logoScale,
                     effects: SpriteEffects.None,
                     layerDepth: 0f);

            _sb.End();

            base.Draw(gameTime);
        }

        private void UpdateLogoPosition(float dt)
        {
            var offset = new Vector2(MathF.Cos(_orbitAngle), MathF.Sin(_orbitAngle)) * _orbitRadius;
            _logoPos = _screenCenter + offset;
        }

        private bool HitTestPixelPerfect(Point mouse)
        {
            Vector2 toPoint = new Vector2(mouse.X, mouse.Y) - _logoPos;

            float invRot = -_logoRotation;
            float cos = MathF.Cos(invRot);
            float sin = MathF.Sin(invRot);
            Vector2 unrot = new Vector2(
                toPoint.X * cos - toPoint.Y * sin,
                toPoint.X * sin + toPoint.Y * cos
            );

            if (_logoScale <= 0f) return false;
            Vector2 local = unrot / _logoScale + _logoOrigin;

            int x = (int)MathF.Floor(local.X);
            int y = (int)MathF.Floor(local.Y);
            if (x < 0 || y < 0 || x >= _texLogo.Width || y >= _texLogo.Height)
                return false;

            Color c = _logoPixels[y * _texLogo.Width + x];
            return c.A > 10;
        }
    }
}

