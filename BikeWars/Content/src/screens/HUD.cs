using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BikeWars.Entities.Characters;
using Autofac.Util;
using System;

namespace BikeWars.Content.screens
{
    public class HUD: IDisposable
    {
        private Texture2D _sheet;
        private readonly Rectangle _hpfill;
        private readonly Rectangle _sprintIcon;
        private readonly Rectangle _xpfill;
        private readonly Rectangle _bikefill;
        public Vector2 Position;


        public HUD()
        {
            _xpfill = new Rectangle(44, 25, 100, 5);
            _hpfill = new Rectangle(32, 34, 118, 17);
            _bikefill = new Rectangle(48, 60, 102, 8);
            _sprintIcon = new Rectangle(12, 36, 16, 15);
            Position = new Vector2(0, 0);
        }

        public void LoadContent(Texture2D sheet)
        {
            _sheet = sheet;
        }
        private const float Scale = 2f;
        public void Draw(SpriteBatch sb, Player player)
        {
            sb.Draw(_sheet, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

            float hpPercent = (float)player.Attributes.Health / player.Attributes.MaxHealth;
            DrawCover(sb, _hpfill, hpPercent);
            float xpPercent = (float)player.XpCounter / player.XpLevelUp;
            DrawCover(sb, _xpfill, xpPercent);

            if (player.CurrentBike == null) {
                DrawCover(sb, _bikefill, 0f);
            }

            if (player.CurrentBike != null)
            {
                float bikePercent = (float)player.CurrentBike.Attributes.Health / player.CurrentBike.Attributes.MaxHealth;
                DrawCover(sb, _bikefill, bikePercent);
            }
            sb.DrawString(
                UIAssets.DefaultFont,
                $"Lvl {player.CurrentLevel}",
                Position + new Vector2(140,30),
                Color.Black
            );
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
            sb.Draw(RenderPrimitives.Pixel, dest, Color.Gray);
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
        public void Dispose()
        {
            _sheet = null;
        }
    }
}