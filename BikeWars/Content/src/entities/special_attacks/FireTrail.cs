using Microsoft.Xna.Framework;
using BikeWars.Entities.Characters;

namespace BikeWars.Content.entities.items
{
    public class FireTrail : TrailBase
    {
        private const string SPRITE_KEY = "FireTrail";
        private const int DAMAGE = 12;
        private const float DURATION = 3.0f;

        public FireTrail(Player player, Vector2 dir)
            : base(player, dir, SPRITE_KEY, DAMAGE, DURATION)
        {
        }
    }
}