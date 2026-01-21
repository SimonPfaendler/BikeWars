using Microsoft.Xna.Framework;
using BikeWars.Entities.Characters;

namespace BikeWars.Content.entities.items
{
    public class IceTrail : TrailBase
    {
        private const string SPRITE_KEY = "IceTrail";
        private const int DAMAGE = 10;
        private const float DURATION = 3.0f;

        public IceTrail(Player player, Vector2 dir)
            : base(player, dir, SPRITE_KEY, DAMAGE, DURATION)
        {
        }
    }
}
