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

        public bool DrawLower => false;
        public bool UpdateLower => false;
        
        public GameScreen()
        {
            // Weltgrenzen
            worldBounds = new Rectangle(0, 0, 4000, 2000);
    
            // Test-Items
            _testItems = new List<ItemBase>();
            _testItems.Add(new Item(new Vector2(worldBounds.Width / 2 + 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
            _testItems.Add(new Chest(new Vector2(worldBounds.Width / 2 - 50, worldBounds.Height / 2 + 50), new Point(32, 32)));
    
            // Player
            player = new Player(new Vector2(worldBounds.Width / 2, worldBounds.Height / 2), new Point(32, 32));
    
            // Camera mit GraphicsDevice von Game1
            Game1 game = Game1.Instance;
            camera = new Camera2D(
                game.GraphicsDevice.Viewport.Width, 
                game.GraphicsDevice.Viewport.Height, 
                worldBounds
            );
    
            // Camera auf Player zentrieren
            camera.Position = player.Transform.Position;
            
            _counter = SaveLoad.LoadGame();
        }
        
        public void LoadContent(ContentManager content)
        {
            // Font und Debugger
            _debugFont = content.Load<SpriteFont>("assets/fonts/Arial");
            _debugger = new Debugger(_debugFont, player);

            // Tiled Map
            _tiledMap = content.Load<TiledMap>("assets/Map/Bikewars_Tilemap");
            _tiledMapRenderer = new TiledMapRenderer(Game1.Instance.GraphicsDevice, _tiledMap);

            // Overlay
            _overlay = new Overlay(_debugFont, Game1.Instance.GraphicsDevice);

            // Player Soundeffekte laden
            SoundHandler soundHandler = new SoundHandler();
            player.LoadContent(content, content.Load<SoundEffect>(soundHandler.WALKING_SOUND_PATH));

            // Items laden
            if (_testItems.Count > 1)
            {
                _testItems[1].LoadContent(content);
            }
        }
        public void Update(GameTime gameTime)  // GameTime parameter verwenden
        {
            // Player Update
            player.Update(gameTime);  // Richtiges GameTime übergeben

            // Kollisionsprüfung
            if (player.Intersects(_testItems[0].Collider))
            {
                player.SetLastTransform();
                player.UpdateCollider();
            }

            if (_testItems.Count > 1 && player.Intersects(_testItems[1].Collider))
            {
                _testItems.RemoveAt(1);
            }

            // Debugger Update
            _debugger.Update(gameTime);  // Richtiges GameTime übergeben

            // Camera Update - freeCamera vorerst false
            camera.Update(gameTime, player.Transform.Position, false);

            // TiledMap Renderer Update
            _tiledMapRenderer.Update(gameTime);  // Richtiges GameTime übergeben
            
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
        public void Draw(GameTime gameTime)  // <- GameTime parameter verwenden
        {
            Game1 game = Game1.Instance;
            SpriteBatch spriteBatch = game.SpriteBatch;

            // Tiled Map zeichnen
            _tiledMapRenderer.Draw(camera.GetTransform());

            // Spielobjekte mit Camera-Transformation
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetTransform());
            player.Draw(spriteBatch);
            foreach (var item in _testItems)
            {
                item.Draw(spriteBatch);
            }
            // Lifelines unter dem Player (World-Space)
            _overlay.DrawOnWorld(spriteBatch, player);
            spriteBatch.End();

            // Debugger oben fixed
            spriteBatch.Begin();
            _debugger.Draw(spriteBatch);
            spriteBatch.End();
    
            // Overlay (Timer und Inventory) mit richtigem GameTime
            spriteBatch.Begin();
            _overlay.DrawOnScreen(spriteBatch, gameTime);  // <- Richtiges GameTime übergeben
            spriteBatch.End();
            
            spriteBatch.Begin();
            spriteBatch.DrawString(_debugFont, $"Counter: {_counter}", new Vector2(20, 100), Color.Black);
            spriteBatch.DrawString(_debugFont, "T=Save  L=Load  R=Reset counter", new Vector2(20, 125), Color.Black);
            spriteBatch.End();
        }
    }
}