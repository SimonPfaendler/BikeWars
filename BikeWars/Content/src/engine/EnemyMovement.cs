using System;
using System.Collections.Generic;
using BikeWars.Content.engine.interfaces;
using BikeWars.Content.managers;
using BikeWars.Content.entities.interfaces;
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
    private const int LocalGridSize = 21; // 7x7 nodes A*
    private Point _lastPlayerGrid = new Point(-1, -1);
    private float _repathTimer = 0f;
    private const float RepathInterval = 0.25f;

    private static readonly Random _rng = new();

    private List<ICollider> _nearbyEnemies = new(32);

    private const float NodeReachDistance = 10f;
    private const float StopRadius = 10f;

   // enemies avoid each other
   private const float AvoidRadius = 60f;
   private const float AvoidStrength = 5.0f;
   private const float AvoidMax = 6f;
   private readonly float _slotAngle;

   public Vector2 PlayerPosition {get; set;}
   public Vector2 EnemyPosition {get; set;}

   // repath scheduler
   private readonly RepathScheduler _repathScheduler;

   public bool IsRepathQueued { get; set; }  // scheduler needs this

   private Point _pendingStartGrid;
   private Point _pendingTargetGrid;
   private bool _hasPendingRepath;


   // sets up the enemy movement system and stores pathfinding + grid helpers.
   public EnemyMovement(bool canMove, bool isMoving, PathFinding pathFinding,
       CollisionManager gridMapper, RepathScheduler repathScheduler)
   {
       Direction = Vector2.Zero;
       CanMove = canMove;
       IsMoving = isMoving;


       _pathFinding = pathFinding;
       _gridMapper = gridMapper;
        // Stagger initial repath timers to distribute load
        _repathTimer = (float)(_rng.NextDouble() * RepathInterval);

        _slotAngle = (float)(_rng.NextDouble() * MathF.Tau);
        _repathScheduler = repathScheduler;
   }

    // Force a repath on the next update
    public void ForceRepath()
    {
        _repathTimer = 0f;
        _currentPath.Clear();
        _pathIndex = 0;

        _hasPendingRepath = false;
        IsRepathQueued = false;
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

       var playerGridRaw = _gridMapper.WorldToGrid(PlayerPosition);
       var playerGrid = OffsetTargetGrid(playerGridRaw);

       // fallback if the offset goes outside the map or onto a blocked tile
       if (!_pathFinding.IsInsideGrid(playerGrid.X, playerGrid.Y) ||
           !_pathFinding.GetNode(playerGrid.X, playerGrid.Y).Walkable)
       {
           playerGrid = playerGridRaw;
       }


       bool timeUp = _repathTimer <= 0f;


       bool needNewPath =
           !_hasPendingRepath && (
               _currentPath == null ||
               _currentPath.Count == 0 ||
               timeUp
           );


       if (needNewPath)
       {
           _pendingStartGrid = enemyGrid;
           _pendingTargetGrid = playerGrid;

           if (_repathScheduler != null && _repathScheduler.Request(this))
           {
               _hasPendingRepath = true;
               _repathTimer = RepathInterval;
           }
           else
           {
               // scheduler full or already queued
               // try again soon (small delay so you don't spam)
               _repathTimer = 0.1f;
           }
       }

       Vector2 pathDir = DirectionAlongPath();
       if (pathDir == Vector2.Zero)
       {
           if (_hasPendingRepath)
           {
               // waiting for scheduler -> take a small obstacle-safe grid step
               pathDir = FallbackStepOnGrid(enemyGrid, playerGrid);
           }
           else
           {
               // not waiting -> ok to chase directly
               _currentPath.Clear();
               _pathIndex = 0;
               pathDir = FallbackStepOnGrid(enemyGrid, playerGrid);
           }
       }

       Vector2 avoidDir = ComputeAvoidance();
       // avoidDir *= 0.6f;

       // Stops the enemy from moving backwards when avoiding others,
       // so it mainly moves forward and only steps to the side.
       // if (pathDir != Vector2.Zero && avoidDir != Vector2.Zero)
       // {
       //     float dot = Vector2.Dot(avoidDir, pathDir);
       //     if (dot < 0f)
       //     {
       //         // remove backward component
       //         avoidDir -= pathDir * dot;
       //     }
       // }

       // commented for debug, bypasses all the avoidance algo
       Vector2 combinedPath = pathDir + avoidDir;
       // Vector2 combinedPath = pathDir;

       if (combinedPath.LengthSquared() < 0.0001f)
           combinedPath = pathDir;

       if (combinedPath != Vector2.Zero)
           combinedPath.Normalize();

       Direction = combinedPath;

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
       Vector2 target = OffsetTarget(PlayerPosition);
       Vector2 toTarget = target - EnemyPosition;


       if (toTarget.LengthSquared() < StopRadius * StopRadius)
           return Vector2.Zero;


       toTarget.Normalize();
       return toTarget;
   }

   // the target will be a radius around the player not only the exact position of the player
   // so that the enemies "avoid" each other
   private Vector2 OffsetTarget(Vector2 playerPosition)
   {
       float angle = _slotAngle;
       float radius = 35f;

       Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

       return playerPosition + offset;
   }

   // makes the enemies aim for different tiles around the player
   // to prevent them from stacking on one position
   private Point OffsetTargetGrid(Point playerGrid)
   {
       int ox = (int)MathF.Round(MathF.Cos(_slotAngle) * 2f);
       int oy = (int)MathF.Round(MathF.Sin(_slotAngle) * 2f);
       return new Point(playerGrid.X + ox, playerGrid.Y + oy);
   }


   // helper function that computes avoidance direction
   private Vector2 ComputeAvoidance()
   {
       if (_gridMapper.DynamicHash == null)
           return Vector2.Zero;

       Vector2 avoid = Vector2.Zero;
       _nearbyEnemies.Clear();
       _gridMapper.DynamicHash.QueryNearby(EnemyPosition, 3, _nearbyEnemies);

       foreach (ICollider otherCollider in _nearbyEnemies)
       {
           if (otherCollider.Layer != CollisionLayer.CHARACTER)
               continue;

           if (otherCollider.Owner is not CharacterBase otherChar)
               continue;

           if (otherChar.Movement is not EnemyMovement)
               continue;

           Vector2 diff = EnemyPosition - otherChar.Transform.Position;
           float distSquared = diff.LengthSquared();

           float avoidRadiusSq = AvoidRadius * AvoidRadius;

           if (distSquared < 0.0001f || distSquared >= avoidRadiusSq)
               continue;

           float dist = (float)Math.Sqrt(distSquared);
           diff /= dist;

           float strength = (AvoidRadius - dist) / AvoidRadius;
           avoid += diff * strength;
       }

       if (avoid == Vector2.Zero)
           return Vector2.Zero;

       avoid *= AvoidStrength;

       if (avoid.LengthSquared() > AvoidMax * AvoidMax)
       {
           avoid.Normalize();
           avoid *= AvoidMax;
       }

       return avoid;
   }

   public void DoRepathNow()
   {
       if (!_hasPendingRepath)
           return;

       RecalculatePath(_pendingStartGrid, _pendingTargetGrid);
       _hasPendingRepath = false;
   }


   private Vector2 FallbackStepOnGrid(Point enemyGrid, Point targetGrid)
   {
       // If already at target, stop
       if (enemyGrid == targetGrid)
           return Vector2.Zero;

       // 8 directions (including diagonals)
       int[] dxs = { -1, 0, 1, -1, 1, -1, 0, 1 };
       int[] dys = { -1, -1, -1, 0, 0, 1, 1, 1 };

       Point best = enemyGrid;
       float bestScore = float.PositiveInfinity;

       // unique offset to stop enemies from walking in a single line
       float noise = (this.GetHashCode() % 100) / 1000.0f;

       for (int i = 0; i < 8; i++)
       {
           int nx = enemyGrid.X + dxs[i];
           int ny = enemyGrid.Y + dys[i];

           if (!_pathFinding.IsInsideGrid(nx, ny))
               continue;

           // don't step into walls
           if (!_pathFinding.GetNode(nx, ny).Walkable)
               continue;

           // check if diagonal nodes are blocked
           if (dxs[i] != 0 && dys[i] != 0)
           {
               // manually check the 2 neighbours
               bool s1 = _pathFinding.GetNode(enemyGrid.X + dxs[i], enemyGrid.Y).Walkable;
               bool s2 = _pathFinding.GetNode(enemyGrid.X, enemyGrid.Y + dys[i]).Walkable;

               // either one of them is blocked we stop
               if (!s1 || !s2)
                   continue;
           }

           // score = distance to target (smaller is better)
           float ddx = targetGrid.X - nx;
           float ddy = targetGrid.Y - ny;
           float score = ddx * ddx + ddy * ddy;

           // add the noise to score to stop 2 enemies from choosing the same path
           score += noise;

           if (score < bestScore)
           {
               bestScore = score;
               best = new Point(nx, ny);
           }
       }

       if (best == enemyGrid)
           return Vector2.Zero;

       // move toward center of the chosen neighbor tile
       Vector2 bestPos = _gridMapper.GridToWorldCenter(_pathFinding.GetNode(best.X, best.Y));
       Vector2 dir = bestPos - EnemyPosition;

       if (dir.LengthSquared() < 0.0001f)
           return Vector2.Zero;

       dir.Normalize();
       return dir;
   }



   private bool UpdateMoving() => Direction != Vector2.Zero;


   public override void Update(GameTime gameTime)
   {
       IsMoving = UpdateMoving();
   }


}
