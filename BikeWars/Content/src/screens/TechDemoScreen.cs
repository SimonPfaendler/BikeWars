using Microsoft.Xna.Framework.Content;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BikeWars.Entities.Characters;
using BikeWars.Content.components;
using BikeWars.Content.entities.interfaces;

// adds debugging tools for testing
// like allowing the dev to spawn a large groups of enemies
// the final tech demo should allow the dev to spawn 1000 enemies on the screen 

namespace BikeWars.Content.screens
{
    
    public enum EnemyType
    {
        Hobo,
        BikeThief
    }
    
    public class TechDemoScreen : GameScreen
    {
        
        private MenuButton _spawnHoboBtn;
        private MenuButton _spawnBikeBtn;

        private Texture2D _buttonTex;
        private SpriteFont _font;
        private MouseState _prevMouse;
        
        private readonly System.Random _random = new System.Random();

        public TechDemoScreen(AudioService audioService) 
            : base(audioService, true)
        {
            LoadContent(Game1.Instance.Content);
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);
            
            _font = content.Load<SpriteFont>("assets/fonts/Arial");

            // creates simple button texture
            _buttonTex = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
            _buttonTex.SetData(new[] { Color.White });
            
            // makes the 2 tech-demo buttons
            _spawnHoboBtn = new MenuButton(
                id: 1,
                texture: _buttonTex,
                bounds: new Rectangle(30, 150, 200, 60),
                text: "Spawn 15 Hobos",
                font: _font,
                audioService: AudioService
            );
            
            _spawnBikeBtn = new MenuButton(
                id: 2,
                texture: _buttonTex,
                bounds: new Rectangle(30, 220, 200, 60),
                text: "Spawn 15 Thieves",
                font: _font,
                audioService: AudioService
            );


        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            MouseState mouse = Mouse.GetState();
            
            _spawnHoboBtn.Update(mouse);
            _spawnBikeBtn.Update(mouse);
            
            if(_spawnHoboBtn.IsClicked(mouse, _prevMouse))
                SpawnEnemies(EnemyType.Hobo, 15);

            if (_spawnBikeBtn.IsClicked(mouse, _prevMouse))
                SpawnEnemies(EnemyType.BikeThief, 15);
            
            _prevMouse = mouse;
        }

        // spawns enemies when you click their button
        private void SpawnEnemies(EnemyType type, int amount)
        {
            var playerPos = GameObjectManager.Player1.Transform.Position;

            for (int i = 0; i < amount; i++)
            {
                float spawnX = _random.Next(-300, 301);
                float spawnY = _random.Next(-300, 301);
                
                Vector2 spawnPos = playerPos + new Vector2(spawnX, spawnY);
                
                CharacterBase enemy;
                
                switch (type)
                {
                    case EnemyType.Hobo:
                        enemy = new Hobo(spawnPos, new Point(32, 32), AudioService);
                        break;

                    case EnemyType.BikeThief:
                        enemy = new BikeThief(spawnPos, new Point(32, 32), AudioService);
                        break;

                    default:
                        return;
                }

                enemy.LoadContent(this.ContentManager);
                GameObjectManager.AddCharacter(enemy);
            }
        }
        
        // draws the buttons
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            var spriteBatch = Game1.Instance.SpriteBatch;

            spriteBatch.Begin();
            _spawnHoboBtn.Draw(spriteBatch);
            _spawnBikeBtn.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}