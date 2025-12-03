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

namespace BikeWars.Content.screens
{
    public class GameScreen : IScreen
    {
        private readonly ItemManager _itemManager;
        private readonly Camera2D camera;
        private Rectangle worldBounds;
        private Overlay _overlay;
        private TiledMapRenderer _tiledMapRenderer;
        private SpriteFont _debugFont;
        private Debugger _debugger;
        private int _counter = 0;
        private float _counterTimer = 0;
        private SpriteFont _font;
        private Texture2D _pixel;
        private const int CELL_SIZE = 16;
        public ScreenManager ScreenManager { get; set; }
        private WorldAudioManager _worldAudioManager;

        private ContentManager _contentManager;
        public ContentManager ContentManager => _contentManager;
        private readonly AudioService _audioService;
        public AudioService AudioService => _audioService;
        public string DesiredMusic => AudioAssets.GameMusic;
        public float MusicVolume => 1f;

        private HUD hud;
        private Texture2D hudTexture;
        private float hpTestTimer = 0f;

        private CollisionManager _collisionManager;
        private GameObjectManager _gameObjectManager;
        public GameObjectManager GameObjectManager => _gameObjectManager;

        private CombatManager _combatManager;

        private bool _freelook; // Has to be optimized

        public bool DrawLower => false;
        public bool UpdateLower => false;

        // bool that checks if you're in the tech demo or normal gameplay
        private readonly bool _isTechDemo;
        private bool _showStaticHitboxes = true;

        public GameScreen(AudioService audioService, bool isTechDemo = false)
        {

            _isTechDemo = isTechDemo;

            worldBounds = new Rectangle(0, 0, 11200, 11200);

            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));

            _itemManager = new ItemManager();
            _itemManager.AddItem(new Item(new Vector2(worldBounds.Width / 2 + 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
            _itemManager.AddItem(new Chest(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
            _itemManager.AddItem(new Xp_Beer(new Vector2(worldBounds.Width / 2 + 50, worldBounds.Height / 2 - 50), new Point(32, 32)));
            _itemManager.AddItem(new Xp_Money(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 - 50), new Point(32, 32)));
            _collisionManager = new CollisionManager(CELL_SIZE, worldBounds.Height);
            Player player = new Player(new Vector2(worldBounds.Width / 2, worldBounds.Height / 2), new Point(32, 32), _audioService);

            // if we are in the tech demo it transforms the player in god mode
            if (_isTechDemo)
            {
                player.IsGodMode = true;
            }

            _gameObjectManager = new GameObjectManager(_contentManager, player, null);
            for (int i = 0; i < 5; i++)
            {
                _gameObjectManager.AddCharacter(new Hobo(new Vector2(worldBounds.Width / 2 + i*40, worldBounds.Height / 2 -500 + i), new Point(32, 32), _audioService));
            }
            _gameObjectManager.AddCharacter(new BikeThief(new Vector2(worldBounds.Width / 2 - 100, worldBounds.Height / 2 - 80), new Point(32, 32), _audioService));
            _gameObjectManager.Items = _itemManager.Items;

            Game1 game = Game1.Instance;
            camera = new Camera2D(
                game.GraphicsDevice.Viewport.Width,
                game.GraphicsDevice.Viewport.Height,
                worldBounds
            );
            _freelook = false;
            camera.Position = _gameObjectManager.Player1.Transform.Position;

            // Create SaveLoad and load saved data
            var state = SaveLoad.LoadGame();
            _counter = state.Counter;
            _gameObjectManager.Player1.Transform.Position = new Vector2(state.PlayerX, state.PlayerY);
            Console.WriteLine("Loaded saved position (or default if no file).");
        }

        public virtual void LoadContent(ContentManager content)
        {
            // Font and Debugger

            _debugFont = content.Load<SpriteFont>("assets/fonts/Arial");
            _debugger = new Debugger(_debugFont, _gameObjectManager.Player1);

            // Tiled Map
            _collisionManager.LoadContent(content);
            _tiledMapRenderer = new TiledMapRenderer(Game1.Instance.GraphicsDevice, _collisionManager.TiledMap);

            // Create Combat Manager
            _combatManager = new CombatManager(_audioService);

            // Combat Manager subcribes to Events from Collision Manager:  Collision → Combat
            _collisionManager.OnProjectileHit += _combatManager.HandleProjectileHit;
            _collisionManager.OnCharacterCollision += _combatManager.HandleCharacterCollision;

            // Overlay
            _overlay = new Overlay(_debugFont, Game1.Instance.GraphicsDevice);

            // HUD
            hudTexture = content.Load<Texture2D>("assets/sprites/HUD/HUD_sheet");
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

        }
        public virtual void Update(GameTime gameTime)
        {
            if (_worldAudioManager != null && _gameObjectManager.Player1 != null)
            {
                _worldAudioManager.UpdateListenerPosition(_gameObjectManager.Player1.Transform.Position);
            }
            _overlay.SetPaused(false, gameTime);
            InputHandler.Update();
            _collisionManager.Update(_gameObjectManager.Player1, _itemManager.Items, _gameObjectManager.Projectiles, _gameObjectManager.Characters);

            _gameObjectManager.Update(gameTime, InputHandler.MakeMouseWorldPosByCamera(camera));
            _itemManager.Update(gameTime, _gameObjectManager.Player1);
            hpTestTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (InputHandler.IsPressed(GameAction.DEBUG_HEAL))
                _gameObjectManager.Player1.Health = _gameObjectManager.Player1.MaxHealth;

            if (InputHandler.IsPressed(GameAction.TECH_DEMO))
            {
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
                ScreenManager.AddScreen(new GameOverScreen(_font, _audioService));
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
            HandleCounter(gameTime);
            HandleSaveLoadInput();


            if (InputHandler.IsPressed(GameAction.PAUSE))
            {
                _audioService.Sounds.PauseAll();
                _overlay.SetPaused(true, gameTime);
                PauseMenuScreen pauseMenu = new PauseMenuScreen(_font, _audioService);
                ScreenManager.AddScreen(pauseMenu);
            }
        }

        private void HandleCounter(GameTime gameTime)
        {
            _counterTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_counterTimer >= 1)
            {
                _counter++;
                _counterTimer = 0;
            }
        }

        private void HandleSaveLoadInput()
        {
            if (InputHandler.IsPressed(GameAction.SAVE))
                // SaveLoad.SaveGame(_counter, _gameObjectManager.Player1.Transform, _gameObjectManager.Projectiles);
                SaveLoad.SaveGame(_counter, _gameObjectManager);

            if (InputHandler.IsPressed(GameAction.LOAD))
            {
                var state = SaveLoad.LoadGame();
                _counter = state.Counter;
                _counterTimer = 0;
                _gameObjectManager.Player1.Transform.Position = new Vector2(state.PlayerX, state.PlayerY);
                _gameObjectManager.Projectiles.Clear();
                foreach (var p in state.Projectiles)
                {
                    if (p.Type == SaveLoad.TYPES.BULLET) // Hier könnte man noch was machen.
                    {
                        Bullet b = new Bullet(p.Position.ToVector2(), p.Size.ToPoint(), _gameObjectManager.Player1); // TODO player doesn't make sense here
                        b.LoadContent(_contentManager);
                        _gameObjectManager.AddProjectile(b);
                    }
                }
                _gameObjectManager.Characters.Clear();
                foreach (var p in state.Characters)
                {
                    if (p.Type == SaveLoad.TYPES.HOBO) // Hier könnte man noch was machen.
                    {
                        Hobo b = new Hobo(p.Position.ToVector2(), p.Size.ToPoint(), _audioService);
                        b.LoadContent(_contentManager);
                        _gameObjectManager.AddCharacter(b);
                    }
                    if (p.Type == SaveLoad.TYPES.BIKETHIEF) // Hier könnte man noch was machen.
                    {
                        BikeThief b = new BikeThief(p.Position.ToVector2(), p.Size.ToPoint(), _audioService);
                        b.LoadContent(_contentManager);
                        _gameObjectManager.AddCharacter(b);
                    }
                }
                Console.WriteLine("Game loaded.");
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
                    _counter = 0;
                    _counterTimer = 0;

                    _gameObjectManager.Player1.Transform.Position =
                        new Vector2(worldBounds.Width / 2, worldBounds.Height / 2);
                    Console.WriteLine("Reset counter and player position.");
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
            _overlay.DrawOnWorld(spriteBatch, _gameObjectManager.Player1);

            if (_isTechDemo && _showStaticHitboxes)
            {
                _collisionManager.DrawHitboxes(
                    spriteBatch,
                    _pixel,
                    _gameObjectManager.Player1,
                    _gameObjectManager.Characters,
                    _itemManager.Items,
                    _gameObjectManager.Projectiles
                );
            }

            spriteBatch.End();
            spriteBatch.Begin();

            _debugger.Draw(spriteBatch);
            _overlay.DrawOnScreen(spriteBatch, gameTime);

            if (!_isTechDemo)
            {
                spriteBatch.DrawString(_debugFont, $"Counter: {_counter}", new Vector2(20, 160), Color.Black);
                spriteBatch.DrawString(_debugFont, "T=Save  L=Load  R=Reset counter", new Vector2(20, 185),
                    Color.Black);
            }
            _gameObjectManager.Player1.Inventory.Draw(spriteBatch, _pixel);
            hud.Draw(spriteBatch, _gameObjectManager.Player1);

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

    }
}