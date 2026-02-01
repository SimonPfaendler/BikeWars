using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BikeWars.Entities.Characters;
using BikeWars.Content.components;
using BikeWars.Content.entities.interfaces;
using BikeWars.Utilities;
using BikeWars.Content.entities.npcharacters;
using BikeWars.Content.engine;
using BikeWars.Content.src.utils.SaveLoadExample;
using System;

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
        Kamikaze,
        Dozent,
        Police
    }

    public class TechDemoScreen : GameScreen
    {

        private MenuButton _spawnHoboBtn;
        private MenuButton _spawnBikeBtn;
        private MenuButton _spawnDogBtn;

        private MenuButton _spawnKamikazeBtn;
        private MenuButton _spawnTramBtn;
        private MenuButton _spawnEnemyCircleBtn;
        private MenuButton _spawnDozentBtn;
        private MenuButton _spawnPoliceBtn;
        private MenuButton _godModeSwitchBtn;
        private MenuButton _startTimerBtn;
        private MenuButton _spawnCarBtn;
        private readonly List<RaveGroup> _raveGroups = new List<RaveGroup>();
        private MouseState _prevMouse;

        public TechDemoScreen(AudioService audioService)
            : base(audioService, GameMode.SinglePlayer, true)
        {
        }

        public override void LoadContent(ContentManager content, GraphicsDevice gd)
        {
            base.LoadContent(content, gd);

            _gameTimer.OnTimerFinished += OnGameTimerFinished;
            // compact left-aligned column for tech-demo controls
            int btnX = 20;
            int btnY = 140;
            int btnW = 240;
            int btnH = 44;
            int btnSpacing = 10;

            Rectangle NextBtnRect()
            {
                var r = new Rectangle(btnX, btnY, btnW, btnH);
                btnY += btnH + btnSpacing;
                return r;
            }

            _spawnHoboBtn = new MenuButton(
                id: 1,
                texture: RenderPrimitives.Pixel,
                bounds: NextBtnRect(),
                text: "Spawn 100 Hobos",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );
            _spawnHoboBtn.TextScale = 1.15f;

            _spawnBikeBtn = new MenuButton(
                id: 2,
                texture: RenderPrimitives.Pixel,
                bounds: NextBtnRect(),
                text: "Spawn 15 Thieves",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );
            _spawnBikeBtn.TextScale = 1.15f;

            _spawnDogBtn = new MenuButton(
                id: 1,
                texture: RenderPrimitives.Pixel,
                bounds: NextBtnRect(),
                text: "Spawn 10 Dogs",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );
            _spawnDogBtn.TextScale = 1.15f;

            _spawnKamikazeBtn = new MenuButton(
                id: 1,
                texture: RenderPrimitives.Pixel,
                bounds: NextBtnRect(),
                text: "Spawn 1 Opa",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );
            _spawnKamikazeBtn.TextScale = 1.15f;

            _spawnTramBtn = new MenuButton(
                id: 1,
                texture: RenderPrimitives.Pixel,
                bounds: NextBtnRect(),
                text: "Spawn 1 Tram",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );
            _spawnTramBtn.TextScale = 1.15f;

            _spawnEnemyCircleBtn = new MenuButton(
                id: 6,
                texture: RenderPrimitives.Pixel,
                bounds: NextBtnRect(),
                text: "Spawn Enemy Circle",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );
            _spawnEnemyCircleBtn.TextScale = 1.15f;

            _spawnDozentBtn = new MenuButton(
                id: 6,
                texture: RenderPrimitives.Pixel,
                bounds: NextBtnRect(),
                text: "Spawn 10 Dozents",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );
            _spawnDozentBtn.TextScale = 1.15f;

            _spawnPoliceBtn = new MenuButton(
                id: 7,
                texture: RenderPrimitives.Pixel,
                bounds: NextBtnRect(),
                text: "Spawn 5 Policemen",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );
            _spawnPoliceBtn.TextScale = 1.15f;

            _godModeSwitchBtn = new MenuButton(
                id: 8,
                texture: RenderPrimitives.Pixel,
                bounds: NextBtnRect(),
                text: "GodMode On",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );
            _godModeSwitchBtn.TextScale = 1.15f;
            _startTimerBtn = new MenuButton(
                id: 9,
                texture: RenderPrimitives.Pixel,
                bounds: NextBtnRect(),
                text: "Start Timer",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );
            _startTimerBtn.TextScale = 1.15f;
            _spawnCarBtn = new MenuButton(
                id: 11,
                texture: RenderPrimitives.Pixel,
                bounds: NextBtnRect(),
                text: "Spawn 5 Cars",
                font: UIAssets.DefaultFont,
                audioService: AudioService
            );
            _spawnHoboBtn.TextScale = 1.15f;
        }

        protected override void OnTechDemoReset()
        {
            _raveGroups.Clear();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _gameTimer.Update(gameTime);
            for (int i = _raveGroups.Count - 1; i >= 0; i--)
            {
                _raveGroups[i].Update(gameTime);

                if (!_raveGroups[i].IsActive)
                    _raveGroups.RemoveAt(i);
            }

            MouseState mouse = Mouse.GetState();

            _spawnHoboBtn.Update(mouse, gameTime);
            _spawnBikeBtn.Update(mouse, gameTime);
            _spawnDogBtn.Update(mouse, gameTime);
            _spawnKamikazeBtn.Update(mouse, gameTime);
            _spawnTramBtn.Update(mouse, gameTime);
            _spawnEnemyCircleBtn.Update(mouse, gameTime);
            _spawnDozentBtn.Update(mouse, gameTime);
            _spawnPoliceBtn.Update(mouse, gameTime);
            _godModeSwitchBtn.Update(mouse, gameTime);
            _startTimerBtn.Update(mouse, gameTime);
            _spawnCarBtn.Update(mouse, gameTime);

            if(_spawnHoboBtn.IsClicked(mouse, _prevMouse))
                SpawnEnemies(EnemyType.Hobo, 100);

            if(_spawnDozentBtn.IsClicked(mouse, _prevMouse))
                SpawnEnemies(EnemyType.Dozent, 10);

            if (_spawnBikeBtn.IsClicked(mouse, _prevMouse))
                SpawnEnemies(EnemyType.BikeThief, 15);

            if(_spawnDogBtn.IsClicked(mouse, _prevMouse))
                SpawnEnemies(EnemyType.Dog, 10);

            if (_spawnKamikazeBtn.IsClicked(mouse, _prevMouse))
                SpawnEnemies(EnemyType.Kamikaze, 1);

            if (_spawnTramBtn.IsClicked(mouse, _prevMouse))
                 _spawnManager.SpawnTram(1500f);

            if (_spawnEnemyCircleBtn.IsClicked(mouse, _prevMouse))
                SpawnRaveCircle(count: 16, startRadius: 200f);

            if (_spawnPoliceBtn.IsClicked(mouse, _prevMouse))
                SpawnEnemies(EnemyType.Police, 5);
            
            if (_spawnCarBtn.IsClicked(mouse, _prevMouse))
            {
                var playerPos = GameObjectManager.Player1.Transform.Position;

                if (!IsPlayerNearRoad(playerPos))
                    return; 

                for (int i = 0; i < 5; i++)
                    _spawnManager.SpawnCar(0.0);
            }

            if (_godModeSwitchBtn.IsClicked(mouse, _prevMouse))
            {
                GameObjectManager.Player1.IsGodMode = !GameObjectManager.Player1.IsGodMode;
                if (GameObjectManager.Player2 != null)
                {
                    GameObjectManager.Player2.IsGodMode = !GameObjectManager.Player2.IsGodMode;
                }
                if (GameObjectManager.Player1.IsGodMode)
                {
                    _godModeSwitchBtn.Text = "GodMode On";
                } else
                {
                    _godModeSwitchBtn.Text = "GodMode Off";
                }
            }

            if (_startTimerBtn.IsClicked(mouse, _prevMouse))
            {
                _gameTimer.Start();
                _gameTimer.setGameTimer(5f);
            }
            _prevMouse = mouse;
        }
        
        // checks if the player is near a road
        private bool IsPlayerNearRoad(Vector2 playerPos, int radiusTiles = 3)
        {
            Point p = CollisionManager.WorldToGrid(playerPos);

            for (int y = -radiusTiles; y <= radiusTiles; y++)
            for (int x = -radiusTiles; x <= radiusTiles; x++)
            {
                if (CollisionManager.IsRoadTile(new Point(p.X + x, p.Y + y)))
                    return true;
            }

            return false;
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
                    float spawnX = RandomUtil.NextInt(-500, 501);
                    float spawnY = RandomUtil.NextInt(-500, 501);

                    var candidate = playerPos + new Vector2(spawnX, spawnY);
                    var grid = CollisionManager.WorldToGrid(candidate);

                    var gridW = CollisionManager.PathGrid.GetLength(0);
                    var gridH = CollisionManager.PathGrid.GetLength(1);

                    if (grid.X < 0 || grid.X >= gridW || grid.Y < 0 || grid.Y >= gridH)
                        continue;

                    if (!CollisionManager.PathGrid[grid.X, grid.Y].Walkable)
                        continue;

                    spawnPos = candidate;
                    break;
                }

                CharacterBase enemy;

                switch (type)
                {
                    case EnemyType.Hobo:
                        enemy = new Hobo(spawnPos, 15, AudioService, PathFinding,
                            CollisionManager, RepathScheduler);
                        break;

                    case EnemyType.BikeThief:
                        enemy = new BikeThief(spawnPos, 15, AudioService, PathFinding,
                            CollisionManager, RepathScheduler);
                        break;
                    case EnemyType.Dog:
                        enemy = new Dog(spawnPos, 13, AudioService, PathFinding,
                            CollisionManager, RepathScheduler);
                        break;
                    case EnemyType.Kamikaze:
                        enemy = new KamikazeOpa(spawnPos, 13, AudioService, PathFinding,
                            CollisionManager, GameObjectManager, RepathScheduler);
                        break;
                    case EnemyType.Dozent:
                        enemy = new Dozent(spawnPos, 25, AudioService, PathFinding,
                            CollisionManager, RepathScheduler);
                        break;
                    case EnemyType.Police:
                        enemy = new PoliceMan(spawnPos, 20, AudioService, PathFinding,
                            CollisionManager, RepathScheduler);
                        break;

                    default:
                        return;
                }
                GameObjectManager.AddCharacter(enemy);
            }
        }

        // spawn the circle
        private void SpawnRaveCircle(int count, float startRadius)
        {
            if (_raveGroups.Any(g => g.IsActive))
                return;

            var group = RaveGroup.SpawnAroundPlayer(
                count: count,
                startRadius: startRadius,
                raverSize: 40,
                audioService: AudioService,
                gameObjectManager: GameObjectManager,
                collisionManager: CollisionManager,
                shrinkSpeed: 30f,
                minRadius: 55f,
                beatInterval: 0.2f
            );

            if (group != null)
            {
                group.OnDied += HandleRaveGroupDied;
                _raveGroups.Add(group);
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
            _spawnEnemyCircleBtn.Draw(sb);
            _spawnDozentBtn.Draw(sb);
            _spawnPoliceBtn.Draw(sb);
            _godModeSwitchBtn.Draw(sb);
            _startTimerBtn.Draw(sb);
            _spawnCarBtn.Draw(sb);
            sb.End();
        }

        public void HandleRaveGroupDied()
        {
            _achievementsManager.OnRaverGroupDied();
        }
    }
}