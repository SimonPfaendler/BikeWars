using System.Collections.Generic;
using System.Numerics;

public enum Direction
{
    LEFT, RIGHT, UP, DOWN
}

public static class DirectionHelper
{
    public static readonly Dictionary<Direction, Vector2> _directions = new()
    {
        {Direction.LEFT, new Vector2(-1, 0)},
        {Direction.RIGHT, new Vector2(1, 0)},
        {Direction.UP, new Vector2(0, -1)},
        {Direction.DOWN, new Vector2(0, 1)},
    };
    public static Vector2 Get(Direction direction) => _directions[direction];
}
