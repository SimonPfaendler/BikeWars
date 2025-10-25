using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel.Design;

namespace Project1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Texture2D Background;
        Texture2D Logo;
        
        // Orbital movement
        private Vector2 _center;
        private Vector2 _position;
        private float _radius = 150f;          
        private float _speed = 275f;           
        private float _angle = 0f;
        
        
        int score = 0;
        SpriteFont font;
        bool mReleased = true;
        private SoundEffect _hitsound;
        private SoundEffect _misssound;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 1024;
            _graphics.ApplyChanges();





        }

        protected override void Initialize()
        {
            
                        
            _center = new Vector2(
                _graphics.PreferredBackBufferWidth / 2f,
                _graphics.PreferredBackBufferHeight / 2f
            );

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            
            Background = Content.Load<Texture2D>("Background");
            Logo = Content.Load<Texture2D>("Unilogo");
            font = Content.Load<SpriteFont>("galleryFont");
            _hitsound = Content.Load<SoundEffect>("Logo_hit");
            _misssound = Content.Load<SoundEffect>("Logo_miss");


        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Winkelgeschwindigkeit = v / r  ->  konstante Bahngeschwindigkeit
            float angularSpeed = _speed / _radius;

            _angle += angularSpeed * delta;


            // Aktuelle Position auf der Kreisbahn
            _position = _center + new Vector2(
                (float)Math.Cos(_angle),
                (float)Math.Sin(_angle)
            ) * _radius;

            MouseState mouseState = Mouse.GetState();

            // Mouse Click Detection 
            if (mouseState.LeftButton == ButtonState.Pressed && mReleased == true)
            {
                if (Vector2.Distance(new Vector2(mouseState.X, mouseState.Y), _position) < Logo.Width * 0.5f * 0.2f)
                {
                    score++;
                    mReleased = false;
                    _hitsound.Play();
                }
                else
                {
                    _misssound.Play();
                    mReleased = false;
                }
            }


            if (mouseState.LeftButton == ButtonState.Released)
            {
                mReleased = true;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            
            _spriteBatch.Begin();
            Vector2 origin = new Vector2(Logo.Width / 2f, Logo.Height / 2f);
            _spriteBatch.Draw(
                Background,
                new Vector2((_graphics.PreferredBackBufferWidth * 0.5f), (_graphics.PreferredBackBufferHeight * 0.5f)),
                null, 
                Color.White,
                0.0f, 
                new Vector2(Background.Width, Background.Height) * 0.5f,
                1.0f,
                SpriteEffects.None, 
                0.0f
            );
            _spriteBatch.Draw(
                Logo,              // texture
                _position,         // position
                null,               // sourceRectangle
                Color.White * 0.5f,        // color
                0.0f,            // rotation
                origin,       // origin
                0.2f,               // scale
                SpriteEffects.None, // effects
                0.0f                // layerDepth
            );
            _spriteBatch.DrawString(
                font,
                score.ToString(),
                new Vector2(100, 100),
                Color.White
            );
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
