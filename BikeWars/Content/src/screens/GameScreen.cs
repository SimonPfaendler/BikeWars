using System;
using System.Collections.Generic;
using BikeWars.Content.components;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.entities.items;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using BikeWars.Utilities;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.input;
using BikeWars.Content.src.screens.Overlay;
using BikeWars.Content.src.utils.SaveLoadExample;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled.Renderers;
using BikeWars.Content.managers;
using BikeWars.Content.events;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.entities.MapObjects;
using BikeWars.Entities.Characters.MapObjects;

namespace BikeWars.Content.screens
{
    public class GameScreen : IScreen
    {

        public static int AliveGameScreens = 0;
        private LevelUpScreen _levelUpScreen;
        private BikeShopScreen _bikeShopScreen;
        private Camera2D camera;
        private Rectangle worldBounds;
        private Overlay _overlay;
        private TiledMapRenderer _tiledMapRenderer;
        private Debugger _debugger;
        private bool _isAlive = true; // If the Screen is still alive. Necessary for the switch

        private Action<float, float> _onScreenShake;
        private Action<int, int> _onPlayerLevelUp;
        private Action _onBikeShopClose;

        private Action<BikeShop> _onBikeShopOpen;
        public Viewport ViewPort {get; set;}
        public event Action<int, IScreen> BtnClicked;
        public event Action PauseBtnPressed;
        public event Action<Statistic> GameOver;
        public event Action<Statistic> GameWon;
        public event Action<IScreen> StartTechDemo;
        private Action _onGameOverExit;
        private const int CELL_SIZE = 16;
        private WorldAudioManager _worldAudioManager;

        private StatisticsManager _statisticsManager {get; set;}
        public StatisticsManager StatisticsManager => _statisticsManager;
        private readonly AudioService _audioService;
        public AudioService AudioService => _audioService;

        private PlayerManager _playerManager;
        public PlayerManager PlayerManager => _playerManager;

        public string DesiredMusic => AudioAssets.GameMusic;
        public float MusicVolume => 1f;

        private HUD _hud;
        private HUD _hudP2;
        private Texture2D hudTexture;

        private CollisionManager _collisionManager;
        private GameObjectManager _gameObjectManager;
        public GameObjectManager GameObjectManager => _gameObjectManager;

        private CombatManager _combatManager;

        protected SpawnManager _spawnManager;

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
        private const float GAME_TIME_LIMIT = 3f;
        private SpriteFont _timerFont;
        private Vector2 _timerPosition;
        private readonly GameMode _gameMode;
        public GameMode GameMode => _gameMode;
        public bool IsMultiplayer => _gameMode == GameMode.MultiPlayer; // might be helpful later
        private InputMode _inputMode = InputMode.Keyboard;

        private float _hitStopTimer = 0f;

        private RepathScheduler _repathScheduler;
        protected RepathScheduler RepathScheduler => _repathScheduler;

        private bool _musicOverrideActive = false;
        private Musicians _activeMusicianOverride = null;
        private float _musicOverrideDelayTimer = 0f;
        private bool _waitingForMetal = false;

        private const float METAL_DELAY_SECONDS = 2f;

        private int _musicianDamageCircleCount = 0;
        private float _musicianDamageCircleTimer = 0f;
        private const int MUSICIAN_DAMAGE_CIRCLE_TOTAL = 3;
        private const float MUSICIAN_DAMAGE_CIRCLE_INTERVAL = 3f;
        private Musicians? _activeMusiciansForAOE = null;

        public event Action Exit;

        public void TriggerHitStop(float duration)
        {
            _hitStopTimer = duration;
        }

        public GameScreen(AudioService audioService, GameMode gameMode, bool isTechDemo = false)
        {
            AliveGameScreens++;
            Console.WriteLine("GameScreen created: " + AliveGameScreens);
            _isTechDemo = isTechDemo;

            worldBounds = new Rectangle(0, 0, 11200, 11200);

            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _gameMode = gameMode;


        }
        public virtual void LoadContent(ContentManager content, GraphicsDevice gd)
        {
            // _contentManager = content; // We need this to add it later to spawning entities. (Maybe there is another possible implementation)
            // Content = content;
            // Font and Debugger
            _playerManager = new PlayerManager(ViewPort, _gameMode, worldBounds, _audioService, _isTechDemo);
            camera = _playerManager.Camera;
            Player player = _playerManager.Player1;
            Player player2 = _playerManager.Player2;

            _gameObjectManager = new GameObjectManager(content, player, player2);
            _debugger = new Debugger(_gameObjectManager.Player1);
            // Initial spawning is now handled by SpawnManager

            _gameObjectManager.AddItem(new Frelo(new Vector2(5700, 5700), new Point(32, 32)));
            _gameObjectManager.AddItem(new RacingBike(new Vector2(5800, 5800), new Point(32, 32)));
            string energy = "Energygel";
            _gameObjectManager.AddObject(new Chest(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 + 50), new Point(32, 32), energy));
            string doping = "DopingSpritze";
            _gameObjectManager.AddObject(new Chest(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 + 90), new Point(32, 32), doping));

            _gameObjectManager.AddTower(new TowerAlly(new Vector2(5600, 5750), new Point(128, 128), _audioService));
            _freelook = false;
            // camera.Position is set by Update usually, but let's init it
            if (player2 == null)
            {
                camera.Position = player.Transform.Position;
            } else
            {
                camera.Position = Maths.Middle(player.Transform.Position, player2.Transform.Position);
            }

            _statisticsManager = new StatisticsManager();
            _gameTimer = new GameTimer(GAME_TIME_LIMIT);

            _gameObjectManager.OnCharacterDied += _statisticsManager.HandleCharacterDied;
            _gameObjectManager.OnTookDamage += _statisticsManager.HandleTookDamage;
            _gameObjectManager.Player1.OnTookDamage += _statisticsManager.HandleTookDamage;
            _gameObjectManager.Player1.OnLevelUp += _statisticsManager.HandleLevel;
            _gameObjectManager.Player1.OnMoreXP += _statisticsManager.HandleExperience;

            _collisionManager = new CollisionManager(CELL_SIZE, worldBounds.Height, _gameObjectManager);
            var players = new HashSet<Player>();
            if (_gameObjectManager.Player1 != null) players.Add(_gameObjectManager.Player1);
            if (_gameObjectManager.Player2 != null) players.Add(_gameObjectManager.Player2);

            _collisionManager.Insertions(_gameObjectManager.Items, players, _gameObjectManager.Projectiles, _gameObjectManager.AOEAttacks, _gameObjectManager.Characters, new List<Tram>(), _gameObjectManager.Objects, _gameObjectManager.Towers);

            _onGameOverExit = OnExit;
            // _gameOverScreen = new GameOverScreen(UIAssets.DefaultFont, _audioService, _statisticsManager.Statistic, ViewPort);
            // _gameOverScreen.Exit += _onGameOverExit;

            GameEvents.OnResumeTimer += ResumeTimer;
            HandleLoadNonInGameData();
            _gameTimer.OnTimerFinished += OnGameTimerFinished;

            // Tiled Map
            _collisionManager.LoadContent(content);

            // pathfinding object
            _pathFinding = new PathFinding(_collisionManager.PathGrid);

            // Pathfinding scheduler (limits how many enemies may repath per frame)
            _repathScheduler = new RepathScheduler(capacity: 2000)
            {
                UpdateMaxEnemies = 120
            };

            _tiledMapRenderer = new TiledMapRenderer(gd, _collisionManager.TiledMap);
            // Create Combat Manager
            _combatManager = new CombatManager(_audioService, _gameObjectManager);

            // Combat Manager subcribes to Events from Collision Manager:  Collision → Combat
            _collisionManager.OnProjectileHit += _combatManager.HandleProjectileHit;
            _collisionManager.OnAOEHit += _combatManager.HandleAOEHit;
            _collisionManager.OnCharacterCollision += _combatManager.HandleCharacterCollision;
            _collisionManager.OnItemPickup += _gameObjectManager.Player1.OnPickUpItem;
            _collisionManager.OnTowerInteraction += _gameObjectManager.OnActivateTower;
            _collisionManager.OnObjectInteraction += _gameObjectManager.Player1.OnInteractObject;
            _gameObjectManager.Player1.ItemPickedUp += _collisionManager.OnRemoveItem;
            _collisionManager.OnTramHit += _combatManager.HandleTramHit;

            _combatManager.OnHitStopRequested += TriggerHitStop;
            _onScreenShake = (intensity, duration) => camera.Shake(intensity, duration);
            _combatManager.OnScreenShakeRequested += _onScreenShake;
            _gameObjectManager.OnScreenShakeRequested += _onScreenShake;

            if (_gameObjectManager.Player2 != null)
            {
                _collisionManager.OnItemPickup += _gameObjectManager.Player2.OnPickUpItem;
                _collisionManager.OnObjectInteraction += _gameObjectManager.Player2.OnInteractObject;
                _gameObjectManager.Player2.ItemPickedUp += _collisionManager.OnRemoveItem;
            }

            // Overlay
            _overlay = new Overlay();

            // HUD
            hudTexture = SpriteManager.GetTexture("HUD_Sheet");
            _hud = new HUD();
            _hudP2 = new HUD();
            _hud.LoadContent(hudTexture);
            _hudP2.LoadContent(hudTexture);

            // Position P2 HUD at bottom right
            int viewW = ViewPort.Width;
            int viewH = ViewPort.Height;
            _hudP2.Position = new Vector2(viewW - 350, viewH - 170);

            _gameObjectManager.LoadContent(content);

             // sound control
            Rectangle initialView = GetCameraWorldRect();
            _worldAudioManager = new WorldAudioManager(initialView);
            _gameObjectManager.SetWorldAudioManager(_worldAudioManager);

            _levelUpScreen = new LevelUpScreen(
                UIAssets.DefaultFont,
                "Level UP!",
                _audioService,
                ViewPort
            );
            _levelUpScreen.Closed += () =>
            {
                _audioService.Sounds.ResumeAll();
            };


            // checks if the event OnLevelUp is triggered if it is LevelUpSreen gets active
            _onPlayerLevelUp = (xp, amount) =>
            {
                _audioService.Sounds.PauseAll();
                _levelUpScreen.Open(_gameObjectManager.Player1);
            };
            _gameObjectManager.Player1.OnLevelUp += _onPlayerLevelUp;

            _bikeShopScreen = new BikeShopScreen(ViewPort);

            _onBikeShopClose = () =>
            {
                _audioService.Sounds.ResumeAll();
            };
            _bikeShopScreen.Closed += _onBikeShopClose;

            _onBikeShopOpen += shop =>
            {
                _audioService.Sounds.PauseAll();
                _bikeShopScreen.Open(_gameObjectManager.Player1, shop);
            };

            _gameObjectManager.Player1.OnBikeShopOpen += _onBikeShopOpen;
            _gameObjectManager.Player1.Dismounted += _gameObjectManager.AddItem;
            _gameObjectManager.Player1.ChestItemSpawn += _gameObjectManager.AddItem;


            // the Option selected gets upgraded
            _levelUpScreen.OnOptionSelected += skillId =>
            {
                _gameObjectManager.Player1.UpgradeSkill(skillId);
            };

            // Spawn Manager
            _spawnManager = new SpawnManager(_gameObjectManager, _collisionManager, _audioService, _pathFinding, _repathScheduler, _worldAudioManager);


            // timer
            _timerFont = content.Load<SpriteFont>("assets/fonts/Arial");
            _timerPosition = new Vector2(
                viewW / 2f,
                40f
            );
            if (!_gameTimer.IsRunning && !_isTechDemo)
            {
                InitializeTimer();
            }
        }
        public virtual void Update(GameTime gameTime)
        {
            if (!_isAlive)
            {
                return;
            }
            // Update HUD and Timer alignment for resolution changes
            int viewW = ViewPort.Width;
            int viewH = ViewPort.Height;

            if (_hudP2 != null)
            {
                _hudP2.Position = new Vector2(viewW - 350, viewH - 170);
            }
            _timerPosition = new Vector2(viewW / 2f, 40f);

            if (InputHandler.IsPressed(GameAction.MODE_SWITCH))
            {
                if (_inputMode == InputMode.Keyboard)
                {
                    _inputMode = InputMode.Controller;
                    // Strict Controller Mode for Player1 on Pad 1
                    _gameObjectManager.Player1.SetInput(new GamepadPlayerInput(PlayerIndex.One));
                }
                else
                {
                    _inputMode = InputMode.Keyboard;
                    _gameObjectManager.Player1.SetInput(new KeyboardPlayerInput(camera));
                }
                Console.WriteLine("Input mode switched to: " + _inputMode);
            }

            // if the LevelUp is Open only the LevelUpMenu gets Updated all the other stuff is basically paused
            // if you want to add something before this or change order please double-check
            if (_levelUpScreen.IsOpen)
            {
                _levelUpScreen.Update(gameTime);
                return;
            }
            if (_bikeShopScreen.IsOpen)
            {
                _bikeShopScreen.Update(gameTime);
                return;
            }
            if (_worldAudioManager != null && _gameObjectManager.Player1 != null)
            {
                _worldAudioManager.UpdateListenerPosition(_gameObjectManager.Player1.Transform.Position);
            }

            _overlay.SetPaused(false, gameTime);

            // Hit Stop Logic
            if (_hitStopTimer > 0f)
            {
                _hitStopTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_hitStopTimer > 0f)
                {
                    // Skip updates for game objects and collision to simulate pause
                     _tiledMapRenderer.Update(gameTime); // keep map rendering updating
                    return;
                }
            }

            var players = new HashSet<Player>();
            if (_gameObjectManager.Player1 != null) players.Add(_gameObjectManager.Player1);
            if (_gameObjectManager.Player2 != null) players.Add(_gameObjectManager.Player2);

            if (!_isTechDemo)
            {
                _spawnManager.Update(gameTime);
            }

            // Let up to 50 enemies recalc their paths this frame
            _repathScheduler?.Update();
            _gameObjectManager.Update(gameTime, InputHandler.MakeMouseWorldPosByCamera(camera));
            _collisionManager.Update(players, _gameObjectManager.Items, _gameObjectManager.Projectiles, _gameObjectManager.AOEAttacks, _gameObjectManager.Characters, new List<Tram>(_gameObjectManager.Trams), _gameObjectManager.Objects, _gameObjectManager.Towers);

            if (InputHandler.IsPressed(GameAction.DEBUG_HEAL))
                _gameObjectManager.Player1.Attributes.Health = _gameObjectManager.Player1.Attributes.MaxHealth;

            if (InputHandler.IsPressed(GameAction.TECH_DEMO))
            {
                _audioService.Sounds.StopAll();
                StartTechDemo?.Invoke(this);
            }

            if (InputHandler.IsPressed(GameAction.DEBUG_HITBOXES) && _isTechDemo)
            {
                _showStaticHitboxes = !_showStaticHitboxes;
            }

            if ((_gameObjectManager.Player1 != null && _gameObjectManager.Player1.IsDead) || (_gameObjectManager.Player2 != null && _gameObjectManager.Player2.IsDead))
            {
                _audioService.Sounds.PauseAll();
                _audioService.Music.Stop();
                _overlay.SetPaused(true, gameTime);
                _audioService.Sounds.Play(AudioAssets.CarCrash);
                GameOver?.Invoke(_statisticsManager.Statistic);
                _statisticsManager.SaveStatistic();
                SaveLoad.SaveNonGame(_statisticsManager);
            }

            _debugger?.Update(gameTime);
            // Needs to be implemented elsewhere.
            if (InputHandler.IsPressed(GameAction.TOGGLE_CAMERA))
            {
                _freelook = !_freelook;
                _gameObjectManager.Player1.Immobalize(_freelook);
            }
            if (_gameObjectManager.Player2 == null)
            {
                camera?.Update(gameTime, _gameObjectManager.Player1.Transform.Position, null, _freelook);
            } else
            {
                camera?.Update(gameTime, _gameObjectManager.Player1.Transform.Position, _gameObjectManager.Player2.Transform.Position, _freelook);
            }

            _tiledMapRenderer?.Update(gameTime);
            HandleSaveLoadInput();

            if (InputHandler.IsPressed(GameAction.PAUSE))
            {
                _audioService.Sounds.PauseAll();
                _overlay.SetPaused(true, gameTime);
                PauseTimer();
                PauseBtnPressed?.Invoke();
            }

            if (!_isTechDemo)
                _gameTimer.Update(gameTime);

            // check if Musicians are nearby and change Music if it's the case

            bool playerNearMusicians = false;

            foreach (var obj in _gameObjectManager.Objects)
            {
                if (obj is Musicians musicians)
                {
                    if (musicians.IsPlayerNearby(_gameObjectManager.Player1.Transform.Position) ||
                        (_gameObjectManager.Player2 != null && musicians.IsPlayerNearby(_gameObjectManager.Player2.Transform.Position)))
                    {
                        playerNearMusicians = true;
                        break;
                    }
                }
            }

            // logic for interaction with musicians (music change and attack)
            if (!_musicOverrideActive)
            {
                foreach (var obj in _gameObjectManager.Objects)
                {
                    if (obj is not Musicians musicians)
                        continue;

                    bool p1Interact =
                        musicians.Intersects(_gameObjectManager.Player1.Collider) &&
                        _gameObjectManager.Player1.IsInteractPressed();

                    bool p2Interact = false;

                    if (_gameObjectManager.Player2 != null)
                    {
                        p2Interact =
                            musicians.Intersects(_gameObjectManager.Player2.Collider) &&
                            _gameObjectManager.Player2.IsInteractPressed();
                    }

                    if ((p1Interact || p2Interact) && musicians.TryTriggerOverride())
                    {
                        StartMusicOverride(musicians);
                        _activeMusiciansForAOE = musicians;
                        _musicianDamageCircleCount = 0;
                        _musicianDamageCircleTimer = MUSICIAN_DAMAGE_CIRCLE_INTERVAL;

                        break;
                    }
                }
            }

            if (_activeMusiciansForAOE != null && _musicianDamageCircleCount < MUSICIAN_DAMAGE_CIRCLE_TOTAL)
            {
                _musicianDamageCircleTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_musicianDamageCircleTimer <= 0f)
                {
                    // spawn DamageCircle
                    Vector2 offset = new Vector2(_activeMusiciansForAOE.Transform.Size.X / 2f,
                        _activeMusiciansForAOE.Transform.Size.Y / 2f);

                    Transform dcTransform = new Transform(_activeMusiciansForAOE.Transform.Position + offset,
                        _activeMusiciansForAOE.Transform.Size);

                    DamageCircle dc = new DamageCircle(
                        dcTransform,
                        owner: null,
                        damagePlayers: false
                    );

                    dc.LoadContent(_gameObjectManager._contentManager);
                    _gameObjectManager.AddAOE(dc);

                    _musicianDamageCircleCount++;
                    _musicianDamageCircleTimer = MUSICIAN_DAMAGE_CIRCLE_INTERVAL;
                }
            }


            if (_musicOverrideActive)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_waitingForMetal)
                {
                    _musicOverrideDelayTimer -= dt;

                    if (_musicOverrideDelayTimer <= 0f)
                    {
                        _waitingForMetal = false;

                        _audioService.Music.Play(AudioAssets.Metal, isRepeating: false);
                    }

                    return;
                }

                if (!_audioService.Music.IsPlaying)
                {
                    _musicOverrideActive = false;

                    if (_activeMusicianOverride != null)
                    {
                        _activeMusicianOverride.ResetOverride();
                        _activeMusicianOverride = null;
                    }
                }

                return;
            }


            if (playerNearMusicians)
            {
                if (_audioService.Music.CurrentSong != AudioAssets.LatinMusic)
                {
                    _audioService.Music.PlayWithFade(AudioAssets.LatinMusic, true);
                }
            }
            else
            {
                if (_audioService.Music.CurrentSong == AudioAssets.LatinMusic)
                {
                    _audioService.Music.PlayWithFade(AudioAssets.GameMusic, true);
                }
            }
        }

        // Load here stuff like statistics or options that is not related to the
        // game and gameplay
        private void HandleLoadNonInGameData()
        {
            var state = SaveLoad.LoadGame();

            if (state.Statistics != null)
                _statisticsManager.Statistics = state.Statistics;
            else
                _statisticsManager.Statistics = new List<Statistic>();
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
                        _collisionManager, _repathScheduler);
                    _gameObjectManager.AddCharacter(b);
                }
                if (p.Type == SaveLoad.TYPES.BIKETHIEF)
                {
                    BikeThief b = new BikeThief(p.Position.ToVector2(), p.Size.ToPoint(), _audioService, _pathFinding,
                        _collisionManager, _repathScheduler);
                    _gameObjectManager.AddCharacter(b);
                }
                if (p.Type == SaveLoad.TYPES.DOG)
                {
                    Dog b = new Dog(p.Position.ToVector2(), p.Size.ToPoint(), _audioService, _pathFinding,
                        _collisionManager, _repathScheduler);
                    _gameObjectManager.AddCharacter(b);
                }
            }

            _gameObjectManager.Items.Clear();
            foreach (var p in state.Items)
            {
                Vector2 pos = p.Position.ToVector2();
                Point size = p.Size.ToPoint();

                if (p.Type == SaveLoad.TYPES.BEER)
                {
                    _gameObjectManager.AddItem(new Xp_Beer(pos, size));
                }
                else if (p.Type == SaveLoad.TYPES.MONEY)
                {
                    _gameObjectManager.AddItem(new Xp_Money(pos, size));
                }
                else if (p.Type == SaveLoad.TYPES.ENERGY_GEL)
                {
                    _gameObjectManager.AddItem(new EnergyGel(pos, size));
                }
                else if (p.Type == SaveLoad.TYPES.FRELO)
                {
                    _gameObjectManager.AddItem(new Frelo(pos, size));
                }
                else if (p.Type == SaveLoad.TYPES.RACINGBIKE)
                {
                    _gameObjectManager.AddItem(new RacingBike(pos, size));
                }
            }
            _gameObjectManager.Objects.Clear();
            foreach (var o in state.Objects)
            {
                var pos = o.Position.ToVector2();
                var size = o.Size.ToPoint();

                if (o.Type == SaveLoad.TYPES.CHEST)
                {
                    _gameObjectManager.AddObject(new Chest(pos, size, o.Item, o.IsOpen ?? false));
                }
                else if (o.Type == SaveLoad.TYPES.BIKESHOP)
                { _gameObjectManager.AddObject(new BikeShop(pos, size));}
                else if (o.Type == SaveLoad.TYPES.DOGBOWL)
                {
                    _gameObjectManager.AddObject(new DogBowl(pos, size, full: o.IsFull ?? false));
                }
            }
            _statisticsManager.Statistic = new Statistic(state.Statistic.Kills, state.Statistic.DealtDamage, state.Statistic.TookDamage, state.Statistic.XP, state.Statistic.Level);
            Console.WriteLine("Game loaded.");
        }
        private void HandleSaveLoadInput()
        {
            if (InputHandler.IsPressed(GameAction.SAVE))
                SaveLoad.SaveGame(_gameTimer, _gameObjectManager, _statisticsManager, _gameMode);

            if (InputHandler.IsPressed(GameAction.LOAD))
            {
                HandleLoadGame();
            }

            if (InputHandler.IsPressed(GameAction.RESET))
            {
                // if R is pressed while in tech demo remove all characters exept the player
                if (_isTechDemo)
                {
                    // remove everything except player(s)
                    _gameObjectManager.Characters.RemoveAll(ch =>
                        ch != _gameObjectManager.Player1 &&
                        ch != _gameObjectManager.Player2
                    );
                    _gameObjectManager.Projectiles.Clear();

                    OnTechDemoReset();

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

        protected virtual void OnTechDemoReset()
        {
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

                        spriteBatch.Draw(RenderPrimitives.Pixel, rect, Color.Blue);
                    }
                }
            }
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch sb)
        {
            _tiledMapRenderer.Draw(camera.GetTransform());

            sb.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetTransform());
            _gameObjectManager.Draw(sb);
            if (_isTechDemo && _showStaticHitboxes)
            {
                _collisionManager.DrawHitboxes(
                    sb,
                    RenderPrimitives.Pixel,
                    _gameObjectManager.Player1,
                    _gameObjectManager.Characters,
                    _gameObjectManager.Items,
                    _gameObjectManager.Projectiles,
                    _gameObjectManager.AOEAttacks,
                    new List<Tram>(_gameObjectManager.Trams),
                    _gameObjectManager.Objects
                );
            }

            // draws A* paths for enemies in tech demo
            if (_isTechDemo && _showStaticHitboxes)
            {
                DrawEnemyPaths(sb);
            }

            sb.End();
            sb.Begin();

            _debugger.Draw(sb);
            DrawTimer(sb, gameTime);

            var player = _gameObjectManager.Player1;
            bool showSelection = (_inputMode == InputMode.Controller);
            player.Inventory.Draw(sb, RenderPrimitives.Pixel, player.SelectedInventoryIndex, showSelection);
            _hud.Draw(sb, _gameObjectManager.Player1);

            if (_gameObjectManager.Player2 != null)
            {
                _hudP2.Draw(sb, _gameObjectManager.Player2);
            }

            if (_levelUpScreen.IsOpen)
            {
                _levelUpScreen.Draw(gameTime, sb);
            }
            if (_bikeShopScreen.IsOpen)
            {
                _bikeShopScreen.Draw(gameTime, sb);
            }

            sb.End();
        }

        public virtual void Dispose()
        {
            Unload();
            _tiledMapRenderer?.Dispose();
            _isAlive = false;
            // hudTexture?.Dispose();

            _hud = null;
            _hudP2 = null;
            _debugger = null;
            camera = null;
        }

        private Rectangle GetCameraWorldRect()
        {
            int viewW = ViewPort.Width;
            int viewH = ViewPort.Height;

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
            // ScreenManager.AddScreen(new GameWonScreen(UIAssets.DefaultFont, _audioService, _statisticsManager.Statistic));
            GameWon?.Invoke(_statisticsManager.Statistic);
            _statisticsManager.SaveStatistic();
            SaveLoad.SaveNonGame(_statisticsManager);
        }

        private void DrawTimer(SpriteBatch spriteBatch, GameTime gameTime)
        {
            string timerText = _gameTimer.GetFormattedTime();

            // change timerUIAssets.DefaultFont collor, when time is running out
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

            spriteBatch.Draw(RenderPrimitives.Pixel, backgroundRect, Color.Black * 0.5f);

            spriteBatch.DrawString(_timerFont, timerText, centeredPosition, timerColor);
        }

        // Pass it to Game1 to Exit the Game
        private void OnExit()
        {
            Exit?.Invoke();
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
            AliveGameScreens--;
            Console.WriteLine("GameScreen unloaded: " + AliveGameScreens);
            // 🔴 STATIC EVENTS
            GameEvents.OnResumeTimer -= ResumeTimer;

            // 🔴 Player Events
            if (_gameObjectManager?.Player1 != null)
            {
                _gameObjectManager.OnCharacterDied -= _statisticsManager.HandleCharacterDied;
                _gameObjectManager.OnTookDamage -= _statisticsManager.HandleTookDamage;
                _gameObjectManager.Player1.OnTookDamage -= _statisticsManager.HandleTookDamage;
                _gameObjectManager.Player1.OnLevelUp -= _statisticsManager.HandleLevel;
                _gameObjectManager.Player1.OnMoreXP -= _statisticsManager.HandleExperience;
            }

            // 🔴 Collision / Combat
            if (_collisionManager != null)
            {
                _collisionManager.OnProjectileHit -= _combatManager.HandleProjectileHit;
                _collisionManager.OnAOEHit -= _combatManager.HandleAOEHit;
                _collisionManager.OnCharacterCollision -= _combatManager.HandleCharacterCollision;
                _collisionManager.OnItemPickup -= _gameObjectManager.Player1.OnPickUpItem;
                _gameObjectManager.Player1.ItemPickedUp -= _collisionManager.OnRemoveItem;
                _collisionManager.OnTramHit -= _combatManager.HandleTramHit;
            }

            if (_combatManager != null)
            {
                _combatManager.OnHitStopRequested -= TriggerHitStop;
                _combatManager.OnScreenShakeRequested -= _onScreenShake;
                _gameObjectManager.OnScreenShakeRequested -= _onScreenShake;
            }

            _gameObjectManager.Player1.OnLevelUp -= _onPlayerLevelUp;
            _gameObjectManager.Player1.OnBikeShopOpen -= _onBikeShopOpen;
            _bikeShopScreen.Closed -= _onBikeShopClose;

            _gameTimer.OnTimerFinished -= OnGameTimerFinished;

            _hud?.Dispose();
            _hud = null;
            _hudP2?.Dispose();
            _hudP2 = null;

            // 🔴 Overlay / Screens
            _levelUpScreen?.Unload();
            _bikeShopScreen?.Unload();

            // 🔴 Manager cleanup
            _spawnManager?.Dispose();
            _worldAudioManager?.Dispose();

            _collisionManager.Unload();
            _gameObjectManager.Unload();

            Console.WriteLine("GameScreen unloaded cleanly");
        }
        public enum InputMode
        {
            Keyboard,
            Controller
        }

        private void StartMusicOverride(Musicians musicians)
        {
            _musicOverrideActive = true;
            _activeMusicianOverride = musicians;

            _audioService.Music.Stop();

            _audioService.Sounds.Play(AudioAssets.VinylStop);

            _musicOverrideDelayTimer = METAL_DELAY_SECONDS;
            _waitingForMetal = true;
        }

        public void OnActivated()
        {
            // throw new NotImplementedException();
        }

        private void OnPauseMenuClicked(int id, IScreen screen)
        {

        }
    }
}