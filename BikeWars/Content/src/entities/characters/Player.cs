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
using BikeWars.Content.entities.MapObjects;
using BikeWars.Content.managers;
using BikeWars.Entities.Characters.MapObjects;
using BikeWars.Utilities;
using BikeWars.Content.components;

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
        public PlayerMovement movement { get; set; }
        public Bike CurrentBike => movement?.CrtBike;
        private IPlayerInput _input;
        public bool IsActionPressed(GameAction action) => _input.IsPressed(action);
        private CooldownWithDuration sprint { get; }
        public new Vector2 GazeDirection { get; private set; }
        public int XpCounter { get; private set; } = 0;
        public int XpLevelUp = 10;
        public int CurrentLevel { get; private set; } = 1;
        public Vector2 AimTarget { get; private set; }
        private Vector2 _facingDirection = Vector2.UnitX; // Default to right
        private const float AimLength = 100f;

        //Range for throwing objects around the player
        public const float ThrowRange = 400f;

        // Virtual aim for analog throwing
        private Vector2 _virtualAimDir = Vector2.UnitX;
        private float _virtualAimDistance = 0f;
        private bool _shootHeldPrev = false;
        private const float THROW_AIM_MAX = 200f;
        private const float THROW_AIM_MIN = 0f;
        private const float THROW_AIM_GROW_SPEED = 400f; // px per second while holding trigger
        public TerrainCollider CurrentTerrain { get; set; }
        public float TerrainSpeedMultiplier = 1.0f;
        private const float IncreaseSpeed = 1.3f;
        private const float DecreaseSpeed = 0.75f;
        public new bool IsGodMode { get; set; }

        public event Action ShotBullet;
        public event Action<ItemBase> ItemPickedUp;
        public event Action Flamethrower;
        public event Action IceTrail;
        public event Action FireTrail;
        public event Action DamageCircle;
        public event Action OnEneryBarPickedUp;
        public event Action<Vector2> ThrowBook;
        public event Action<Vector2> ThrowBanana;
        public event Action<Vector2> ThrowBottle;
        public event Action<Vector2> ThrowBeer;

        private Vector2 _mouseWorldPos;
        private bool _isThrowTargetInRange;

        public event Action<Bike> Dismounted;

        private readonly SpriteAnimation _bikeUpAnimation;
        private readonly SpriteAnimation _walkDownAnimation;
        private readonly SpriteAnimation _walkUpAnimation;
        private readonly SpriteAnimation _walkLeftAnimation;
        private readonly SpriteAnimation _walkRightAnimation;
        private readonly SpriteAnimation _idleAnimation;

        private SpriteAnimation _currentAnimation;

        private new readonly AudioService _audio;
        private new WorldAudioManager _worldAudioManager;
        private string _currentMovementSound = null;
        public event Action<int, int> OnLevelUp;
        public event Action<BikeShop> OnBikeShopOpen;
        public event Action<int> OnMoreXP;

        public event Action<ItemBase> ChestItemSpawn;
        private bool _isUsingItem = false;
        private float _itemUseTimer = 0f;
        private const float ItemUseDuration = 2f;

        private float _bikeMountTime = 0f;
        private const float BikeMountTime = 0.1f;
        private int _currentItemIndex = -1;
        private int _selectedInventoryIndex = 0;
        public int SelectedInventoryIndex => _selectedInventoryIndex;

        private WeaponType _weaponBefore;
        private bool _beerThrowSelected = false;
        private int _inventoryIndexBeer = -1;

        // For achievements and statistics
        public bool phaseFoundBike;
        public event Action FoundBike;

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
            FireTrail,
            DamageCircle,
            BookThrow,
            BananaThrow,
            BottleThrow,
            // BeerThrow isn t an actual Weapon and can t be reached with weapon switch
            BeerThrow
        }

        public WeaponType CurrentWeapon { get; private set; } = WeaponType.BookThrow;
        private Dictionary<WeaponType, WeaponAttributes> _unlockedWeapons = new Dictionary<WeaponType, WeaponAttributes>();

        public WeaponAttributes? GetWeaponAttributes(WeaponType type)
        {
            if (_unlockedWeapons.TryGetValue(type, out var attributes))
            {
                return attributes;
            }
            return null;
        }

        public int GetWeaponLevel(WeaponType type)
        {
            if (_unlockedWeapons.TryGetValue(type, out var attributes))
            {
                return attributes.Level;
            }
            return 0;
        }

        private float _dopingTimer = 0f;
        public bool IsDoped => _dopingTimer > 0f;

        private bool TryShoot()
        {
            // Only shoot if we have a valid gaze direction
            Vector2 finalDirection = GazeDirection == Vector2.Zero ? _facingDirection : GazeDirection;

            if (finalDirection == Vector2.Zero) return false;

            switch (CurrentWeapon)
            {
                case WeaponType.Gun:
                    Attributes.AttackCooldown = 0.25f;
                    ShotBullet?.Invoke();
                    _audio.Sounds.Play(AudioAssets.GunShot);
                    return true;

                case WeaponType.Flamethrower:
                    Attributes.AttackCooldown = 3.0f;
                    Flamethrower?.Invoke();
                    _audio.Sounds.Play(AudioAssets.Flamethrower);
                    return true;

                case WeaponType.IceTrail:
                    Attributes.AttackCooldown = 3.0f;
                    IceTrail?.Invoke();
                    _audio.Sounds.Play(AudioAssets.IceTrail);
                    return true;

                case WeaponType.FireTrail:
                    Attributes.AttackCooldown = 3.0f;
                    FireTrail?.Invoke();
                    _audio.Sounds.Play(AudioAssets.Flamethrower);
                    return true;

                case WeaponType.DamageCircle:
                    Attributes.AttackCooldown = 3.0f;
                    DamageCircle?.Invoke();
                    _audio.Sounds.Play(AudioAssets.DamageCircle);
                    return true;

                case WeaponType.BookThrow:
                    if (!_isThrowTargetInRange) return false;
                    Attributes.AttackCooldown = 1.0f;
                    ThrowBook?.Invoke(_mouseWorldPos);
                    _audio.Sounds.Play(AudioAssets.ThrowObject);
                    return true;

                case WeaponType.BananaThrow:
                    if (!_isThrowTargetInRange) return false;
                    Attributes.AttackCooldown = 1.0f;
                    ThrowBanana?.Invoke(_mouseWorldPos);
                    _audio.Sounds.Play(AudioAssets.ThrowObject);
                    return true;

                case WeaponType.BottleThrow:
                    if (!_isThrowTargetInRange) return false;
                    Attributes.AttackCooldown = 1.0f;
                    ThrowBottle?.Invoke(_mouseWorldPos);
                    _audio.Sounds.Play(AudioAssets.ThrowObject);
                    return true;
                case WeaponType.BeerThrow:
                    if (!_isThrowTargetInRange) return false;
                    if (_beerThrowSelected)
                    {
                        Attributes.AttackCooldown = 0.1f;
                        ThrowBeer?.Invoke(_mouseWorldPos);
                        _audio.Sounds.Play(AudioAssets.ThrowObject);
                        _audio.Sounds.Play(AudioAssets.Jaegermeister);
                        Inventory.RemoveAt(_inventoryIndexBeer);
                        _beerThrowSelected = false;
                        CurrentWeapon = _weaponBefore;
                        return true;
                    }
                    return false;
            }
            return false;
        }

        public void OnPickUpItem(Player player, ItemBase item)
        {
            if (player != this) return;

            if (item.IsPickedUp) return; // Because of the frames we have to cut it here

            if (item is Xp xp)
            {
                AddXp(xp.xp_value);
            }

            if (item is EnergyBar eb)
            {
                Attributes.Health += eb.HealAmount;
                sprint.DecreaseCoolDownTimer(eb.DecreaseSprintCoolDown);
                OnEneryBarPickedUp?.Invoke();
                _audio.Sounds.Play(AudioAssets.EatingSnack);
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
            if (item is WeaponItem weapon)
            {
                if (_unlockedWeapons.TryGetValue(weapon.Type, out var attributes))
                {
                    upgradeWeapon(attributes);
                } else {
                    WeaponAttributes wp = new WeaponAttributes();
                    _unlockedWeapons.Add(weapon.Type, wp);
                }
            }
            item.IsPickedUp = true;
            ItemPickedUp?.Invoke(item);
        }

        private void upgradeWeapon(WeaponAttributes wa)
        {
        }

        public void OnInteractObject(Player player, ObjectBase obj)
        {
            if (player != this) return;
            if (!_input.IsPressed(GameAction.INTERACT)) return;
            if (obj is BikeShop shop)
            {
                if (shop.ShopReady)
                {OnBikeShopOpen?.Invoke(shop);}
                else
                {
                    _audio.Sounds.Play(AudioAssets.ShortPain);
                }
                return;
            }

            if (obj is Chest chest)
            {
                var drop = chest.OpenChest();
                if (drop != null)
                {
                    ChestItemSpawn?.Invoke(drop);
                }
                return;
            }

            if (obj is DogBowl dogBowl)
            {
                var selected = Inventory.GetItemAt(_selectedInventoryIndex);
                if (selected is DogFood)
                {
                    if (!dogBowl.TryActivateDogBowl())
                    {
                        return;
                    }

                    dogBowl.ActivateDogBowl(dogBowl.Transform.Position);
                    dogBowl.FillUpDogBowl();
                    Inventory.RemoveAt(_selectedInventoryIndex);
                }
                return;
            }
        }

        public void OnInteractTower(Player player, TowerAlly tower)
        {
            if (player != this) return;
            // Only activate if the Interact key (e.g. 'Q') is pressed
            if (!_input.IsPressed(GameAction.INTERACT)) return;

            tower.Activate();
            _audio.Sounds.Play(AudioAssets.HandgunClick);
        }

        public Player(Vector2 start, float radius, Point renderSize, AudioService audio, IPlayerInput input, bool isTechDemo = false, string characterPrefix = "Character1")
        {
            Attributes = new CharacterAttributes(this, 800, 0, 20, 2f, false);
            Transform = new Transform(start, radius);
            LastTransform = new Transform(start, radius);
            RenderTransform = new Transform(start, renderSize);
            _input = input;
            movement = new PlayerMovement(canMove: true, isMoving: false, _input);

            sprint = new CooldownWithDuration(1f, 5f);
            Inventory = new Inventory();
            _audio = audio;

            // LOAD BOTH ANIMATION SETS
            _bikeUpAnimation = SpriteManager.GetAnimation($"{characterPrefix}_BikeUp");

            _walkDownAnimation = SpriteManager.GetAnimation($"{characterPrefix}_WalkDown");
            _walkLeftAnimation = SpriteManager.GetAnimation($"{characterPrefix}_WalkLeft");
            _walkRightAnimation = SpriteManager.GetAnimation($"{characterPrefix}_WalkRight");
            _walkUpAnimation = SpriteManager.GetAnimation($"{characterPrefix}_WalkUp");
            _idleAnimation = SpriteManager.GetAnimation($"{characterPrefix}_Idle");

            if (movement.OwnsBike && movement.CrtBike != null)
            {
                _currentAnimation = _bikeUpAnimation;
            } else
            {
                _currentAnimation = _idleAnimation;
            }
            phaseFoundBike = false;

            // start weapon
            if (isTechDemo)
            {
                foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
                {
                    if (type != WeaponType.BeerThrow)
                        switch(type)
                        {
                            case WeaponType.Gun:
                                _unlockedWeapons.Add(type, new GunStatics(1, this));
                            break;
                            default:
                                _unlockedWeapons.Add(type, new WeaponAttributes());
                            break;
                        }
                }
            }
            else
            {
                _unlockedWeapons.Add(WeaponType.BookThrow, new WeaponAttributes());
            }
            CurrentWeapon = WeaponType.BookThrow;
            UpdateCollider();
        }

        public override void Update(GameTime gameTime)
        {
            Update(gameTime, AimTarget);
        }

        public void Update(GameTime gameTime, Vector2 mousePos)
        {
            _mouseWorldPos = mousePos;

            // Analog: use virtual cursor that can be stretched while holding shoot
            if (_input.IsAnalog)
            {
                UpdateAnalogThrowAim(gameTime);
            }

            var throwOrigin = Transform.Bounds.Center.ToVector2();
            _isThrowTargetInRange = Vector2.Distance(throwOrigin, _mouseWorldPos) <= ThrowRange;
            UpdateAttackCooldown(gameTime);
            UpdateMountTimer(gameTime);
            if (IsDying)
            {
                UpdateDyingState(gameTime);
                UpdateCollider();
                return;
            }

            if (_dopingTimer > 0f)
            {
                _dopingTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            Beer.UpdateCooldown(gameTime);
            HandleWeaponSwitch();
            HandleShooting();
            UpdateMovement(gameTime);
            UpdateCollider();
            HandleInventoryNavigation();
            HandleItemUsage(gameTime);
            HandleMovementSound();
            HandleAnimation(gameTime);
            UpdateGazeDirection(_mouseWorldPos);
            HandleGhostTrail(gameTime);
            HandleSwitchMovement();
            UpdateHitFlash(gameTime);
            UpdateCollider();
        }

        public override bool IsCharacterMoving()
        {
            return movement.IsMoving();
        }

        public override float GetPulseMultiplier()
        {
            // Reduce pulse significantly when on bike, as it looks weird if the bike breathes
            return movement.OwnsBike ? 0.5f : 1.0f;
        }

        public override void TakeDamage(int amount, object hitBy, bool shouldSquash = true)
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
                if (IsDoped) reducedDamage = 0; // Invulnerable
                base.TakeDamage(reducedDamage, hitBy, shouldSquash);

                if (bike.IsDestroyed)
                {
                    Dismount();
                }
            }
            else
            {
                if (IsDoped) amount = 0; // Invulnerable
                base.TakeDamage(amount, hitBy, shouldSquash);
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

            // 2) Spieler zeichnen
            if (_currentAnimation == null)
                return;

            // Dying Animation
            float rotationOffset = IsDying ? MathHelper.PiOver2 : 0f;
            Color drawColor = Color.White;

            if (IsDying)
            {
                float dyingProgress = 1f - (DyingTimer / DyingDuration);
                drawColor = Color.Lerp(Color.White, Color.Red, dyingProgress);
            }

            if (movement.CurrentMovement.GetType() ==
                typeof(WalkingMovement)) // TODO THIS IS ONLY INSERTED TO SHOW. BUT NOT GOOD!
            {
                _currentAnimation.Draw(spriteBatch, RenderTransform.Position, RenderTransform.Size,
                    movement.CurrentMovement.Rotation + rotationOffset, _renderScale, drawColor);
            }
            else
            {
                _currentAnimation.Draw(spriteBatch, RenderTransform.Position, RenderTransform.Size,
                    movement.CurrentMovement.Rotation + MathHelper.PiOver2 + rotationOffset, _renderScale, drawColor);
            }

            // Draw line from eye position only if GazeDirection is valid (non-zero)
            if (GazeDirection != Vector2.Zero)
            {
                Vector2 center = Transform.Position;

                // Draw static valid zone arc based on facing direction
                if (movement.OwnsBike)
                {
                    float facingAngle = (float)Math.Atan2(_facingDirection.Y, _facingDirection.X);
                    DrawUtils.DrawArc(spriteBatch, RenderPrimitives.Pixel, center, 50f, facingAngle, MathHelper.ToRadians(240),
                        Color.Red * 0.5f);
                }
                else
                {
                     DrawUtils.DrawCircleOutline(spriteBatch, RenderPrimitives.Pixel, center, 50f, Color.Red * 0.5f);
                }

                // Draw aiming line
                Vector2 aimEnd = center + GazeDirection * 50f;
                DrawUtils.DrawLine(spriteBatch, RenderPrimitives.Pixel, center, aimEnd, Color.Red);
            }

            if ((CurrentWeapon == WeaponType.BookThrow || CurrentWeapon == WeaponType.BananaThrow || CurrentWeapon == WeaponType.BottleThrow || CurrentWeapon == WeaponType.BeerThrow) && _isThrowTargetInRange)
            {
                DrawUtils.DrawCircleOutline(spriteBatch, RenderPrimitives.Pixel, _mouseWorldPos, 10f, Color.Gold);
            }
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

                    case TerrainType.BAECHLE:
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

        public bool TrySpendXp(int cost)
        {
            if (XpCounter < cost)
            {
                _audio.Sounds.Play(AudioAssets.ShortPain);
                return false;
            }
            else
            {
                XpCounter -= cost;
                return true;
            }
        }

        private void LevelUp()
        {
            while (XpCounter >= XpLevelUp)
            {
                XpCounter = XpCounter - XpLevelUp;
                CurrentLevel++;
                XpLevelUp = 7 + (CurrentLevel * CurrentLevel * 3);
                // level up screen is triggered:
                OnLevelUp?.Invoke(XpLevelUp, CurrentLevel);
            }
        }

        // the Upgrades from LevelUpScreen are applied here
        /*TODO these Upgrades are just a suggestion i can imagine, that the game crashes or has unsuspected
        behaviour, if for example the player is currently sprinting and the Sprint Time changes*/
        //TODO its not working properly yet
        public void UpgradeSkill(SkillTree.SkillId skill)
        {
            if (skill is SkillTree.SkillId.MoreHp)
            {
                Attributes.MaxHealth += 50;
                Attributes.Health += 50;
            }
            else if (skill is SkillTree.SkillId.MoreDamage)
            {
                Attributes.AttackDamage += 10;
            }
            else if (skill is SkillTree.SkillId.LongerSprintDuration)
            {
                sprint.Duration += 0.5f;
            }
            else if (skill is SkillTree.SkillId.AutomaticFire)
            {
                Attributes.CanAutoAttack = true;
            }
            else if (skill is SkillTree.SkillId.WeaponGun)
            {
                if (_unlockedWeapons.TryGetValue(WeaponType.Gun, out var weaponAttributes))
                {
                    if (weaponAttributes is GunStatics gunStats)
                    {
                        gunStats.LevelUp();
                        gunStats.Upgrade();
                    }
                    return;
                }
                _unlockedWeapons.Add(WeaponType.Gun, new GunStatics(1, this));
            }
            else if (skill is SkillTree.SkillId.WeaponBanana)
            {
                if (_unlockedWeapons.TryGetValue(WeaponType.BananaThrow, out var weaponAttributes))
                {
                    if (weaponAttributes is BananaStatics bananaStats)
                    {
                        bananaStats.LevelUp();
                        bananaStats.Upgrade();
                    }
                    return;
                }
                _unlockedWeapons.Add(WeaponType.BananaThrow, new BananaStatics(1, this));
            }
            else if (skill is SkillTree.SkillId.WeaponBottle)
            {
                if (_unlockedWeapons.TryGetValue(WeaponType.BottleThrow, out var weaponAttributes))
                {
                     if (weaponAttributes is BottleStatics bottleStats)
                    {
                        bottleStats.LevelUp();
                        bottleStats.Upgrade();
                    }
                    return;
                }
                _unlockedWeapons.Add(WeaponType.BottleThrow, new BottleStatics(1, this));
            }
            else if (skill is SkillTree.SkillId.CritChance)
            {
                Attributes.CritChance += 0.05f;
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
            if (isBikeWeapon(CurrentWeapon))
            {
                CurrentWeapon = WeaponType.BookThrow;
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
                }
                else if (item is DopingSpritze doping)
                {
                    _dopingTimer = 5f;
                    Attributes.Health = Math.Max(1, Attributes.Health - 50);
                }
                if (item is Beer beer)
                {

                    if (beer.TryActivateBeer())
                    {
                        _weaponBefore = CurrentWeapon;
                        _beerThrowSelected = true;
                        CurrentWeapon = WeaponType.BeerThrow;
                        _inventoryIndexBeer = _currentItemIndex;
                    }
                    else
                    {
                        Inventory.RedSlot(_currentItemIndex);
                    }
                }

                if (item is not Beer)
                {
                    Inventory.RemoveAt(_currentItemIndex);
                }
            }

            _isUsingItem = false;
            _currentItemIndex = -1;

            _audio.Sounds.Play(AudioAssets.Relief);
        }

        // Mount a Bike
        private void Mount(Bike b)
        {
            if (!phaseFoundBike)
            {
                phaseFoundBike = true;
                FoundBike?.Invoke();
            }
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
                LastTransform = new Transform(new Vector2(Transform.Position.X, Transform.Position.Y), Transform.Radius);

            TerrainSpeedMultiplier = GetTerrainMultiplier();
            if (IsDoped)
            {
                TerrainSpeedMultiplier *= 1.7f;
            }

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
            if (CurrentWeapon == WeaponType.BeerThrow)
                return;
            if (!_input.IsPressed(GameAction.SWITCH_WEAPON))
                return;

            var types = (WeaponType[])Enum.GetValues(typeof(WeaponType));
            int currentIndex = Array.IndexOf(types, CurrentWeapon);

            for (int i = 1; i < types.Length; i++)
            {
                int nextIndex = (currentIndex + i) % types.Length;
                WeaponType nextType = types[nextIndex];

                if (nextType == WeaponType.BeerThrow) continue;

                if (_unlockedWeapons.TryGetValue(nextType, out var weaponAttributes))
                {
                    if (isBikeWeapon(nextType) && !movement.OwnsBike)
                        continue;
                    CurrentWeapon = nextType;
                    return;
                }
            }
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

        private bool IsThrowWeapon(WeaponType weapon)
        {
            return weapon == WeaponType.BookThrow ||
                   weapon == WeaponType.BananaThrow ||
                   weapon == WeaponType.BottleThrow ||
                   weapon == WeaponType.BeerThrow;
        }

        private void UpdateAnalogThrowAim(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Direction comes from right stick; fall back to facing
            var dir = _input.GetAimDirection(Transform.Position, _facingDirection);
            if (dir != Vector2.Zero)
            {
                _virtualAimDir = Vector2.Normalize(dir);
            }
            else if (_virtualAimDir == Vector2.Zero)
            {
                _virtualAimDir = _facingDirection == Vector2.Zero ? Vector2.UnitX : _facingDirection;
            }

            bool shootHeld = _input.IsHeld(GameAction.SHOOT);

            if (shootHeld)
            {
                _virtualAimDistance += THROW_AIM_GROW_SPEED * dt;
            }
            else
            {
                // Decay slightly back toward a comfortable default
                _virtualAimDistance = MathF.Max(_virtualAimDistance - THROW_AIM_GROW_SPEED * 0.5f * dt, THROW_AIM_MIN);
            }

            _virtualAimDistance = MathHelper.Clamp(_virtualAimDistance, THROW_AIM_MIN, THROW_AIM_MAX);

            _mouseWorldPos = Transform.Position + _virtualAimDir * _virtualAimDistance;

            // On trigger release, throw for analog-controlled throw weapons
            if (_shootHeldPrev && !shootHeld && IsThrowWeapon(CurrentWeapon) && CanAttack())
            {
                bool fired = TryShoot();
                if (fired)
                {
                    ResetAttackCooldown();
                }
            }

            _shootHeldPrev = shootHeld;
        }

        private void UpdateGazeDirection(Vector2 mousePos)
        {
            // Gaze Direction Logic
            Vector2 eyePos = Transform.Position;
            Vector2 potentialGaze = _input.GetAimDirection(eyePos, _facingDirection);

            if (potentialGaze != Vector2.Zero)
            {
                AimTarget = eyePos + potentialGaze * AimLength;

                if (!movement.OwnsBike)
                {
                    // Full 360 degree gaze when walking
                    GazeDirection = potentialGaze;
                }
                else
                {
                    // 3. Apply Angle Restriction (240 degrees total = +/- 120 degrees) when on bike
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
            }
            if (GazeDirection == Vector2.Zero) {
                GazeDirection = _facingDirection;
            }
        }

        private void HandleShooting()
        {
            bool isAnalogThrow = _input.IsAnalog && IsThrowWeapon(CurrentWeapon);

            if (isAnalogThrow)
            {
                // Throw is handled on trigger release in UpdateAnalogThrowAim
                return;
            }

            bool shooting =
                (Attributes.CanAutoAttack && _input.IsHeld(GameAction.SHOOT) ||
                 _input.IsPressed(GameAction.SHOOT)) && CanAttack();
            if (shooting)
            {
                bool fired = TryShoot();
                if (fired)
                {
                    ResetAttackCooldown();
                }
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
            else
            {
                 if (movement.CurrentMovement is WalkingMovement)
                 {
                     _currentAnimation = _idleAnimation;
                     _currentAnimation?.Update(gameTime, true);
                 }
                 else
                 {
                     _currentAnimation = _bikeUpAnimation;
                 }
            }
        }

        public WeaponAttributes AttributesOfCurrentWeapon()
        {
            if (_unlockedWeapons.TryGetValue(CurrentWeapon, out var attributes)) {
                return attributes;
            }
            return new WeaponAttributes();
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
            if (_input.IsPressed(GameAction.INVENTORY_1)) _selectedInventoryIndex = 0;
            else if (_input.IsPressed(GameAction.INVENTORY_2)) _selectedInventoryIndex = 1;
            else if (_input.IsPressed(GameAction.INVENTORY_3)) _selectedInventoryIndex = 2;
            else if (_input.IsPressed(GameAction.INVENTORY_4)) _selectedInventoryIndex = 3;
            else if (_input.IsPressed(GameAction.INVENTORY_5)) _selectedInventoryIndex = 4;
        }

        public bool IsInteractPressed()
        {
            return _input.IsPressed(GameAction.INTERACT);
        }

        private bool isBikeWeapon(WeaponType weapon)
        {
            return weapon == WeaponType.IceTrail
                   || weapon == WeaponType.FireTrail
                   || weapon == WeaponType.DamageCircle;
        }
    }
}