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
    TRAM
}