using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using System.Collections.Generic;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.entities.Inventory;
using BikeWars.Content.entities.items;
using BikeWars.Content.entities.levelup;
using BikeWars.Content.managers;
using System.Diagnostics.Metrics;

// ============================================================
// Player.cs
//
//
// Description:
// Represents the player character in the game, handling movement, animation, and sound effects.
// ============================================================
namespace BikeWars.Entities.Characters
{
    public class Player : CharacterBase, IWorldAudioAware
    {
        public Inventory Inventory { get; private set; }
        private PlayerMovement movement { get; set; }
        private CooldownWithDuration sprint { get; }

        public Vector2 GazeDirection { get; private set; }
        public int XpCounter { get; private set; } = 0;
        public int XpLevelUp = 10;
        public int CurrentLevel { get; private set; } = 1;
        public Vector2 AimTarget { get; private set; }
        private Vector2 _facingDirection = Vector2.UnitX; // Default to right
        private bool _usingControllerAim = false;
        private Vector2 _lastGazeDirection = Vector2.UnitX;

        public TerrainCollider CurrentTerrain { get; set; }
        public float TerrainSpeedMultiplier = 1.0f;
        // public bool IsGodMode { get; set; }

        public event Action ShotBullet;
        public event Action<ItemBase> ItemPickedUp;
        public event Action Flamethrower;
        public event Action IceTrail;

        private SpriteAnimation _walkDownAnimation;
        private SpriteAnimation _walkUpAnimation;
        private SpriteAnimation _walkLeftAnimation;
        private SpriteAnimation _walkRightAnimation;

        private SpriteAnimation _currentAnimation;

        private readonly AudioService _audio;
        private WorldAudioManager _worldAudioManager;
        private string _currentMovementSound = null;
        public event Action OnLevelUp;
        
        private bool _isUsingItem = false;
        private float _itemUseTimer = 0f;
        private const float ItemUseDuration = 2f;
        private ItemBase _currentItemBeingUsed;
        private int _currentItemIndex = -1;


        private struct GhostFrame
        {
            public Texture2D Texture;
            public Vector2 Position;
            public Rectangle Source;
            public float TimeLeft;
        }

        private readonly List<GhostFrame> _ghostTrail = new List<GhostFrame>();

        private float _ghostSpawnTimer = 0f;
        private const float GhostSpawnInterval = 0.05f; // alle 0,05s ein neues Ghost
        private const float GhostLifeTime = 0.1f;

        public enum WeaponType
        {
            Gun,
            Flamethrower,
            IceTrail
        }

        public WeaponType CurrentWeapon { get; private set; } = WeaponType.Gun;


        public override void UpdateCollider()
        {
            Vector2 colliderPosition = new Vector2(
                Transform.Position.X - Transform.Size.X / 2f,
                Transform.Position.Y - Transform.Size.Y / 2f
            );

            Collider = new BoxCollider(
                colliderPosition,
                Transform.Size.X,
                Transform.Size.Y,
                CollisionLayer.PLAYER,
                this
            );
        }

        // 1x1 Texture to represent the player
        public static Texture2D pixel;

        private void Shooting()
        {
            // Only shoot if we have a valid gaze direction
            if (GazeDirection == Vector2.Zero)
                return;

            switch (CurrentWeapon)
            {
                case WeaponType.Gun:
                    Attributes.AttackCooldown = 0.5f;
                    ShotBullet?.Invoke();
                    _audio.Sounds.Play(AudioAssets.GunShot);
                    break;

                case WeaponType.Flamethrower:
                    Attributes.AttackCooldown = 3.0f;
                    Flamethrower?.Invoke();
                    _audio.Sounds.Play(AudioAssets.Flamethrower);
                    break;

                case WeaponType.IceTrail:
                    Attributes.AttackCooldown = 3.0f;
                    IceTrail?.Invoke();
                    _audio.Sounds.Play(AudioAssets.IceTrail);
                    break;
            }
        }

        public void OnPickUpItem(Player player, ItemBase item)
        {
            if (player != this)
            {
                return;
            }

            if (item is Xp xp)
            {
                AddXp(xp.xp_value);
                if (XpCounter >= XpLevelUp)
                {
                    LevelUp();
                }
            }

            if (item.InventoryItem)
            {
                if (InputHandler.IsPressed(GameAction.INTERACT) && Inventory.AddItem(item))
                {
                    ItemPickedUp?.Invoke(item);
                }
                return;
            }
            ItemPickedUp?.Invoke(item);
        }

        public Player(Vector2 start, Point size, AudioService audio)
        {
            Attributes = new CharacterAttributes(300, 0 , 10, 2f, false);
            Transform = new Transform(start, size);
            LastTransform = new Transform(start, size);
            Speed = 200f;
            SprintSpeed = 350f;
            movement = new PlayerMovement(canMove: true, isMoving: false);
            sprint = new CooldownWithDuration(1f, 5f);
            Inventory = new Inventory();
            _audio = audio;
            if (pixel == null)
            {
                pixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
                pixel.SetData(new[] { Color.White });
            }

            _walkDownAnimation = SpriteManager.GetAnimation("Character1_WalkDown");
            _walkLeftAnimation = SpriteManager.GetAnimation("Character1_WalkLeft");
            _walkRightAnimation = SpriteManager.GetAnimation("Character1_WalkRight");
            _walkUpAnimation = SpriteManager.GetAnimation("Character1_WalkUp");

            _currentAnimation = _walkRightAnimation;
            UpdateCollider();
        }

        public override void Update(GameTime gameTime)
        {
            Update(gameTime, AimTarget);
        }

        public void Update(GameTime gameTime, Vector2 mousePos)
        {
            UpdateAttackCooldown(gameTime);

            // TODO THIS IS NOW ONLY FOR TESTING AND SHOWING
            if (InputHandler.IsPressed(GameAction.SWITCH))
            {
                if (movement.CurrentMovement.GetType() == typeof(BicycleMovement))
                {
                    movement.CurrentMovement = new WalkingMovement(true, true);
                    TerrainSpeedMultiplier = 1.0f;
                } else
                {
                    movement.CurrentMovement = new BicycleMovement(true, true, movement.RotationAcceleration);
                }
            }

            if (InputHandler.IsPressed(GameAction.SWITCH_WEAPON))
            {
                // Toggle between the two weapons
                if (CurrentWeapon == WeaponType.Gun)
                    CurrentWeapon = WeaponType.Flamethrower;
                else if (CurrentWeapon == WeaponType.Flamethrower)
                    CurrentWeapon = WeaponType.IceTrail;
                else
                    CurrentWeapon = WeaponType.Gun;
            }

            bool shooting = (Attributes.CanAutoAttack && InputHandler.IsHeld(GameAction.SHOOT) || InputHandler.IsPressed(GameAction.SHOOT)) && CanAttack();
            if (shooting)
            {
                Shooting();
                ResetAttackCooldown();
            }

            movement.Update();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // consume logic
            if (_isUsingItem)
            {
                _itemUseTimer -= delta;
                if (_itemUseTimer <= 0f)
                {
                    FinishUsingItem();
                }
            }
            else
            {
                if (InputHandler.IsPressed(GameAction.INVENTORY_1)) StartUsingItem(0);
                else if (InputHandler.IsPressed(GameAction.INVENTORY_2)) StartUsingItem(1);
                else if (InputHandler.IsPressed(GameAction.INVENTORY_3)) StartUsingItem(2);
                else if (InputHandler.IsPressed(GameAction.INVENTORY_4)) StartUsingItem(3);
                else if (InputHandler.IsPressed(GameAction.INVENTORY_5)) StartUsingItem(4);
            }

            // Sound-Control
            string desiredSound = movement.CurrentMovement is BicycleMovement
                ? AudioAssets.Driving
                : AudioAssets.Walking;

            float speedThreshold = 5.0f;
            bool hasSpeed = movement.CurrentMovement.Speed > speedThreshold;
            bool isInputMoving = movement.IsMoving();

            bool shouldPlaySound = hasSpeed && isInputMoving;

            if (!shouldPlaySound)
            {
                if (_currentMovementSound != null)
                {
                    _audio.Sounds.StopLoop(_currentMovementSound);
                    _currentMovementSound = null;
                }
            }
            else
            {
                if (_currentMovementSound != desiredSound)
                {
                    if (_currentMovementSound != null)
                        _audio.Sounds.StopLoop(_currentMovementSound);

                    _audio.Sounds.PlayLoop(desiredSound);
                    _currentMovementSound = desiredSound;
                }
            }

            // Sprinting Logic
            sprint.Update(gameTime);
            if (InputHandler.IsHeld(GameAction.SPRINT) && sprint.Ready)
            {
                sprint.Activate();
            }

            CurrentSpeed = sprint.IsActive ? SprintSpeed : movement.CurrentMovement.Speed;
            Vector2 direction = movement.CurrentMovement.Direction;

            if (Transform.Position.X != LastTransform.Position.X || Transform.Position.Y != LastTransform.Position.Y)
                LastTransform = new Transform(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size);

            TerrainSpeedMultiplier = GetTerrainMultiplier();


            bool isMoving = movement.IsMoving();
            if (isMoving)
            {
                direction.Normalize();
                _facingDirection = direction; // Update facing direction
                if (sprint.IsActive)
                {
                    Transform.Position += direction * CurrentSpeed * delta * TerrainSpeedMultiplier;
                }
                else
                {
                    Transform.Position += direction * movement.CurrentMovement.Speed * delta * TerrainSpeedMultiplier;
                }
                _currentAnimation = _walkUpAnimation;
                // choose animation based on main direction
                // @TODO We need to decide on how the animation should look like so don't delete it now
                if (movement.CurrentMovement.GetType() == typeof(WalkingMovement)) // THIS IS ONLY INSERTED TO SHOW. BUT NOT GOOD!
                {
                    if (MathF.Abs(direction.X) > MathF.Abs(direction.Y))
                {
                    _currentAnimation = (direction.X > 0) ? _walkRightAnimation : _walkLeftAnimation;
                }
                else
                {
                    _currentAnimation = (direction.Y > 0) ? _walkDownAnimation : _walkUpAnimation;
                }
                }
            }

            // Gaze Direction Logic
            Vector2 eyePos = Transform.Position;
            Vector2 potentialGaze = Vector2.Zero;

            // 1. Check Controller Input (Right Stick)
            Vector2 rightStick = InputHandler.GamePad.RightStick;

            // Check if mouse moved to switch back to mouse aiming
            if (InputHandler.Mouse.Delta != Point.Zero || InputHandler.Mouse.Held(MouseButton.Left))
            {
                 // Also reset if clicking, just in case
                _usingControllerAim = false;
            }

            if (rightStick != Vector2.Zero)
            {
                // Controller aiming
                rightStick.Y *= -1;
                potentialGaze = Vector2.Normalize(rightStick);

                // Store state
                _usingControllerAim = true;
                _lastGazeDirection = potentialGaze;

                AimTarget = eyePos + potentialGaze * 100f;
            }
            else if (_usingControllerAim)
            {
                // Fallback to last controller direction if we haven't touched the mouse
                potentialGaze = _lastGazeDirection;
                AimTarget = eyePos + potentialGaze * 100f;
            }
            else
            {
                // 2. Fallback to Mouse Input
                AimTarget = mousePos;
                Vector2 diff = AimTarget - eyePos;
                if (diff != Vector2.Zero)
                {
                    potentialGaze = Vector2.Normalize(diff);
                }
            }

            // 3. Apply Angle Restriction (240 degrees total = +/- 120 degrees)
            if (potentialGaze != Vector2.Zero)
            {
                // Check if the angle is within +/- 120 degrees
                if (Vector2.Dot(_facingDirection, potentialGaze) > -0.5f)
                {
                    GazeDirection = potentialGaze;
                }
                else
                {
                    // Clamp to nearest 120 degree angle
                    float facingAngle = (float)Math.Atan2(_facingDirection.Y, _facingDirection.X);
                    float cross = _facingDirection.X * potentialGaze.Y - _facingDirection.Y * potentialGaze.X;
                    float limit = MathHelper.ToRadians(120);

                    float targetAngle = facingAngle + (cross > 0 ? limit : -limit);

                    GazeDirection = new Vector2((float)Math.Cos(targetAngle), (float)Math.Sin(targetAngle));
                }
            }
            else
            {
                GazeDirection = Vector2.Zero;
            }

            _currentAnimation?.Update(gameTime, isMoving);

            if (movement.CurrentMovement.IsMoving && sprint.IsActive && _currentAnimation != null)
            {
                _ghostSpawnTimer -= delta;
                if (_ghostSpawnTimer <= 0f)
                {
                    _ghostSpawnTimer = GhostSpawnInterval;

                    Rectangle source = _currentAnimation.GetCurrentFrame();

                    _ghostTrail.Add(new GhostFrame
                    {
                        Texture = SpriteManager.GetCharacterAtlas(),
                        Position = Transform.Position,
                        Source = source,
                        TimeLeft = GhostLifeTime
                    });
                }
            }

            for (int i = _ghostTrail.Count - 1; i >= 0; i--)
            {
                var ghost = _ghostTrail[i];
                ghost.TimeLeft -= delta;

                if (ghost.TimeLeft <= 0f)
                {
                    _ghostTrail.RemoveAt(i);
                }
                else
                {
                    _ghostTrail[i] = ghost;
                }
            }
            UpdateCollider();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (var ghost in _ghostTrail)
            {
                float alpha = ghost.TimeLeft / GhostLifeTime;

                var ghostDest = new Rectangle(
                    (int)MathF.Round(ghost.Position.X),
                    (int)MathF.Round(ghost.Position.Y),
                    Transform.Size.X,
                    Transform.Size.Y
                );
                spriteBatch.Draw(
                    ghost.Texture,
                    destinationRectangle: ghostDest,
                    sourceRectangle: ghost.Source,
                    color: Color.White * alpha,
                    rotation: movement.Rotation,
                    origin: new Vector2(ghost.Source.Width / 2f, ghost.Source.Height / 2f),
                    effects: SpriteEffects.None,
                    layerDepth: 0f
                );
            }
            // saubere Ganzzahl-Position, sonst „zittert“ Pixelart
            var dest = new Rectangle(
                (int)MathF.Round(Transform.Position.X),
                (int)MathF.Round(Transform.Position.Y),
                Transform.Size.X,
                Transform.Size.Y
            );

            // 2) Spieler zeichnen
            if (_currentAnimation == null)
            return;

            if (movement.CurrentMovement.GetType() == typeof(WalkingMovement)) // TODO THIS IS ONLY INSERTED TO SHOW. BUT NOT GOOD!
            {
                _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size, movement.CurrentMovement.Rotation);
            } else
            {
                _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size, movement.CurrentMovement.Rotation + MathHelper.PiOver2);
            }

            // Draw line from eye position only if GazeDirection is valid (non-zero)
            if (GazeDirection != Vector2.Zero)
            {
                Vector2 center = Transform.Position;

                // Draw static valid zone arc based on facing direction
                float facingAngle = (float)Math.Atan2(_facingDirection.Y, _facingDirection.X);
                DrawArc(spriteBatch, center, 50f, facingAngle, MathHelper.ToRadians(240), Color.Red * 0.5f);

                // Draw aiming line
                Vector2 aimEnd = center + GazeDirection * 50f;
                DrawLine(spriteBatch, center, aimEnd, Color.Red);
            }
        }

        // Is Helpful for example with colliders to set the original position back.
        public override void SetLastTransform()
        {
            Transform = new Transform(new Vector2(LastTransform.Position.X, LastTransform.Position.Y), LastTransform.Size);
        }

        public void Immobalize(bool value)
        {
            if (value)
            {
                movement.CurrentMovement.CanMove = false;
            }
            else
            {
                movement.CurrentMovement.CanMove = true;
            }
        }

        public bool IsSprinting()
        {
            return sprint.IsActive;
        }

        public float CooldownTimer()
        {
            return sprint.GetRemainingCooldown();
        }

        private float GetTerrainMultiplier()
        {
            if (movement.CurrentMovement.GetType() == typeof(BicycleMovement))
            {
                if (CurrentTerrain == null)
                    return 1.0f;

                switch (CurrentTerrain.TerrainType)
                {
                    case TerrainType.ROAD:
                        return 1.10f;

                    case TerrainType.GRASS:
                        return 0.90f;

                    default:
                        return 1.0f;
                }
            }
            else
            {
                return 1f;
            }
        }



        // Helper function to draw a line
        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            spriteBatch.Draw(pixel,
                new Rectangle((int)start.X, (int)start.Y, (int)length, 2), // 2 is thickness
                null,
                color,
                angle,
                Vector2.Zero,
                SpriteEffects.None,
                0);
        }

        public void SetWorldAudioManager(WorldAudioManager manager)
        {
            _worldAudioManager = manager;
        }

        private void DrawArc(SpriteBatch spriteBatch, Vector2 center, float radius, float angle, float sweep, Color color, int segments = 16)
        {
            float startAngle = angle - sweep / 2f;
            float step = sweep / segments;

            for (int i = 0; i < segments; i++)
            {
                float theta1 = startAngle + i * step;
                float theta2 = startAngle + (i + 1) * step;

                Vector2 p1 = center + new Vector2((float)Math.Cos(theta1), (float)Math.Sin(theta1)) * radius;
                Vector2 p2 = center + new Vector2((float)Math.Cos(theta2), (float)Math.Sin(theta2)) * radius;

                DrawLine(spriteBatch, p1, p2, color);
            }
        }
        public void AddXp(int XpAmount)
        {
            XpCounter += XpAmount;

            if (XpCounter >= XpLevelUp)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            XpCounter = XpCounter - XpLevelUp;
            XpLevelUp = XpLevelUp * 2;
            CurrentLevel++;
            // level upscreen is triggered:
            OnLevelUp?.Invoke();
        }

        // the Upgrades from LevelUpScreen are applied here
        /*TODO these Upgrades are just a suggestion i can imagine, that the game crashes or has unsuspected
        behaviour, if for example the player is currently sprinting and the Sprint Time changes*/
        //TODO its not working properly yet
        public void UpgradeSkill(SkillTree.SkillId skill)
        {
            if (skill is SkillTree.SkillId.MoreHp)
            {
                Attributes.MaxHealth += 30;
            }
            else if (skill is SkillTree.SkillId.MoreDamage)
            {
                Attributes.AttackDamage += 2;
            }
            else if (skill is SkillTree.SkillId.LongerSprintDuration)
            {
                sprint.Duration += 0.5f;
            }
            else if (skill is SkillTree.SkillId.AutomaticFire)
            {
                Attributes.CanAutoAttack = true;
            }
        }
        
        private void StartUsingItem(int inventoryIndex)
        {
            if (_isUsingItem || inventoryIndex < 0 || inventoryIndex >= 5) // 5 = Inventory Size
                return;
            
            var item = Inventory.GetItemAt(inventoryIndex);
            
            if (item == null || !item.IsConsumable)
                return;

            _isUsingItem = true;
            _itemUseTimer = ItemUseDuration;
            _currentItemIndex = inventoryIndex;
            
            _audio.Sounds.Play(AudioAssets.Slurp);
        }
        
        private void FinishUsingItem()
        {
            var item = Inventory.GetItemAt(_currentItemIndex);

            if (item != null)
            {
                if (item is EnergyGel gel)
                {
                    Attributes.Health += gel.HealAmount;
                    if (Attributes.Health > Attributes.MaxHealth)
                        Attributes.Health = Attributes.MaxHealth;
                }
                
                Inventory.RemoveAt(_currentItemIndex);
            }

            _isUsingItem = false;
            _currentItemIndex = -1;
            
            _audio.Sounds.Play(AudioAssets.Relief);
        }



    }
}