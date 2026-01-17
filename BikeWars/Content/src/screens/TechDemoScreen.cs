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
        BikeThief,
        Dog,
        Kamikaze
    }

    public class TechDemoScreen : GameScreen
    {

        private MenuButton _spawnHoboBtn;
        private MenuButton _spawnBikeBtn;
        private MenuButton _spawnDogBtn;

        private MenuButton _spawnKamikazeBtn;
        private MenuButton _spawnTramBtn;

        private MouseState _prevMouse;

        private readonly System.Random _random = new System.Random();

        public TechDemoScreen(AudioService audioService)
            : base(audioService, GameMode.SinglePlayer, true)
        {
        }

        public override void LoadContent(ContentManager content, GraphicsDevice gd)
        {
            base.LoadContent(content, gd);

            // creates simple button texture

            // makes the 3 tech-demo buttons
            _spawnHoboBtn = new MenuButton(
                id: 1,
                texture: RenderPrimitives.Pixel,
                bounds: new Rectangle(30, 150, 200, 60),
                text: "Spawn 100 Hobos",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );

            _spawnBikeBtn = new MenuButton(
                id: 2,
                texture: RenderPrimitives.Pixel,
                bounds: new Rectangle(30, 220, 200, 60),
                text: "Spawn 15 Thieves",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );

            _spawnDogBtn = new MenuButton(
                id: 1,
                texture: RenderPrimitives.Pixel,
                bounds: new Rectangle(30, 290, 200, 60),
                text: "Spawn 50 Dogs",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );

            _spawnKamikazeBtn = new MenuButton(
                id: 1,
                texture: RenderPrimitives.Pixel,
                bounds: new Rectangle(30, 360, 200, 60),
                text: "Spawn 1 Opa",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );

            _spawnTramBtn = new MenuButton(
                id: 1,
                texture: RenderPrimitives.Pixel,
                bounds: new Rectangle(30, 430, 200, 60),
                text: "Spawn 1 Tram",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            MouseState mouse = Mouse.GetState();

            _spawnHoboBtn.Update(mouse, gameTime);
            _spawnBikeBtn.Update(mouse, gameTime);
            _spawnDogBtn.Update(mouse, gameTime);
            _spawnKamikazeBtn.Update(mouse, gameTime);
            _spawnTramBtn.Update(mouse, gameTime);

            if(_spawnHoboBtn.IsClicked(mouse, _prevMouse))
                SpawnEnemies(EnemyType.Hobo, 100);

            if (_spawnBikeBtn.IsClicked(mouse, _prevMouse))
                SpawnEnemies(EnemyType.BikeThief, 15);

            if(_spawnDogBtn.IsClicked(mouse, _prevMouse))
                SpawnEnemies(EnemyType.Dog, 50);

            if (_spawnKamikazeBtn.IsClicked(mouse, _prevMouse))
                SpawnEnemies(EnemyType.Kamikaze, 1);

            if (_spawnTramBtn.IsClicked(mouse, _prevMouse))
                 _spawnManager.SpawnTram(1500f);



            _prevMouse = mouse;
        }

        // spawns enemies when you click their button
        private void SpawnEnemies(EnemyType type, int amount)
        {
            var playerPos = GameObjectManager.Player1.Transform.Position;

            for (int i = 0; i < amount; i++)
            {
                Vector2 spawnPos = playerPos;

                // Try up to 20 times to find a walkable spawn tile
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    float spawnX = _random.Next(-300, 301);
                    float spawnY = _random.Next(-300, 301);

                    var candidate = playerPos + new Vector2(spawnX, spawnY);
                    var grid = CollisionManager.WorldToGrid(candidate);

                    var gridW = CollisionManager.PathGrid.GetLength(0);
                    var gridH = CollisionManager.PathGrid.GetLength(1);

                    if (grid.X < 0 || grid.X >= gridW || grid.Y < 0 || grid.Y >= gridH)
                        continue;

                    if (!CollisionManager.PathGrid[grid.X, grid.Y].Walkable)
                        continue;

                    // ok, this is a walkable tile → use it
                    spawnPos = candidate;
                    break;
                }

                CharacterBase enemy;

                switch (type)
                {
                    case EnemyType.Hobo:
                        enemy = new Hobo(spawnPos, new Point(32, 32), AudioService, PathFinding,
                            CollisionManager, RepathScheduler);
                        break;

                    case EnemyType.BikeThief:
                        enemy = new BikeThief(spawnPos, new Point(32, 32), AudioService, PathFinding,
                            CollisionManager, RepathScheduler);
                        break;
                    case EnemyType.Dog:
                        enemy = new Dog(spawnPos, new Point(32, 32), AudioService, PathFinding,
                            CollisionManager, RepathScheduler);
                        break;
                    case EnemyType.Kamikaze:
                        enemy = new KamikazeOpa(spawnPos, new Point(32, 32), AudioService, PathFinding,
                            CollisionManager, GameObjectManager, RepathScheduler);
                        break;

                    default:
                        return;
                }
                GameObjectManager.AddCharacter(enemy);
            }
        }

        // draws the buttons
        public override void Draw(GameTime gameTime, SpriteBatch sb)
        {
            base.Draw(gameTime, sb);
            sb.Begin();
            _spawnHoboBtn.Draw(sb);
            _spawnBikeBtn.Draw(sb);
            _spawnDogBtn.Draw(sb);
            _spawnKamikazeBtn.Draw(sb);
            _spawnTramBtn.Draw(sb);
            sb.End();
        }
    }
}