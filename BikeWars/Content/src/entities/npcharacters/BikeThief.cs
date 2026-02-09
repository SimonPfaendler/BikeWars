#nullable enable
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Content.engine;
using BikeWars.Content.engine.Audio;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.entities.interfaces;
using BikeWars.Content.managers;
using BikeWars.Utilities;

namespace BikeWars.Entities.Characters
{
    public class BikeThief : CharacterBase, IWorldAudioAware
    {
        private readonly SpriteAnimation _idleAnimation;
        private readonly SpriteAnimation _walkLeftAnimation;
        private readonly SpriteAnimation _walkRightAnimation;
        private readonly SpriteAnimation _walkLeftFreloAnimation;
        private readonly SpriteAnimation _walkRightFreloAnimation;
        private readonly SpriteAnimation _walkLeftRacingAnimation;
        private readonly SpriteAnimation _walkRightRacingAnimation;
        private SpriteAnimation _currentAnimation;
        protected override string WalkingSound => AudioAssets.Walking;
        private readonly float _baseSpeed = 100f;
        private const float RacingBikePriorityRange = 800f;

        private readonly PathFinding _pathFinding;
        private readonly CollisionManager _collisionManager;
        private readonly RepathScheduler _repathScheduler;
        private float _talkTimer = 0f;
        private const float TALK_INTERVAL = 7.5f;

        private Bike? _stolenBike;
        private bool _isEscaping;
        private Vector2 _escapeTarget;
        private const float KnockOffForce = 180f;
        private static readonly Vector2 EscapeExit = new(9414f, 7815f);

        private static readonly string[] TalkSounds = {
            AudioAssets.BikeThiefLaugh,
            AudioAssets.BikeThiefTalk,
        };

        public BikeThief(Vector2 start, float size, AudioService audio, PathFinding pathFinding,
            CollisionManager collisionManager, RepathScheduler repathScheduler, ITargetProvider targetProvider): base(targetProvider)
        {
            // Werte kannst du anpassen, wenn der BikeThief z.B. stärker/schneller sein soll
            _audio = audio;
            _pathFinding = pathFinding;
            _collisionManager = collisionManager;
            _repathScheduler =  repathScheduler;

            Attributes = new CharacterAttributes(this, 40, 0, 5, 2f, false);
            Transform = new Transform(start, size);
            RenderTransform = new Transform(start, new Point(32, 32));
            Speed = _baseSpeed;
            Movement = new EnemyMovement(canMove: true, isMoving: false, pathFinding: _pathFinding,
                gridMapper: _collisionManager, repathScheduler: _repathScheduler);
            _idleAnimation = SpriteManager.GetAnimation("BikeThief_Idle");
            _walkLeftAnimation = SpriteManager.GetAnimation("BikeThief_WalkLeft");
            _walkRightAnimation = SpriteManager.GetAnimation("BikeThief_WalkRight");
            _walkLeftFreloAnimation = SpriteManager.GetAnimation("BikeThief_Frelo_WalkLeft");
            _walkRightFreloAnimation = SpriteManager.GetAnimation("BikeThief_Frelo_WalkRight");
            _walkLeftRacingAnimation = SpriteManager.GetAnimation("BikeThief_Racing_WalkLeft");
            _walkRightRacingAnimation = SpriteManager.GetAnimation("BikeThief_Racing_WalkRight");
            _currentAnimation = _idleAnimation;
            UpdateCollider(CollisionLayer.CHARACTER);

            Attributes.OnDied += _ => DropStolenBike();
        }

        public override void Update(GameTime gameTime)
        {
            if (Movement is EnemyMovement em)
            {
                em.EnemyPosition = Transform.Position;
                if (_isEscaping)
                {
                    em.PlayerPosition = _escapeTarget;
                }
                else
                {
                    var gom = _collisionManager.GameObjectManager;
                    Player? target = GetRacingBikeTargetInRange(RacingBikePriorityRange)
                        ?? gom.GetTargetPlayer(Transform.Position);

                    if (target != null)
                        em.PlayerPosition = target.Transform.Position;
                }
            }
            Movement.HandleMovement(gameTime);

            if (_isEscaping && Vector2.DistanceSquared(Transform.Position, _escapeTarget) < 25f * 25f)
            {
                _isEscaping = false;
                Movement.CanMove = false;
                Movement.Direction = Vector2.Zero;
                Speed = _baseSpeed;
                return;
            }

            _talkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_talkTimer >= TALK_INTERVAL)
            {
                _talkTimer = 0f;

                PlayTalkWithWorldAudio();
            }

            UpdateAttackCooldown(gameTime);
            UpdateKnockback(gameTime);
            UpdateHitFlash(gameTime);
            // Sound-Control
            HandleSound(Movement.IsMoving);

            Vector2 direction = Movement.Direction;

            if (Movement.IsMoving)
            {
                float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (direction.LengthSquared() > 0.0001f)
                {
                    direction.Normalize();
                    Transform.Position += direction * Speed * delta;
                }

                if (direction.X > 0.1f)
                {
                    _currentAnimation = SelectRightAnimation();
                }
                else if (direction.X < -0.1f)
                {
                    _currentAnimation = SelectLeftAnimation();
                }
                else
                {
                    _currentAnimation = SelectIdleAnimation();
                }
            }
            else
            {
                _currentAnimation = SelectIdleAnimation();
            }

            if (_currentAnimation != null)
            {
                _currentAnimation.Update(gameTime, Movement.IsMoving);
            }
            UpdateCollider(CollisionLayer.CHARACTER);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead)
                return;
            if (_currentAnimation == null)
                return;

            Color drawColor = (_hitFlashTimer > 0f) ? _hitColor : Color.White;
            _currentAnimation.Draw(spriteBatch, RenderTransform.Position, RenderTransform.Size, 0f, _renderScale, drawColor);
        }

        public void SetWorldAudioManager(WorldAudioManager manager)
        {
            _worldAudioManager = manager;
        }

        public override void Attack(ICombat target)
        {
            if (!CanAttack()) return;

            if (target is Player player)
            {
                if (TryStealBike(player))
                {
                    ResetAttackCooldown();
                    return;
                }
            }

            if (_stolenBike != null)
                return;

            base.Attack(target);
            _audio.Sounds.Play(AudioAssets.Punch);
        }

        private void PlayTalkWithWorldAudio()
        {
            if (_worldAudioManager == null) {
                return;
            }

            float volume = _worldAudioManager.GetVolumeFor(Transform.Position);

            if (volume > 0)
            {
                if (PlayerOnBike())
                {
                    int index = RandomUtil.NextInt(0, TalkSounds.Length);
                    string randomTalk = TalkSounds[index];

                    _audio.Sounds.Play(randomTalk);
                }
                else
                {_audio.Sounds.Play(AudioAssets.BikeThiefLaugh);}

            }
        }

        private bool PlayerOnBike()
        {
            var gom = _collisionManager.GameObjectManager;

            if (gom.Player1 != null && gom.Player1.movement.OwnsBike)
                return true;

            if (gom.Player2 != null && gom.Player2.movement.OwnsBike)
                return true;

            return false;
        }

        private Player? GetRacingBikeTargetInRange(float range)
        {
            var gom = _collisionManager.GameObjectManager;
            float rangeSq = range * range;
            Player? best = null;
            float bestDistSq = rangeSq;

            Check(gom.Player1);
            Check(gom.Player2);

            return best;

            void Check(Player? p)
            {
                if (p == null || p.IsDead || p.movement == null || !p.movement.OwnsBike)
                    return;

                if (p.CurrentBike is not RacingBike)
                    return;

                float distSq = Vector2.DistanceSquared(Transform.Position, p.Transform.Position);
                if (distSq > bestDistSq)
                    return;

                bestDistSq = distSq;
                best = p;
            }
        }

        private bool TryStealBike(Player player)
        {
            if (player?.movement == null)
                return false;

            if (!player.movement.OwnsBike || player.CurrentBike == null)
                return false;

            _stolenBike = player.CurrentBike;

            // Knock the player off the bike without dropping it as an item yet
            player.movement.CurrentMovement = new WalkingMovement(
                player.movement.CurrentMovement.CanMove,
                player.movement.CurrentMovement.IsMoving,
                player.movement.WalkingSpeed,
                player.movement.SprintAcceleration);
            player.movement.OwnsBike = false;
            player.movement.CrtBike = null;

            Vector2 knockDir = player.Transform.Position - Transform.Position;
            if (knockDir != Vector2.Zero)
            {
                player.ApplyKnockback(knockDir, KnockOffForce);
            }

            _isEscaping = true;
            _escapeTarget = EscapeExit;
            Speed = _baseSpeed;

            if (Movement is EnemyMovement em)
            {
                em.PlayerPosition = _escapeTarget;
                em.ForceRepath();
            }

            UpdateAnimationSet();
            return true;
        }

        private void UpdateAnimationSet()
        {
            if (_stolenBike == null)
                return;

            // pick idle variant based on latest movement; fallback already handled in update loop
            _currentAnimation = _idleAnimation;
        }

        private SpriteAnimation SelectLeftAnimation()
        {
            if (_stolenBike is Frelo)
                return _walkLeftFreloAnimation;

            if (_stolenBike is RacingBike)
                return _walkLeftRacingAnimation;

            return _walkLeftAnimation;
        }

        private SpriteAnimation SelectRightAnimation()
        {
            if (_stolenBike is Frelo)
                return _walkRightFreloAnimation;

            if (_stolenBike is RacingBike)
                return _walkRightRacingAnimation;

            return _walkRightAnimation;
        }

        private SpriteAnimation SelectIdleAnimation()
        {
            if (_stolenBike is Frelo)
                return _walkRightFreloAnimation;

            if (_stolenBike is RacingBike)
                return _walkRightRacingAnimation;

            return _idleAnimation;
        }

        private void DropStolenBike()
        {
            if (_stolenBike == null)
                return;

            _stolenBike.Transform.Position = Transform.Position;
            _stolenBike.Collider.Position = new Vector2(Collider.Position.X, Collider.Position.Y);
            _collisionManager.GameObjectManager.AddItem(_stolenBike);
            _stolenBike = null;
        }

    }
}
