using System;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.projectiles
{
    public class ThrowBeer : ThrowObject
    {
        public event Action<Vector2> OnBeerLanded;

        public ThrowBeer(Vector2 start, Vector2 target, object owner)
            : base(start, target, owner, textureKey: "Beer_throw", damage: 12, speed: 500f, arcScale: 1.2f,
                lingerDuration: 0.25f)
        {
        }

        protected override void OnLanded()
        {
            base.OnLanded();
            Vector2 offset = new Vector2(-10f, -15f); //corrects LandedBeer Spawn position
            OnBeerLanded?.Invoke(_target + offset);
        }
    }
}