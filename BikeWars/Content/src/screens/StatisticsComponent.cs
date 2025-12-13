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
    }
}