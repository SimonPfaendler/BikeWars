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

    public void Draw(SpriteBatch spriteBatch, Texture2D overlay, int boxLeft, int boxTop, SpriteFont font)
    {
        Rectangle box = new Rectangle(boxLeft, boxTop, 500, 250);
        spriteBatch.Draw(overlay, box, Color.DarkSlateGray);
        spriteBatch.DrawString(font, $"Kills: {statistic.Kills}", new Vector2(box.Left, box.Top), Color.White);
        spriteBatch.DrawString(font, $"Schaden hinzugefuegt: {statistic.DealtDamage}", new Vector2(box.Left, box.Top + 20), Color.White);
        spriteBatch.DrawString(font, $"Schaden erhalten: {statistic.TookDamage}", new Vector2(box.Left, box.Top + 40), Color.White);
        spriteBatch.DrawString(font, $"Erfahrungspunkte: {statistic.XP}", new Vector2(box.Left, box.Top + 60), Color.White);
        spriteBatch.DrawString(font, $"Level: {statistic.Level}", new Vector2(box.Left, box.Top + 80), Color.White);
    }
}