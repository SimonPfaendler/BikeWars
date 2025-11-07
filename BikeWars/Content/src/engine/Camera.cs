using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BikeWars.Content.engine
{
    public enum CameraMode
    {
        PlayerLock,
        FreeLook
    }

    public class Camera2D
    {
        // === Felder & Eigenschaften ===
        public Vector2 Position { get; set; } = Vector2.Zero; // Where the camera points to
        public float Zoom { get; set; } = 1f;
        public float Rotation { get; set; } = 0f;

        public CameraMode Mode { get; set; } = CameraMode.PlayerLock;

        private readonly int _viewportWidth;
        private readonly int _viewportHeight;
        private readonly Rectangle _worldBounds;

        private int _prevScrollValue;
        private const float ZoomSpeed = 0.001f;
        private const float MoveSpeed = 8f;
        private const float LerpFactor = 0.1f;

        // === Konstruktor ===
        public Camera2D(int viewportWidth, int viewportHeight, Rectangle worldBounds)
        {
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;
            _worldBounds = worldBounds;
            _prevScrollValue = Mouse.GetState().ScrollWheelValue;
        }

        // === Hauptmethoden ===

        public void Update(GameTime gameTime, Vector2 playerPosition, bool freeCamera)
        {
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            // Umschalten zwischen FreeLook & PlayerLock
            if (freeCamera) 
                Mode = CameraMode.FreeLook;
            else
                Mode = CameraMode.PlayerLock;

            // Zoom per Mausrad
            int scrollDelta = mouse.ScrollWheelValue - _prevScrollValue;
            _prevScrollValue = mouse.ScrollWheelValue;
            AdjustZoom(scrollDelta * ZoomSpeed);

            // Kamera steuern
            if (Mode == CameraMode.PlayerLock)
                SmoothFollow(playerPosition);
            else
                HandleFreeLookInput(keyboard);

            ClampToWorld();
        }

        // === Spieler-Folgemodus ===
        private void SmoothFollow(Vector2 target)
        {
            Position = Vector2.Lerp(Position, target, LerpFactor);
        }

        // === Freies Bewegen ===
        private void HandleFreeLookInput(KeyboardState keyboard)
        {
            float adjustedSpeed = MoveSpeed / Zoom;
            Vector2 pos = Position;

            if (keyboard.IsKeyDown(Keys.Left))  pos.X -= adjustedSpeed;
            if (keyboard.IsKeyDown(Keys.Right)) pos.X += adjustedSpeed;
            if (keyboard.IsKeyDown(Keys.Up))    pos.Y -= adjustedSpeed;
            if (keyboard.IsKeyDown(Keys.Down)) pos.Y += adjustedSpeed;

            Position = pos;
        }

        // === Zoom-Steuerung ===
        private void AdjustZoom(float amount)
        {
            Zoom += amount;
            Zoom = MathHelper.Clamp(Zoom, 0.5f, 3f);
        }

        // === Begrenzung auf Welt ===
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

        // === Transformationsmatrix für SpriteBatch ===
        public Matrix GetTransform()
        {

            Vector2 screenCenter = new Vector2(_viewportWidth / 2f, _viewportHeight / 2f);

            return
                Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
                Matrix.CreateScale(Zoom, Zoom, 1f) *
                Matrix.CreateTranslation(new Vector3(screenCenter, 0f));

        }
    }
}
