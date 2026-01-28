using BikeWars.Content.engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.screens;
public class StatisticsComponent
{

    private static int HEIGHT_OF_COMPONENT = 11 * 20; // Check this in StatisticsSCreen. Not optimla but works now
    public Statistic statistic {get; set;}
    public StatisticsComponent(Statistic s)
    {
        statistic = s;
    }
    public void Draw(SpriteBatch sb, Texture2D basicTexture, Color overlayColor, Vector2 drawPos, SpriteFont font)
    {
        Rectangle box = new Rectangle((int)drawPos.X, (int)drawPos.Y, 500, HEIGHT_OF_COMPONENT);
        sb.Draw(basicTexture, box, overlayColor);

        sb.DrawString(font, $"Kills: {statistic.Kills}", new Vector2(box.X, box.Y), Color.White);
        sb.DrawString(font, $"Regulaere Kills: {statistic.RegularKills}", new Vector2(box.X, box.Y + 20), Color.White);
        sb.DrawString(font, $"Schaden hinzugefuegt: {statistic.DealtDamage}", new Vector2(box.X, box.Y + 40), Color.White);
        sb.DrawString(font, $"Schaden erhalten: {statistic.TookDamage}", new Vector2(box.X, box.Y + 60), Color.White);
        sb.DrawString(font, $"XP: {statistic.XP}", new Vector2(box.X, box.Y + 80), Color.White);
        sb.DrawString(font, $"Level: {statistic.Level}", new Vector2(box.X, box.Y + 100), Color.White);
        sb.DrawString(font, $"Gespielte Zeit: {statistic.TimeToMinuteDisplay()}", new Vector2(box.X, box.Y + 120), Color.White);
        sb.DrawString(font, $"Spieler gestorben: {statistic.DeathCount}", new Vector2(box.X, box.Y + 140), Color.White);
        sb.DrawString(font, $"Gefallene Schuesse: {statistic.ShotsFired}", new Vector2(box.X, box.Y + 160), Color.White);
        sb.DrawString(font, $"Zielsicherheit: {statistic.Accuracy():0.00}%", new Vector2(box.X, box.Y + 180), Color.White);
        sb.DrawString(font, $"Fahrradreperaturen: {statistic.Repairs}", new Vector2(box.X, box.Y + 200), Color.White);
        sb.DrawString(font, $"Zeit um erstes Fahrrad zu finden: {statistic.TimeToFindBikeMinuteDisplay()}", new Vector2(box.X, box.Y + 220), Color.White);
    }
}