using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Entities.Characters;

namespace BikeWars.Content.screens
{
    public class HUD
    {
        private readonly Texture2D _sheet;
        private readonly Texture2D _pixel;
        //private readonly Rectangle _xpFill;
        private readonly Rectangle _hpfill;
        private readonly Rectangle _sprintIcon;
        private readonly Rectangle _xpfill;


        public Vector2 Position;


        public HUD(Texture2D sheet)
        {
            _sheet = sheet;
            _pixel = new Texture2D(sheet.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
            _xpfill = new Rectangle(37, 30, 91, 2);
            _hpfill = new Rectangle(27, 37, 108, 10);
            _sprintIcon = new Rectangle(32, 58, 11, 9);
            Position = new Vector2(0, 0);
        }
        private const float Scale = 2f;
        public void Draw(SpriteBatch sb, Player player)
        {
            
            sb.Draw(_sheet, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

            float hpPercent = (float)player.Health / player.MaxHealth;
            DrawCover(sb, _hpfill, hpPercent);
            float xpPercent = (float)player.XpCounter / player.XpLevelUp;
            DrawCover(sb, _xpfill, xpPercent);
            DrawSprintIcon(sb, player);
        }

        private void DrawCover(SpriteBatch sb, Rectangle src, float percent)
        {
            percent = MathHelper.Clamp(percent, 0f, 1f);

            int fullWidth = src.Width;
            int lostWidth = (int)(fullWidth * (1f - percent));

            if (lostWidth <= 0)
                return;

            // Skalierte Werte
            int scaledFullWidth = (int)(fullWidth * Scale);
            int scaledLostWidth = (int)(lostWidth * Scale);

            int scaledX = (int)(Position.X + src.X * Scale);
            int scaledY = (int)(Position.Y + src.Y * Scale);
            int scaledHeight = (int)(src.Height * Scale);

            var dest = new Rectangle(
                scaledX + (scaledFullWidth - scaledLostWidth),
                scaledY,
                scaledLostWidth,
                scaledHeight
            );

            sb.Draw(_pixel, dest, Color.Gray);
        }
        
        private void DrawSprintIcon(SpriteBatch sb, Player player)
        {
            // Icon position (scaled)
            Vector2 iconPos = Position + new Vector2(_sprintIcon.X * Scale, _sprintIcon.Y * Scale);

            Color tint;

            if (player.CooldownTimer() > 0)
            {
                tint = Color.Orange;  
            }
            else
            {
                tint = Color.White;
            }

            sb.Draw(_sheet, iconPos, _sprintIcon, tint, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
        }

    }
}