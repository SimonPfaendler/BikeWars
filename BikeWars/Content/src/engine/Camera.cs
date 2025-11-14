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
        public Vector2 Position { get; set; } = Vector2.Zero; // Where the camera points to
        public float Zoom { get; set; } = 1f;

        public CameraMode Mode { get; set; } = CameraMode.PlayerLock;

        // Defines the visible screen window size
        private readonly int _viewportWidth;
        private readonly int _viewportHeight;

        // Defines the border of the game world so the camera doesnt leave the visible game
        private readonly Rectangle _worldBounds;

        // Parameters to manage camera movement speed
        private int _prevScrollValue;
        private const float ZoomSpeed = 0.001f;
        private const float MoveSpeed = 8f;
        private const float LerpFactor = 0.1f;

        public Camera2D(int viewportWidth, int viewportHeight, Rectangle worldBounds)
        {
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;
            _worldBounds = worldBounds;
            _prevScrollValue = Mouse.GetState().ScrollWheelValue;
        }


        public void Update(GameTime gameTime, Vector2 playerPosition, bool freeCamera)
        {
            // switch between FreeLook & PlayerLock
            if (freeCamera)
                Mode = CameraMode.FreeLook;
            else
                Mode = CameraMode.PlayerLock;

            // Zoom via Mousewheel
            int scrollDelta = InputHandler.Mouse.ScrollDelta;
            AdjustZoom(scrollDelta * ZoomSpeed);

            // Select camera mode
            if (Mode == CameraMode.PlayerLock)
                SmoothFollow(playerPosition);
            else
                HandleFreeLookInput();

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

        private void AdjustZoom(float amount)
        {
            Zoom += amount;
            Zoom = MathHelper.Clamp(Zoom, 0.5f, 3f);
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

            return
                Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
                Matrix.CreateScale(Zoom, Zoom, 1f) *
                Matrix.CreateTranslation(new Vector3(screenCenter, 0f));

        }
    }
}
