using System;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BikeWars.Content.entities.items;
using BikeWars.Content.entities.interfaces;
using BikeWars.Entities.Characters;
using BikeWars.Content.engine;
using BikeWars.Utilities;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using BikeWars.Content.src.screens.Overlay;
using BikeWars.Content.src.utils.SaveLoadExample;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using BikeWars.Content.engine;

namespace BikeWars.Content.screens
{
    public class GameScreen : IScreen
    {
        private List<ItemBase> _testItems;
        private Player player;
        private Camera2D camera;
        private Rectangle worldBounds;
        private Overlay _overlay;
        private TiledMap _tiledMap;
        private TiledMapRenderer _tiledMapRenderer;
        private SpriteFont _debugFont;
        private Debugger _debugger;
        private int _counter = 0;
        private float _counterTimer = 0;
        private List<BoxCollider> _collisionBoxes;
        private Hobo hobo;

        private bool _freelook; // Has to be optimized

        public bool DrawLower => false;
        public bool UpdateLower => false;

        public GameScreen()
        {
            worldBounds = new Rectangle(0, 0, 11200, 11200);

            _testItems = new List<ItemBase>();
            _testItems.Add(new Item(new Vector2(worldBounds.Width / 2 + 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
            _testItems.Add(new Chest(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
            _testItems.Add(new Xp_Beer(new Vector2(worldBounds.Width / 2 + 50, worldBounds.Height / 2 - 50), new Point(32, 32)));
            _testItems.Add(new Xp_Money(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 - 50), new Point(32, 32)));

            player = new Player(new Vector2(worldBounds.Width / 2, worldBounds.Height / 2), new Point(32, 32));

            hobo = new Hobo(new Vector2(worldBounds.Width / 2 + 10, worldBounds.Height / 2), new Point(32, 32));

            Game1 game = Game1.Instance;
            camera = new Camera2D(
                game.GraphicsDevice.Viewport.Width,
                game.GraphicsDevice.Viewport.Height,
                worldBounds
            );
            _freelook = false;
            camera.Position = player.Transform.Position;
            
            // Create SaveLoad and load saved data
            var state = SaveLoad.LoadGame();
            _counter = state.Counter;
            player.Transform.Position = new Vector2(state.PlayerX, state.PlayerY);
            Console.WriteLine("Loaded saved position (or default if no file).");
        }

        public void LoadContent(ContentManager content)
        {
            // Font and Debugger
            _debugFont = content.Load<SpriteFont>("assets/fonts/Arial");
            _debugger = new Debugger(_debugFont, player);

            // Tiled Map
            _tiledMap = content.Load<TiledMap>("assets/Map/Bike_Wars_Map");
            _tiledMapRenderer = new TiledMapRenderer(Game1.Instance.GraphicsDevice, _tiledMap);

            var collisionLayer = _tiledMap.GetLayer<TiledMapTileLayer>("Collision");
            _collisionBoxes = new List<BoxCollider>();

            foreach (var tile in collisionLayer.Tiles)
            {
                if(tile.GlobalIdentifier == 0) continue;

                int x = tile.X * 16;
                int y = tile.Y * 16;

                _collisionBoxes.Add(new BoxCollider(new Vector2(x, y), 16, 16));
            }

            // Overlay
            _overlay = new Overlay(_debugFont, Game1.Instance.GraphicsDevice);

            // Player and Hobo Soundeffects
            SoundHandler soundHandler = new SoundHandler();
            player.LoadContent(content, content.Load<SoundEffect>(soundHandler.DRIVING_SOUND_PATH));
            hobo.LoadContent(content, content.Load<SoundEffect>(soundHandler.WALKING_SOUND_PATH));

            // Items
            if (_testItems.Count > 1)
            {
                _testItems[1].LoadContent(content);
                _testItems[2].LoadContent(content);
                _testItems[3].LoadContent(content);
            }
        }
        public void Update(GameTime gameTime)
        {
            InputHandler.Update();

            player.Update(gameTime);

            // Collision Handling with player
            if (player.Intersects(_testItems[0].Collider))
            {
                player.SetLastTransform();
                player.UpdateCollider();
            }

            if (_testItems.Count > 1 && player.Intersects(_testItems[1].Collider))
            {
                _testItems.RemoveAt(1);
            }
            
            foreach (var item in _testItems)
            {
                item.Update(gameTime);
            }

            foreach (var box in _collisionBoxes)
            {
                if (player.Intersects(box))
                {
                    player.SetLastTransform();
                    player.UpdateCollider();
                }
            }

            _debugger.Update(gameTime);


            // Needs to be implemented elsewhere.
            if (InputHandler.IsPressed(GameAction.TOGGLE_CAMERA))
            {
                _freelook = !_freelook;
                player.Immobalize(_freelook);
            }
            camera.Update(gameTime, player.Transform.Position, _freelook);

            _tiledMapRenderer.Update(gameTime);

            hobo.Update(gameTime);

            HandleCounter(gameTime);
            HandleSaveLoadInput();
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
                SaveLoad.SaveGame(_counter, player.Transform);

            if (InputHandler.IsPressed(GameAction.LOAD))
            {
               var state = SaveLoad.LoadGame();
               _counter = state.Counter;
                _counterTimer = 0;
                player.Transform.Position = new Vector2(state.PlayerX, state.PlayerY);
                Console.WriteLine("Game loaded.");
            }

            if (InputHandler.IsPressed(GameAction.RESET))
            {
                _counter = 0;
                _counterTimer = 0;
                
                player.Transform.Position = new Vector2(worldBounds.Width / 2, worldBounds.Height / 2);
                Console.WriteLine("Reset counter and player position.");
            }
        }
        public void Draw(GameTime gameTime)
        {
            Game1 game = Game1.Instance;
            SpriteBatch spriteBatch = game.SpriteBatch;

            _tiledMapRenderer.Draw(camera.GetTransform());

            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetTransform());
            player.Draw(spriteBatch);
            hobo.Draw(spriteBatch);
            foreach (var item in _testItems)
            {
                item.Draw(spriteBatch);
            }

            _overlay.DrawOnWorld(spriteBatch, player);
            spriteBatch.End();

            spriteBatch.Begin();
            _debugger.Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();
            _overlay.DrawOnScreen(spriteBatch, gameTime);
            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawString(_debugFont, $"Counter: {_counter}", new Vector2(20, 100), Color.Black);
            spriteBatch.DrawString(_debugFont, "T=Save  L=Load  R=Reset counter", new Vector2(20, 125), Color.Black);
            spriteBatch.End();
        }
    }
}