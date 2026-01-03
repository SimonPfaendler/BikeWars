using System;
using System.Collections.Generic;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
using Microsoft.Xna.Framework;


// Handles the enemy movement:
// the enemy chases the player
// if it collides with an object on the map, the enemy will turn 30 degrees
// to the right and try again


namespace BikeWars.Content.engine;


public class EnemyMovement : MovementBase
{
   private readonly PathFinding _pathFinding;
   private readonly CollisionManager _gridMapper;


   private List<Node> _currentPath = new();
   private int _pathIndex = 0;
   public IReadOnlyList<Node> CurrentPath => _currentPath;
   public int CurrentPathIndex => _pathIndex;
   private const int QueryRadius = 3; // nearby collisions
   private const int LocalGridSize = 40; // 7x7 nodes A*
   private Point _lastPlayerGrid = new Point(-1, -1);
   private float _repathTimer = 0f;
   private const float RepathInterval = 1f;


   private const float NodeReachDistance = 18f;
   private const float StopRadius = 10f;


   public Vector2 PlayerPosition {get; set;}
   public Vector2 EnemyPosition {get; set;}


   // sets up the enemy movement system and stores pathfinding + grid helpers.
   public EnemyMovement(bool canMove, bool isMoving, PathFinding pathFinding,
       CollisionManager gridMapper)
   {
       Direction = Vector2.Zero;
       CanMove = canMove;
       IsMoving = isMoving;


       _pathFinding = pathFinding;
       _gridMapper = gridMapper;
   }

    // Force a repath on the next update
    public void ForceRepath()
    {
        _repathTimer = 0f;
        _currentPath.Clear();
        _pathIndex = 0;
    }


   // runs every frame: updates path, chooses direction, and moves the enemy.
   // A* doesn't run every frame
   public override void HandleMovement(GameTime gameTime)
   {
       if (!CanMove) return;


       // Count down the pathfinding timer using the time passed since last frame.
       float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
       _repathTimer -= dt;


       if (_repathTimer < 0f) _repathTimer = 0f;




       var enemyGrid= _gridMapper.WorldToGrid(EnemyPosition);
       var playerGrid= _gridMapper.WorldToGrid(PlayerPosition);


       bool timeUp = _repathTimer <= 0f;


       bool needNewPath =
           _currentPath == null ||
           _currentPath.Count == 0 ||
           timeUp;


       if (needNewPath)
       {
           RecalculatePath(enemyGrid, playerGrid);
           _repathTimer = RepathInterval;
       }
       Direction = DirectionAlongPath();
       if (Direction == Vector2.Zero)
       {
           // optional: clear path when done, then just chase directly
           _currentPath.Clear();
           _pathIndex = 0;
           Direction = DirectionToTarget();
       }
       Update(gameTime);
   }


   // asks A* for a new path to the player and resets the path index.
   private void RecalculatePath(Point enemyGrid, Point playerGrid)
   {
       int localSize = LocalGridSize; // z.B. 7
       int half = localSize / 2;


       int dx = playerGrid.X - enemyGrid.X;
       int dy = playerGrid.Y - enemyGrid.Y;


       // if the player is outside the local grid range, use global A* instead
       if (Math.Abs(dx) > half || Math.Abs(dy) > half)
       {
           _currentPath = _pathFinding.FindPath(
               enemyGrid.X, enemyGrid.Y,
               playerGrid.X, playerGrid.Y
           );


           _pathIndex = 0;
           _lastPlayerGrid = playerGrid;
           return;
       }


       // 1. Local Grid erstellen
       Node[,] localGrid = new Node[localSize, localSize];
       for (int y = 0; y < localSize; y++)
       {
           for (int x = 0; x < localSize; x++)
           {
               int globalX = enemyGrid.X - half + x;
               int globalY = enemyGrid.Y - half + y;


               bool walkable = false;
               if (_pathFinding.IsInsideGrid(globalX, globalY))
                   walkable = _pathFinding.GetNode(globalX, globalY).Walkable;


               localGrid[x, y] = new Node(x, y, walkable);
           }
       }


       // 2. PathFinding für Local Grid
       PathFinding localFinder = new PathFinding(localGrid);


       // 3. Start & Ziel auf Local Grid abbilden
       Point startLocal = new Point(half, half);
       Point targetLocal = new Point(
           half + (playerGrid.X - enemyGrid.X),
           half + (playerGrid.Y - enemyGrid.Y)
       );


       var localPath = localFinder.FindPath(startLocal.X, startLocal.Y, targetLocal.X, targetLocal.Y);
       _currentPath.Clear();


       foreach (var node in localPath)
       {
           int globalX = node.X + (enemyGrid.X - half);
           int globalY = node.Y + (enemyGrid.Y - half);
           _currentPath.Add(_pathFinding.GetNode(globalX, globalY));
       }


       _pathIndex = 0;
       _lastPlayerGrid = playerGrid;
   }


   // returns the direction toward the next node in the path, or moves to next node.
   private Vector2 DirectionAlongPath()
   {
       if (_currentPath == null || _currentPath.Count == 0)
           return Vector2.Zero;


       if (_pathIndex < 0 || _pathIndex >= _currentPath.Count)
           return Vector2.Zero;


       Node targetNode = _currentPath[_pathIndex];
       Vector2 targetPos = _gridMapper.GridToWorldCenter(targetNode);


       Vector2 toTarget = targetPos - EnemyPosition;


       // If we’re close enough to this node, go to the next
       if (toTarget.LengthSquared() < NodeReachDistance * NodeReachDistance)
       {
           _pathIndex++;
           if (_pathIndex >= _currentPath.Count)
               return Vector2.Zero;


           targetNode = _currentPath[_pathIndex];
           targetPos = _gridMapper.GridToWorldCenter(targetNode);
           toTarget = targetPos - EnemyPosition;
       }


       if (toTarget == Vector2.Zero)
           return Vector2.Zero;


       toTarget.Normalize();
       return toTarget;
   }


   // gives a direction pointing straight toward the player if not too close.
   private Vector2 DirectionToTarget()
   {
       Vector2 toTarget = PlayerPosition - EnemyPosition;


       if (toTarget.LengthSquared() < StopRadius * StopRadius)
           return Vector2.Zero;


       toTarget.Normalize();
       return toTarget;
   }


   private bool UpdateMoving() => Direction != Vector2.Zero;


   public override void Update(GameTime gameTime)
   {
       IsMoving = UpdateMoving();
   }


}
