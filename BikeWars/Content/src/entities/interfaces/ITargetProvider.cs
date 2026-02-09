using BikeWars.Entities.Characters;
using Microsoft.Xna.Framework;

public interface ITargetProvider
{
    Player? GetTargetPlayer(Vector2 fromPosition);
}