using System;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.entities.projectiles
{
    public class ThrowBeer : ThrowObject
    {
        public event Action<Vector2> OnBeerLanded;

        private readonly bool _emitLandingEvent;

        public ThrowBeer(Vector2 start, Vector2 target, object owner, bool emitLandingEvent = true)
            : base(start, target, owner, textureKey: "Beer_throw", damage: 12, speed: 100f, arcScale: 1.2f, lingerDuration: 0.25f)
        {
            _emitLandingEvent = emitLandingEvent;
        }

        protected override void OnLanded()
        {
            base.OnLanded();
            if (!_emitLandingEvent) return;

            Vector2 offset = new Vector2(-10f, -15f); //corrects LandedBeer Spawn position
            OnBeerLanded?.Invoke(_target + offset);
        }
    }
}