using System;
using BikeWars.Utilities;
using Microsoft.Xna.Framework;
// ============================================================
// Camera.cs
// Defines a 2D camera with PlayerLock and FreeLook modes with
// pressing TOGGLE_CAMERA(C).
// ============================================================
namespace BikeWars.Content.engine
{
    public enum CameraMode
    {
        PlayerLock,
        FreeLook
    }

    public class Camera2D
    {
        public Vector2 Position { get; set; } = Vector2.Zero; // Where the camera points to
        public float Zoom { get; set; } = 1f;

        private const float MIN_ZOOM = 0.5f;
        private const float MAX_ZOOM = 1.1f;

        public CameraMode Mode { get; set; } = CameraMode.PlayerLock;

        // Defines the visible screen window size
        private readonly int _viewportWidth;
        private readonly int _viewportHeight;
        private const float _TWO_PLAYER_CAMERA_PADDING = 300f; // padding to the viewport

        // Defines the border of the game world so the camera doesnt leave the visible game
        private readonly Rectangle _worldBounds;

        // Parameters to manage camera movement speed
        private const float ZoomSpeed = 0.001f;
        private const float MoveSpeed = 8f;
        private const float LerpFactor = 0.1f;

        private readonly float worldMinZoom;

        private Vector2 _lastCameraPosition;

        public Camera2D(int viewportWidth, int viewportHeight, Rectangle worldBounds)
        {
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;
            _worldBounds = worldBounds;
            float minZoomX = (float)_viewportWidth  / _worldBounds.Width;
            float minZoomY = (float)_viewportHeight / _worldBounds.Height;
            worldMinZoom = Math.Max(minZoomX, minZoomY);
        }

        // Screen Shake
        private float _shakeTimer;
        private float _shakeIntensity;
        private Vector2 _shakeOffset;
        private Random _rnd = new Random();

        public void Shake(float intensity, float duration)
        {
            _shakeIntensity = intensity;
            _shakeTimer = duration;
        }

        public void Update(GameTime gameTime, Vector2 playerPosition, Vector2? player2Position, bool freeCamera)
        {
            // switch between FreeLook & PlayerLock
            if (freeCamera)
                Mode = CameraMode.FreeLook;
            else
                Mode = CameraMode.PlayerLock;

            // Zoom via Mousewheel
            int scrollDelta = InputHandler.Mouse.ScrollDelta;
            AdjustZoom(scrollDelta * ZoomSpeed, MIN_ZOOM, MAX_ZOOM);

            // Select camera mode
            if (Mode == CameraMode.PlayerLock)
            {
                if (player2Position == null)
                {
                    _lastCameraPosition = playerPosition;
                }
                if (player2Position != null)
                {
                    AdjustZoom(playerPosition, (Vector2)player2Position, 0.2f, MAX_ZOOM);
                }
                SmoothFollow(_lastCameraPosition);
            }
            else
            {
                HandleFreeLookInput();
            }

            // Handle Shake
            if (_shakeTimer > 0)
            {
                _shakeTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_shakeTimer <= 0)
                {
                    _shakeTimer = 0;
                    _shakeOffset = Vector2.Zero;
                }
                else
                {
                    float x = (float)(_rnd.NextDouble() * 2 - 1) * _shakeIntensity;
                    float y = (float)(_rnd.NextDouble() * 2 - 1) * _shakeIntensity;
                    _shakeOffset = new Vector2(x, y);
                }
            }

            // Make sure camera does not leave gameworld
            ClampToWorld();
        }

        // Camera position follows target with small delay to make smooth camera movements (lerp)
        private void SmoothFollow(Vector2 target)
        {
            Position = Vector2.Lerp(Position, target, LerpFactor);
        }

        // Move camera separately from player
        private void HandleFreeLookInput()
        {
            float adjustedSpeed = MoveSpeed / Zoom;
            Vector2 pos = Position;
            if (InputHandler.IsHeld(GameAction.MOVE_LEFT))  pos.X -= adjustedSpeed;
            if (InputHandler.IsHeld(GameAction.MOVE_RIGHT)) pos.X += adjustedSpeed;
            if (InputHandler.IsHeld(GameAction.MOVE_UP))    pos.Y -= adjustedSpeed;
            if (InputHandler.IsHeld(GameAction.MOVE_DOWN)) pos.Y += adjustedSpeed;
            Position = pos;
        }

        // This is mainly used to handle the Zoom while having 2 players
        private void AdjustZoom(Vector2 playerPosition, Vector2 player2Position, float minZoom, float maxZoom)
        {
            _lastCameraPosition = Maths.Middle(playerPosition, player2Position);
            float xDistance = Math.Abs(playerPosition.X - player2Position.X);
            float yDistance = Math.Abs(playerPosition.Y - player2Position.Y);
            float zoomX = Math.Abs(_viewportWidth - _TWO_PLAYER_CAMERA_PADDING) / xDistance;
            float zoomY = Math.Abs(_viewportHeight - _TWO_PLAYER_CAMERA_PADDING) / yDistance;

            float targetZoom = Math.Min(zoomX, zoomY);
            Zoom = MathHelper.Clamp(targetZoom, minZoom, maxZoom);
        }

        // This is mainly used to handle the Zoom of one player
        private void AdjustZoom(float amount, float minV, float maxV)
        {
            Zoom = MathHelper.Clamp(Zoom + amount, minV, maxV);
        }

        // Defines borders for camera movement in gameworld
        private void ClampToWorld()
        {
            float halfWidth = (_viewportWidth / 2f) / Zoom;
            float halfHeight = (_viewportHeight / 2f) / Zoom;

            float minX = _worldBounds.Left + halfWidth;
            float maxX = _worldBounds.Right - halfWidth;
            float minY = _worldBounds.Top + halfHeight;
            float maxY = _worldBounds.Bottom - halfHeight;

            Vector2 pos = Position;
            pos.X = MathHelper.Clamp(pos.X, minX, maxX);
            pos.Y = MathHelper.Clamp(pos.Y, minY, maxY);
            Position = pos;
        }

        // Calculates new size and position of all objects on screen
        public Matrix GetTransform()
        {
            Vector2 screenCenter = new Vector2(_viewportWidth / 2f, _viewportHeight / 2f);
            // Add shake offset to position
            Vector2 transformPos = Position + _shakeOffset;
            return
                Matrix.CreateTranslation(new Vector3(-transformPos, 0f)) *
                Matrix.CreateScale(Zoom, Zoom, 1f) *
                Matrix.CreateTranslation(new Vector3(screenCenter, 0f));
        }
    }
}
