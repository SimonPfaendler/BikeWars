using Microsoft.Xna.Framework;
using System;
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
using BikeWars.Utilities;

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
        public Bike CurrentBike => movement?.CrtBike;
        private IPlayerInput _input;
        private CooldownWithDuration sprint { get; }

        public Vector2 GazeDirection { get; private set; }
        public int XpCounter { get; private set; } = 0;
        public int XpLevelUp = 10;
        public int CurrentLevel { get; private set; } = 1;
        public Vector2 AimTarget { get; private set; }
        private Vector2 _facingDirection = Vector2.UnitX; // Default to right
        private bool _usingControllerAim = false;
        private Vector2 _lastGazeDirection = Vector2.UnitX;
        private const float AimLength = 100f;

        public TerrainCollider CurrentTerrain { get; set; }
        public float TerrainSpeedMultiplier = 1.0f;
        private const float IncreaseSpeed = 1.1f;
        private const float DecreaseSpeed = 0.9f;
        public bool IsGodMode { get; set; }

        public event Action ShotBullet;
        public event Action<ItemBase> ItemPickedUp;
        public event Action Flamethrower;
        public event Action IceTrail;
        public event Action DamageCircle;

        public event Action<Bike> Dismounted;
        private SpriteAnimation _bikeDownAnimation;
        private SpriteAnimation _bikeUpAnimation;
        private SpriteAnimation _bikeLeftAnimation;
        private SpriteAnimation _bikeRightAnimation;

        private SpriteAnimation _walkDownAnimation;
        private SpriteAnimation _walkUpAnimation;
        private SpriteAnimation _walkLeftAnimation;
        private SpriteAnimation _walkRightAnimation;

        private SpriteAnimation _currentAnimation;

        private readonly AudioService _audio;
        private WorldAudioManager _worldAudioManager;
        private string _currentMovementSound = null;
        public event Action<int, int> OnLevelUp;
        public event Action<int> OnMoreXP;

        private bool _isUsingItem = false;
        private float _itemUseTimer = 0f;
        private const float ItemUseDuration = 2f;

        private float _bikeMountTime = 0f;
        private const float BikeMountTime = 0.1f;
        private ItemBase _currentItemBeingUsed;
        private int _currentItemIndex = -1;
        private int _selectedInventoryIndex = 0;
        public int SelectedInventoryIndex => _selectedInventoryIndex;



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
            IceTrail,
            DamageCircle
        }

        public WeaponType CurrentWeapon { get; private set; } = WeaponType.Gun;

        // 1x1 Texture to represent the player
        public static Texture2D pixel;

        private void Shooting()
        {
            // Only shoot if we have a valid gaze direction
            Vector2 finalDirection = GazeDirection == Vector2.Zero ? _facingDirection : GazeDirection;

            if (finalDirection == Vector2.Zero) return;

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

                case WeaponType.DamageCircle:
                    Attributes.AttackCooldown = 3.0f;
                    DamageCircle?.Invoke();
                    _audio.Sounds.Play(AudioAssets.DamageCircle);
                    break;
            }
        }

        public void OnPickUpItem(Player player, ItemBase item)
        {
            if (player != this)
            {
                return;
            }
            if (item.IsPickedUp) // Because of the frames we have to cut it here
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
                if (_input.IsPressed(GameAction.INTERACT) && Inventory.AddItem(item))
                {
                    item.IsPickedUp = true;
                    ItemPickedUp?.Invoke(item);
                }
                return;
            }
            if (item.IsBike)
            {
                if (_input.IsPressed(GameAction.SWITCH) && _bikeMountTime == 0f)
                {
                    Mount((Bike)item);
                    item.IsPickedUp = true;
                    ItemPickedUp?.Invoke(item);
                }
                return;
            }
            item.IsPickedUp = true;
            ItemPickedUp?.Invoke(item);
        }

        public Player(Vector2 start, Point size, AudioService audio, IPlayerInput input)
        {
            Attributes = new CharacterAttributes(this, 300, 0, 10, 2f, false);
            Transform = new Transform(start, size);
            LastTransform = new Transform(start, size);
            _input = input;
            movement = new PlayerMovement(canMove: true, isMoving: false, _input);

            sprint = new CooldownWithDuration(1f, 5f);
            Inventory = new Inventory();
            _audio = audio;
            if (pixel == null)
            {
                pixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
                pixel.SetData(new[] { Color.White });
            }

            // LOAD BOTH ANIMATION SETS
            _bikeDownAnimation = SpriteManager.GetAnimation("Character1_BikeDown");
            _bikeLeftAnimation = SpriteManager.GetAnimation("Character1_BikeLeft");
            _bikeRightAnimation = SpriteManager.GetAnimation("Character1_BikeRight");
            _bikeUpAnimation = SpriteManager.GetAnimation("Character1_BikeUp");

            _walkDownAnimation = SpriteManager.GetAnimation("Character1_WalkDown");
            _walkLeftAnimation = SpriteManager.GetAnimation("Character1_WalkLeft");
            _walkRightAnimation = SpriteManager.GetAnimation("Character1_WalkRight");
            _walkUpAnimation = SpriteManager.GetAnimation("Character1_WalkUp");

            _currentAnimation = _bikeRightAnimation;
            UpdateCollider();
        }

        public override void Update(GameTime gameTime)
        {
            Update(gameTime, AimTarget);
        }

        public void Update(GameTime gameTime, Vector2 mousePos)
        {
            UpdateAttackCooldown(gameTime);
            UpdateMountTimer(gameTime);

            HandleWeaponSwitch();
            HandleShooting();
            UpdateMovement(gameTime);
            HandleInventoryNavigation();
            HandleItemUsage(gameTime);
            HandleMovementSound();
            HandleAnimation(gameTime);
            UpdateGazeDirection(mousePos);
            HandleGhostTrail(gameTime);
            HandleSwitchMovement();

            UpdateCollider();
        }

        public override void TakeDamage(int amount)
        {
            // Deactivate Godmode for testing damage
            if (IsGodMode)
               return;

            if (IsDead) return;

            if (movement.OwnsBike && movement.CrtBike != null)
            {
                var bike = movement.CrtBike;
                bike.TakeDamage(amount);

                int reducedDamage = Math.Max(0, amount - bike.Attributes.Armor);
                base.TakeDamage(reducedDamage);

                if (bike.IsDestroyed)
                {
                    Dismount();
                }
            }
            else
            {
                base.TakeDamage(amount);
            }
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

            if (movement.CurrentMovement.GetType() ==
                typeof(WalkingMovement)) // TODO THIS IS ONLY INSERTED TO SHOW. BUT NOT GOOD!
            {
                _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size,
                    movement.CurrentMovement.Rotation);
            }
            else
            {
                _currentAnimation.Draw(spriteBatch, Transform.Position, Transform.Size,
                    movement.CurrentMovement.Rotation + MathHelper.PiOver2);
            }

            // Draw line from eye position only if GazeDirection is valid (non-zero)
            if (GazeDirection != Vector2.Zero)
            {
                Vector2 center = Transform.Position;

                // Draw static valid zone arc based on facing direction
                float facingAngle = (float)Math.Atan2(_facingDirection.Y, _facingDirection.X);
                DrawUtils.DrawArc(spriteBatch, pixel, center, 50f, facingAngle, MathHelper.ToRadians(240),
                    Color.Red * 0.5f);

                // Draw aiming line
                Vector2 aimEnd = center + GazeDirection * 50f;
                DrawUtils.DrawLine(spriteBatch, pixel, center, aimEnd, Color.Red);
            }
        }

        // Is Helpful for example with colliders to set the original position back.
        public override void SetLastTransform()
        {
            Transform = new Transform(new Vector2(LastTransform.Position.X, LastTransform.Position.Y),
                LastTransform.Size);
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
            return sprint.RemainingCooldown;
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
                        return IncreaseSpeed;

                    case TerrainType.GRASS:
                        return DecreaseSpeed;

                    default:
                        return 1.0f;
                }
            }
            else
            {
                return 1f;
            }
        }

        public void SetWorldAudioManager(WorldAudioManager manager)
        {
            _worldAudioManager = manager;
        }

        public void AddXp(int XpAmount)
        {
            XpCounter += XpAmount;
            OnMoreXP?.Invoke(XpLevelUp + XpAmount);
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
            OnLevelUp?.Invoke(XpLevelUp, CurrentLevel);
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

        // When you dismount of the bike
        public void Dismount()
        {
            if (!movement.OwnsBike)
            {
                return;
            }
            _bikeMountTime = BikeMountTime;
            movement.CurrentMovement = new WalkingMovement(movement.CurrentMovement.CanMove, movement.CurrentMovement.IsMoving, movement.WalkingSpeed, movement.SprintAcceleration);
            movement.OwnsBike = false;
            movement.CrtBike.Transform.Position = Transform.Position;
            movement.CrtBike.Collider.Position = Collider.Position;
            if (!movement.CrtBike.IsDestroyed)
            {
                Dismounted?.Invoke(movement.CrtBike);
            }
            movement.CrtBike = null;
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

        // Mount a Bike
        private void Mount(Bike b)
        {
            _bikeMountTime = BikeMountTime;
            movement.CurrentMovement = new BicycleMovement(movement.CurrentMovement.CanMove, movement.CurrentMovement.IsMoving, 0, b.Attributes.MaxSpeed, b.Attributes.SpeedAcceleration, b.Attributes.SprintAcceleration, b.Attributes.RotationAcceleration);
            switch (b)
            {
                case Frelo:
                    movement.CrtBike = new Frelo(b.Transform.Position, b.Transform.Size, b.Attributes);
                    break;
                case RacingBike:
                    movement.CrtBike = new RacingBike(b.Transform.Position, b.Transform.Size, b.Attributes);
                    break;
            }
            movement.OwnsBike = true;
        }

        private void HandleSwitchMovement()
        {
            if (!_input.IsPressed(GameAction.SWITCH))
                return;
            if (movement.OwnsBike)
            {
                Dismount();
            }
        }

        private void UpdateMovement(GameTime gameTime)
        {
            float d = (float)gameTime.ElapsedGameTime.TotalSeconds;
            movement.Update();
            sprint.Update(gameTime);
            if (_input.IsHeld(GameAction.SPRINT) && sprint.Ready)
            {
                sprint.Activate();
            }

            CurrentSpeed = sprint.IsActive ? movement.CurrentMovement.Speed *movement.CurrentMovement.SprintAcceleration : movement.CurrentMovement.Speed;
            Vector2 direction = movement.CurrentMovement.Direction;

            if (Transform.Position.X != LastTransform.Position.X || Transform.Position.Y != LastTransform.Position.Y)
                LastTransform = new Transform(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Size);

            TerrainSpeedMultiplier = GetTerrainMultiplier();

            if (movement.IsMoving())
            {
                direction.Normalize();
                _facingDirection = direction; // Update facing direction
                if (sprint.IsActive)
                {
                    Transform.Position += direction * CurrentSpeed * movement.CurrentMovement.SprintAcceleration * d * TerrainSpeedMultiplier;
                }
                else
                {
                    Transform.Position += direction * CurrentSpeed * d * TerrainSpeedMultiplier;
                }
            }
        }

        private void HandleWeaponSwitch()
        {
            if (!_input.IsPressed(GameAction.SWITCH_WEAPON))
                return;

            // Toggle between the two weapons
            if (CurrentWeapon == WeaponType.Gun)
                CurrentWeapon = WeaponType.Flamethrower;
            else if (CurrentWeapon == WeaponType.Flamethrower)
                CurrentWeapon = WeaponType.IceTrail;
            else if (CurrentWeapon == WeaponType.IceTrail)
                CurrentWeapon = WeaponType.DamageCircle;
            else
                CurrentWeapon = WeaponType.Gun;
        }

        private void HandleItemUsage(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
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
                if (_input.IsPressed(GameAction.INVENTORY_1)) StartUsingItem(0);
                else if (_input.IsPressed(GameAction.INVENTORY_2)) StartUsingItem(1);
                else if (_input.IsPressed(GameAction.INVENTORY_3)) StartUsingItem(2);
                else if (_input.IsPressed(GameAction.INVENTORY_4)) StartUsingItem(3);
                else if (_input.IsPressed(GameAction.INVENTORY_5)) StartUsingItem(4);

                if (_input.IsPressed(GameAction.INVENTORY_USE))
                {
                    StartUsingItem(_selectedInventoryIndex);
                }
            }
        }

        private void HandleMovementSound()
        {
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
        }


        private void UpdateGazeDirection(Vector2 mousePos)
        {
            // Gaze Direction Logic
            Vector2 eyePos = Transform.Position;
            Vector2 potentialGaze = _input.GetAimDirection(eyePos, _facingDirection);

            // 3. Apply Angle Restriction (240 degrees total = +/- 120 degrees)
            if (potentialGaze != Vector2.Zero)
            {
                AimTarget = eyePos + potentialGaze * AimLength;
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
            if (GazeDirection == Vector2.Zero) {
                GazeDirection = _facingDirection;
            }
        }

        private void HandleShooting()
        {
            bool shooting =
                (Attributes.CanAutoAttack && _input.IsHeld(GameAction.SHOOT) ||
                 _input.IsPressed(GameAction.SHOOT)) && CanAttack();
            if (shooting)
            {
                Shooting();
                ResetAttackCooldown();
            }
        }

        // If we press the button to exchange the bike it can be now immediately. With this delay that shouldn't occur
        public void UpdateMountTimer(GameTime gameTime)
        {
            if (_bikeMountTime > 0f)
            {
                _bikeMountTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_bikeMountTime < 0f)
                    _bikeMountTime = 0f;
            }
        }

        private void HandleGhostTrail(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
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
        }

        private void HandleAnimation(GameTime gameTime)
        {
            if (movement.IsMoving())
            {
                // Choose animation based on movement type
                if (movement.CurrentMovement is WalkingMovement)
                {
                    if (MathF.Abs(_facingDirection.X) > MathF.Abs(_facingDirection.Y))
                    {
                        _currentAnimation = (_facingDirection.X > 0) ? _walkRightAnimation : _walkLeftAnimation;
                    }
                    else
                    {
                        _currentAnimation = (_facingDirection.Y > 0) ? _walkDownAnimation : _walkUpAnimation;
                    }
                }
                else // BicycleMovement
                {
                    _currentAnimation = _bikeUpAnimation;
                }

                _currentAnimation?.Update(gameTime, movement.IsMoving());
            }
        }
        public void SetInput(IPlayerInput input)
        {
            _input = input;
            movement.SetInput(input);
        }
        private void HandleInventoryNavigation()
        {
            if (_input.IsPressed(GameAction.INVENTORY_NEXT))
            {
                _selectedInventoryIndex = (_selectedInventoryIndex + 1) % 5;
            }
            else if (_input.IsPressed(GameAction.INVENTORY_PREV))
            {
                _selectedInventoryIndex = (_selectedInventoryIndex + 4) % 5;
            }
        }


    }
}