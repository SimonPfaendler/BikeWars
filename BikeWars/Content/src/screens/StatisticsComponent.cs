using BikeWars.Content.engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.screens;
public class StatisticsComponent
{
    public Statistic statistic {get; set;}
    public StatisticsComponent(Statistic s)
    {
        statistic = s;
    }
    public void Draw(SpriteBatch sb, Texture2D basicTexture, Color overlayColor, Vector2 drawPos, SpriteFont font)
    {
        Rectangle box = new Rectangle((int)drawPos.X, (int)drawPos.Y, 500, 100);
        sb.Draw(basicTexture, box, overlayColor);

        sb.DrawString(font, $"Kills: {statistic.Kills}", new Vector2(box.X, box.Y), Color.White);
        sb.DrawString(font, $"Schaden hinzugefuegt: {statistic.DealtDamage}", new Vector2(box.X, box.Y + 20), Color.White);
        sb.DrawString(font, $"Schaden erhalten: {statistic.TookDamage}", new Vector2(box.X, box.Y + 40), Color.White);
        sb.DrawString(font, $"XP: {statistic.XP}", new Vector2(box.X, box.Y + 60), Color.White);
        sb.DrawString(font, $"Gespielte Zeit: {statistic.TimeToMinuteDisplay()}", new Vector2(box.X, box.Y + 80), Color.White);
        sb.DrawString(font, $"Level: {statistic.Level}", new Vector2(box.X, box.Y + 10), Color.White);
    }
}