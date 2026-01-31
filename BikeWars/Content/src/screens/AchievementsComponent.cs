using BikeWars.Content.engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.screens;
public class AchievementsComponent
{
    private static int HEIGHT_OF_COMPONENT = 4 * 20; // Check this in AchievementsScreen. Not optimla but works now
    private const int PADDING = 5;
    public Achievement achievement {get; set;}
    public AchievementsComponent(Achievement a)
    {
        achievement = a;
    }
    public void Draw(SpriteBatch sb, Texture2D basicTexture, Color overlayColor, Vector2 drawPos, SpriteFont font)
    {
        if (achievement.Id == 0)return;
        Rectangle box = new Rectangle((int)drawPos.X, (int)drawPos.Y, 500, HEIGHT_OF_COMPONENT);
        Rectangle pictureBox = new Rectangle((int)drawPos.X, (int)drawPos.Y, 100, HEIGHT_OF_COMPONENT); // Should be on the left side
        sb.Draw(basicTexture, box, overlayColor);

        Color titleColor = Color.Red;
        if (achievement.Succeeded)
        {
            titleColor = Color.Green;
        }

        sb.DrawString(font, $"{achievement.Name}", new Vector2(box.X + PADDING + pictureBox.Width, box.Y), titleColor);
        sb.DrawString(font, $"{achievement.Description},", new Vector2(box.X + PADDING + pictureBox.Width, box.Y + 20), Color.Black);
        if (achievement.Picture == null) return;

        if (!achievement.Succeeded)
        {
            achievement.Picture = CreateGrayTexture(achievement.Picture);
        }
        sb.Draw(achievement.Picture, pictureBox, Color.White);
    }
    private Texture2D CreateGrayTexture(Texture2D picture)
    {
        Color[] data = new Color[picture.Width * picture.Height];
        picture.GetData(data);

        for (int i = 0; i < data.Length; i++)
        {
            int g = (int)(data[i].R * 0.299f +
                        data[i].G * 0.587f +
                        data[i].B * 0.114f);
            data[i] = new Color(g, g, g, data[i].A);
        }
        Texture2D gray = new Texture2D(
            picture.GraphicsDevice,
            picture.Width,
            picture.Height
        );
        gray.SetData(data);
        return gray;
    }
}