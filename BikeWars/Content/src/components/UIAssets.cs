using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BikeWars.Content.components
{
    public static class UIAssets
    {
        public static SpriteFont DefaultFont { get; private set; }

        public static void Load(ContentManager content)
        {
            DefaultFont = content.Load<SpriteFont>("assets/fonts/Arial");
        }
    }
}