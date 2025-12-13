using System;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.entities.items;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using BikeWars.Utilities;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.src.screens.Overlay;
using BikeWars.Content.src.utils.SaveLoadExample;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled.Renderers;
using BikeWars.Content.managers;
using BikeWars.Content.events;

namespace BikeWars.Content.screens
{
    public class GameScreen : IScreen
    {
        private LevelUpScreen _levelUpScreen;
        private readonly ItemManager _itemManager;
        private readonly Camera2D camera;
        private Rectangle worldBounds;
        private Overlay _overlay;
        private TiledMapRenderer _tiledMapRenderer;
        private SpriteFont _debugFont;
        private Debugger _debugger;
        private SpriteFont _font;
        private Texture2D _pixel;
        private const int CELL_SIZE = 16;
        public ScreenManager ScreenManager { get; set; }
        private WorldAudioManager _worldAudioManager;

        private ContentManager _contentManager;
        public ContentManager ContentManager => _contentManager;

        private StatisticsManager _statisticsManager {get; set;}
        public StatisticsManager StatisticsManager => _statisticsManager;
        private readonly AudioService _audioService;
        public AudioService AudioService => _audioService;

        public string DesiredMusic => AudioAssets.GameMusic;
        public float MusicVolume => 1f;

        private HUD hud;
        private Texture2D hudTexture;


        private CollisionManager _collisionManager;
        private GameObjectManager _gameObjectManager;
        public GameObjectManager GameObjectManager => _gameObjectManager;

        private CombatManager _combatManager;

        private SpawnManager _spawnManager;

        private PathFinding _pathFinding;

        private bool _freelook; // Has to be optimized

        protected CollisionManager CollisionManager => _collisionManager;
        protected PathFinding PathFinding => _pathFinding;

        public bool DrawLower => false;
        public bool UpdateLower => false;

        // bool that checks if you're in the tech demo or normal gameplay
        private readonly bool _isTechDemo;
        private bool _showStaticHitboxes = true;

        private GameTimer _gameTimer;
        private const float GAME_TIME_LIMIT = 120f;
        private SpriteFont _timerFont;
        private Vector2 _timerPosition;

        public GameScreen(AudioService audioService, bool isTechDemo = false)
        {

            _isTechDemo = isTechDemo;

            worldBounds = new Rectangle(0, 0, 11200, 11200);

            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));

            _itemManager = new ItemManager();
            _itemManager.AddItem(new Chest(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
            _itemManager.AddItem(new Chest(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 + 90), new Point(32, 32)));
            _collisionManager = new CollisionManager(CELL_SIZE, worldBounds.Height);
            Player player = new Player(new Vector2(worldBounds.Width / 2, worldBounds.Height / 2), new Point(32, 32), _audioService);

            // if we are in the tech demo it transforms the player in god mode
            if (_isTechDemo)
            {
                player.IsGodMode = true;
            }

            _gameObjectManager = new GameObjectManager(_contentManager, player, null);
            // Initial spawning is now handled by SpawnManager
            _gameObjectManager.Items = _itemManager.Items;

            Game1 game = Game1.Instance;
            camera = new Camera2D(
                game.GraphicsDevice.Viewport.Width,
                game.GraphicsDevice.Viewport.Height,
                worldBounds
            );
            _freelook = false;
            camera.Position = _gameObjectManager.Player1.Transform.Position;

            _gameTimer = new GameTimer(GAME_TIME_LIMIT);
            _gameTimer.OnTimerFinished += OnGameTimerFinished;

            _statisticsManager = new StatisticsManager();
            _gameObjectManager.OnCharacterDied += _statisticsManager.HandleCharacterDied;
            _gameObjectManager.OnTookDamage += _statisticsManager.HandleTookDamage;
            _gameObjectManager.Player1.OnTookDamage += _statisticsManager.HandleTookDamage;
            _gameObjectManager.Player1.OnLevelUp += _statisticsManager.HandleLevel;
            _gameObjectManager.Player1.OnMoreXP += _statisticsManager.HandleExperience;

            GameEvents.OnResumeTimer += ResumeTimer;
            HandleLoadNonInGameData();
        }

        public virtual void LoadContent(ContentManager content)
        {
            // Font and Debugger

            _debugFont = content.Load<SpriteFont>("assets/fonts/Arial");
            _debugger = new Debugger(_debugFont, _gameObjectManager.Player1);

            // Tiled Map
            _collisionManager.LoadContent(content);

            // pathfinding object
            _pathFinding = new PathFinding(_collisionManager.PathGrid);
            _tiledMapRenderer = new TiledMapRenderer(Game1.Instance.GraphicsDevice, _collisionManager.TiledMap);

            // Create Combat Manager
            _combatManager = new CombatManager(_audioService, _gameObjectManager);

            // Combat Manager subcribes to Events from Collision Manager:  Collision → Combat
            _collisionManager.OnProjectileHit += _combatManager.HandleProjectileHit;
            _collisionManager.OnAOEHit += _combatManager.HandleAOEHit;
            _collisionManager.OnCharacterCollision += _combatManager.HandleCharacterCollision;
            _collisionManager.OnItemPickup += _gameObjectManager.Player1.OnPickUpItem;
            _gameObjectManager.Player1.ItemPickedUp += _collisionManager.OnRemoveItem;


            // Overlay
            _overlay = new Overlay();

            // HUD
            hudTexture = managers.SpriteManager.GetTexture("HUD_Sheet");
            hud = new HUD(hudTexture);

            _gameObjectManager.LoadContent(content);
            _font = content.Load<SpriteFont>("assets/fonts/Arial");
            _contentManager = content; // We need this to add it later to spawning entities. (Maybe there is another possible implementation)
            _gameObjectManager._contentManager = content;

            _pixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
             // sound control
            Rectangle initialView = GetCameraWorldRect();
            _worldAudioManager = new WorldAudioManager(initialView);
            _gameObjectManager.SetWorldAudioManager(_worldAudioManager);

            _levelUpScreen = new LevelUpScreen();
            // checks if the event OnLevelUp is triggered if it is LevelUpSreen gets active
            _gameObjectManager.Player1.OnLevelUp += (int xp, int amount) =>
            {
                _levelUpScreen.Open(_gameObjectManager.Player1);
            };


            // the Option selected gets upgraded
            _levelUpScreen.OnOptionSelected += skillId =>
            {
                _gameObjectManager.Player1.UpgradeSkill(skillId);
            };

            // Spawn Manager
            _spawnManager = new SpawnManager(_gameObjectManager, _collisionManager, _audioService, _pathFinding);

            // timer
            _timerFont = content.Load<SpriteFont>("assets/fonts/Arial");
            Game1 game = Game1.Instance;
            _timerPosition = new Vector2(
                game.GraphicsDevice.Viewport.Width / 2f,
                40f
            );
            if (!_gameTimer.IsRunning)
            {
                InitializeTimer();
            }
        }
        public virtual void Update(GameTime gameTime)
        {
            // if the LevelUp is Open only the LevelUpMenu gets Updated all the other stuff is basically paused
            // if you want to add something before this or change order please double-check
            InputHandler.Update();
            if (_levelUpScreen.IsOpen)
            {
                _levelUpScreen.Update(gameTime);
                return;
            }
            if (_worldAudioManager != null && _gameObjectManager.Player1 != null)
            {
                _worldAudioManager.UpdateListenerPosition(_gameObjectManager.Player1.Transform.Position);
            }
            _overlay.SetPaused(false, gameTime);
            _collisionManager.Update(_gameObjectManager.Player1, _itemManager.Items, _gameObjectManager.Projectiles, _gameObjectManager.AOEAttacks, _gameObjectManager.Characters);

            if (!_isTechDemo)
            {
                _spawnManager.Update(gameTime);
            }

            _gameObjectManager.Update(gameTime, InputHandler.MakeMouseWorldPosByCamera(camera));
            _itemManager.Update(gameTime);


            if (InputHandler.IsPressed(GameAction.DEBUG_HEAL))
                _gameObjectManager.Player1.Attributes.Health = _gameObjectManager.Player1.Attributes.MaxHealth;

            if (InputHandler.IsPressed(GameAction.TECH_DEMO))
            {
                _audioService.Sounds.StopAll();
                ScreenManager.AddScreen(new TechDemoScreen(_audioService));
            }

            if (InputHandler.IsPressed(GameAction.DEBUG_HITBOXES) && _isTechDemo)
            {
                _showStaticHitboxes = !_showStaticHitboxes;
            }

            if ((_gameObjectManager.Player1 != null) && _gameObjectManager.Player1.IsDead)
            {
                _audioService.Sounds.PauseAll();
                _audioService.Music.Stop();
                _overlay.SetPaused(true, gameTime);
                _audioService.Sounds.Play(AudioAssets.CarCrash);
                ScreenManager.AddScreen(new GameOverScreen(_font, _audioService, _statisticsManager.Statistic));
                _statisticsManager.SaveStatistic();
                SaveLoad.SaveNonGame(_statisticsManager);
            }

            _debugger.Update(gameTime);
            // Needs to be implemented elsewhere.
            if (InputHandler.IsPressed(GameAction.TOGGLE_CAMERA))
            {
                _freelook = !_freelook;
                _gameObjectManager.Player1.Immobalize(_freelook);
            }
            camera.Update(gameTime, _gameObjectManager.Player1.Transform.Position, _freelook);

            _tiledMapRenderer.Update(gameTime);
            HandleSaveLoadInput();


            if (InputHandler.IsPressed(GameAction.PAUSE))
            {
                _audioService.Sounds.PauseAll();
                _overlay.SetPaused(true, gameTime);
                PauseTimer();
                PauseMenuScreen pauseMenu = new PauseMenuScreen(_font, _audioService);
                ScreenManager.AddScreen(pauseMenu);
            }

            _gameTimer.Update(gameTime);
        }

        // Load here stuff like statistics or options that is not related to the
        // game and gameplay
        private void HandleLoadNonInGameData()
        {
            var state = SaveLoad.LoadGame();
            _statisticsManager.Statistics = state.Statistics;
        }

        public void HandleLoadGame()
        {
            var state = SaveLoad.LoadGame();
            _gameObjectManager.Player1.Transform.Position = new Vector2(state.PlayerX, state.PlayerY);
            _gameTimer.SetFromSave(state.GameTimerCurrentTime, state.IsGameTimerRunning, state.IsGameTimerPaused);

            _gameObjectManager.Projectiles.Clear();
            foreach (var p in state.Projectiles)
            {
                if (p.Basic.Type == SaveLoad.TYPES.BULLET)
                {
                    Bullet b = new Bullet(p.Basic.Position.ToVector2(), p.Basic.Size.ToPoint(), _gameObjectManager.Player1); // TODO player doesn't make sense here
                    b.HasHit = p.HasHit;
                    b.Movement.IsMoving = p.IsMoving;
                    b.Movement.CanMove = p.CanMove;
                    b.Movement.Direction = p.Direction.ToVector2();
                    b.Movement.Rotation = p.Rotation;
                    _gameObjectManager.AddProjectile(b);
                }
            }
            _gameObjectManager.Characters.Clear();
            foreach (var p in state.Characters)
            {
                if (p.Type == SaveLoad.TYPES.HOBO)
                {
                    Hobo b = new Hobo(p.Position.ToVector2(), p.Size.ToPoint(), _audioService, _pathFinding,
                        _collisionManager);
                    _gameObjectManager.AddCharacter(b);
                }
                if (p.Type == SaveLoad.TYPES.BIKETHIEF)
                {
                    BikeThief b = new BikeThief(p.Position.ToVector2(), p.Size.ToPoint(), _audioService, _pathFinding,
                        _collisionManager);
                    _gameObjectManager.AddCharacter(b);
                }
                if (p.Type == SaveLoad.TYPES.DOG)
                {
                    Dog b = new Dog(p.Position.ToVector2(), p.Size.ToPoint(), _audioService, _pathFinding,
                        _collisionManager);
                    _gameObjectManager.AddCharacter(b);
                }
            }

            _gameObjectManager.Items.Clear();
            foreach (var p in state.Items)
            {
                if (p.Type == SaveLoad.TYPES.CHEST)
                {
                    Chest b = new Chest(p.Position.ToVector2(), p.Size.ToPoint());
                    _gameObjectManager.AddItem(b);
                }
                if (p.Type == SaveLoad.TYPES.BEER)
                {
                    Xp_Beer b = new Xp_Beer(p.Position.ToVector2(), p.Size.ToPoint());
                    _gameObjectManager.AddItem(b);
                }
                if (p.Type == SaveLoad.TYPES.MONEY)
                {
                    Xp_Money b = new Xp_Money(p.Position.ToVector2(), p.Size.ToPoint());
                    _gameObjectManager.AddItem(b);
                }
                if (p.Type == SaveLoad.TYPES.ENERGY_GEL)
                {
                    EnergyGel b = new EnergyGel(p.Position.ToVector2(), p.Size.ToPoint());
                    _gameObjectManager.AddItem(b);
                }
            }
            _statisticsManager.Statistic = new Statistic(state.Statistic.Kills, state.Statistic.DealtDamage, state.Statistic.TookDamage, state.Statistic.XP, state.Statistic.Level);
            Console.WriteLine("Game loaded.");
        }
        private void HandleSaveLoadInput()
        {
            if (InputHandler.IsPressed(GameAction.SAVE))
                SaveLoad.SaveGame(_gameTimer, _gameObjectManager, _statisticsManager);

            if (InputHandler.IsPressed(GameAction.LOAD))
            {
                HandleLoadGame();
            }

            if (InputHandler.IsPressed(GameAction.RESET))
            {
                // if R is pressed while in tech demo remove all characters exept the player
                if (_isTechDemo)
                {
                    _gameObjectManager.Characters.RemoveAll(
                        ch => ch != _gameObjectManager.Player1);

                    _gameObjectManager.Projectiles.Clear();

                    Console.WriteLine("Tech demo reset: removed all enemies and projectiles.");
                }
                else
                {

                    _gameObjectManager.Player1.Transform.Position =
                        new Vector2(worldBounds.Width / 2, worldBounds.Height / 2);
                    ResetGameTimer();
                    Console.WriteLine("Reset counter and player position.");
                }
            }
        }

        // draws the enemy path in the tech demo
        private void DrawEnemyPaths(SpriteBatch spriteBatch)
        {
            // Need EnemyMovement + Node + GridToWorldCenter
            foreach (var c in _gameObjectManager.Characters)
            {
                if (c.Movement is EnemyMovement em && em.CurrentPath != null)
                {
                    var path = em.CurrentPath;

                    // draw every 2nd node to reduce overdraw
                    for (int i = 0; i < path.Count; i += 2)
                    {
                        Node node = path[i];

                        // convert grid coords -> world pixel center
                        Vector2 worldPos = _collisionManager.GridToWorldCenter(node);

                        // tiny 2x2 blue dot
                        var rect = new Rectangle(
                            (int)worldPos.X - 1,
                            (int)worldPos.Y - 1,
                            2,
                            2
                        );

                        spriteBatch.Draw(_pixel, rect, Color.Blue);
                    }
                }
            }
        }

        public virtual void Draw(GameTime gameTime)
        {
            Game1 game = Game1.Instance;
            SpriteBatch spriteBatch = game.SpriteBatch;

            _tiledMapRenderer.Draw(camera.GetTransform());

            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetTransform());
            _gameObjectManager.Draw(spriteBatch);

            if (_isTechDemo && _showStaticHitboxes)
            {
                _collisionManager.DrawHitboxes(
                    spriteBatch,
                    _pixel,
                    _gameObjectManager.Player1,
                    _gameObjectManager.Characters,
                    _itemManager.Items,
                    _gameObjectManager.Projectiles,
                    _gameObjectManager.AOEAttacks
                );
            }

            // draws A* paths for enemies in tech demo
            if (_isTechDemo)
            {
                DrawEnemyPaths(spriteBatch);
            }

            spriteBatch.End();
            spriteBatch.Begin();

            _debugger.Draw(spriteBatch);
            DrawTimer(spriteBatch, gameTime);

            _gameObjectManager.Player1.Inventory.Draw(spriteBatch, _pixel);
            hud.Draw(spriteBatch, _gameObjectManager.Player1);

            if (_levelUpScreen.IsOpen)
            {
                _levelUpScreen.Draw(spriteBatch);
            }

            spriteBatch.End();
        }

        private Rectangle GetCameraWorldRect()
        {
            var game = Game1.Instance;
            int viewW = game.GraphicsDevice.Viewport.Width;
            int viewH = game.GraphicsDevice.Viewport.Height;

            // world half extents depend on zoom (analog zur Camera2D.ClampToWorld)
            float halfW = (viewW / 2f) / camera.Zoom;
            float halfH = (viewH / 2f) / camera.Zoom;

            float leftF = camera.Position.X - halfW;
            float topF = camera.Position.Y - halfH;

            return new Rectangle(
                (int)MathF.Floor(leftF),
                (int)MathF.Floor(topF),
                (int)MathF.Ceiling(halfW * 2f),
                (int)MathF.Ceiling(halfH * 2f)
            );
        }

        private void InitializeTimer()
        {
            _gameTimer.Restart(); // restart Timer
        }

        private void OnGameTimerFinished()
        {
            _audioService.Sounds.PauseAll();
            _audioService.Music.Stop();
            _overlay.SetPaused(true, Game1.CurrentGameTime);
            _audioService.Sounds.Play(AudioAssets.CarHorn);
            ScreenManager.AddScreen(new GameWonScreen(_font, _audioService, _statisticsManager.Statistic));
            _statisticsManager.SaveStatistic();
            SaveLoad.SaveNonGame(_statisticsManager);
        }

        private void DrawTimer(SpriteBatch spriteBatch, GameTime gameTime)
        {
            string timerText = _gameTimer.GetFormattedTime();

            // change timer_font collor, when time is running out
            Color timerColor = Color.White;
            if (_gameTimer.CurrentTime < 60f)
            {
                timerColor = Color.Yellow;
            }
            if (_gameTimer.CurrentTime < 30f)
            {
                timerColor = Color.Red;

                // blink when time is about to run out
                float blink = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 10) * 0.5f + 0.5f;
                timerColor = Color.Lerp(Color.Red, Color.White, blink);
            }

            // background
            Vector2 textSize = _timerFont.MeasureString(timerText);
            Vector2 centeredPosition = new Vector2(
                _timerPosition.X - textSize.X / 2f,
                _timerPosition.Y
            );

            Rectangle backgroundRect = new Rectangle(
                (int)centeredPosition.X - 5,
                (int)centeredPosition.Y - 2,
                (int)textSize.X + 10,
                (int)textSize.Y + 4
            );

            spriteBatch.Draw(_pixel, backgroundRect, Color.Black * 0.5f);

            spriteBatch.DrawString(_timerFont, timerText, centeredPosition, timerColor);
        }

        public void ResetGameTimer()
        {
            _gameTimer.Restart();
        }

        public void PauseTimer()
        {
            _gameTimer.Pause();
        }

        public void ResumeTimer()
        {
            _gameTimer.Resume();
        }

        public void Unload()
        {
            GameEvents.OnResumeTimer -= ResumeTimer;
        }

    }
}