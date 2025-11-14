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
using InputAction = BikeWars.Content.engine.GameAction;

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
        private KeyboardState _prevKbState;
        private List<BoxCollider> _collisionBoxes;
        private Hobo hobo;

        public bool DrawLower => false;
        public bool UpdateLower => false;
        
        public GameScreen()
        {
            worldBounds = new Rectangle(0, 0, 11200, 11200);
            
            _testItems = new List<ItemBase>();
            _testItems.Add(new Item(new Vector2(worldBounds.Width / 2 + 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
            _testItems.Add(new Chest(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
            
            player = new Player(new Vector2(worldBounds.Width / 2, worldBounds.Height / 2), new Point(32, 32));
            
            hobo = new Hobo(new Vector2(worldBounds.Width / 2 + 10, worldBounds.Height / 2), new Point(32, 32));
            
            Game1 game = Game1.Instance;
            camera = new Camera2D(
                game.GraphicsDevice.Viewport.Width, 
                game.GraphicsDevice.Viewport.Height, 
                worldBounds
            );
            
            camera.Position = player.Transform.Position;
            
            _counter = SaveLoad.LoadGame();
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
            player.LoadContent(content, Content.Load<SoundEffect>(soundHandler.DRIVING_SOUND_PATH));
            hobo.LoadContent(content, Content.Load<SoundEffect>(soundHandler.WALKING_SOUND_PATH));

            // Items
            if (_testItems.Count > 1)
            {
                _testItems[1].LoadContent(content);
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
            
            foreach (var box in _collisionBoxes)   
            {
                if (player.Intersects(box))
                {
                    player.SetLastTransform();
                    player.UpdateCollider();
                }
            }
            
            _debugger.Update(gameTime);
            
            camera.Update(gameTime, player.Transform.Position, false);
            
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
            KeyboardState KbState = Keyboard.GetState();
    
            // use the central mapping instead of hardcoding keys
            Keys saveKey  = InputHandler.KeyMapping[InputAction.SAVE];
            Keys loadKey  = InputHandler.KeyMapping[InputAction.LOAD];
            Keys resetKey = InputHandler.KeyMapping[InputAction.RESET];

    
            // edge-triggered: pressed this frame, not last frame
            if (KbState.IsKeyDown(saveKey) && !_prevKbState.IsKeyDown(saveKey))
                SaveLoad.SaveGame(_counter);

            if (KbState.IsKeyDown(loadKey) && !_prevKbState.IsKeyDown(loadKey))
            {
                _counter = SaveLoad.LoadGame(); 
                _counterTimer = 0;    
            }

            if (KbState.IsKeyDown(resetKey) && !_prevKbState.IsKeyDown(resetKey))
            {
                _counter = 0;
                _counterTimer = 0;
                Console.WriteLine("Counter Reset. Counter=0");
            }

            _prevKbState = KbState;
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