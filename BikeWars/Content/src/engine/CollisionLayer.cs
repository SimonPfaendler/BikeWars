namespace BikeWars.Content.engine;

public enum CollisionLayer
{
    NONE,
    PLAYER,
    CHARACTER,
    PROJECTILE,
    ITEM,
    WALL,
    WATER,
    TERRAIN,
    SPAWNENEMIES,
    INTERACT, // Like interacting while walking on it. Similar like Terrain
    AOE, // area-of-affect damange
    TOWER,
    TRIGGER, // Should be used for the Achievements or Statistic. Mostly if someone walks on. Similar like Terrain
    TRAM,
    CAR
}