using System.Collections.Generic;
using Microsoft.Xna.Framework;



namespace BikeWars.Content.components;
  
public enum MoveDirection
{
    LEFT, RIGHT, UP, DOWN
}

public static class DirectionHelper
{
    public static Dictionary<MoveDirection, Vector2> _directions = new()
    {
        {MoveDirection.LEFT, new Vector2(-1, 0)},
        {MoveDirection.RIGHT, new Vector2(1, 0)},
        {MoveDirection.UP, new Vector2(0, -1)},
        {MoveDirection.DOWN, new Vector2(0, 1)},
    };
    public static Vector2 Get(MoveDirection direction) => _directions[direction];
}

