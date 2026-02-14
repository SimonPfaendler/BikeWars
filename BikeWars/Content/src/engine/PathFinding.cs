using System;
using System.Collections.Generic;

namespace BikeWars.Content.engine;
public class Node
{
    public int X { get; }
    public int Y { get; }

    public bool Walkable { get; set; }

    public int G_cost { get; set; }
    public int H_cost { get; set; }
    public int F_cost => G_cost + H_cost;
    public Node Parent_node { get; set; }

    public int SearchId { get; set; } = -1;
    public int ClosedId { get; set; }

    // constructor
    public Node(int x, int y, bool walkable)
    {
        X = x;
        Y = y;
        Walkable = walkable;
        G_cost = int.MaxValue;
    }
}

public class PathFinding
{
    private readonly Node[,] _grid;
    public readonly int _width;
    public readonly int _height;
    private int _searchId;

    private readonly Node[] _neighbourBuffer = new Node[8];

    public PathFinding(Node[,] grid)
    {
        _grid = grid;
        _width = grid.GetLength(0);
        _height = grid.GetLength(1);
        _searchId = 0;
    }

    public Node GetNode(int x, int y)
    {
        return _grid[x,y];
    }

    // checks whether a tile position is inside the grid and not outside the map
    public bool IsInsideGrid(int x, int y) =>
        x >= 0 && x < _width && y >= 0 && y < _height;

    // checks if the movement direction is diagonal.
    private bool IsDiagonal(int dx, int dy) =>
        dx != 0 && dy != 0;

    // checks whether a diagonal move is allowed by verifying the two side tiles
    private bool IsDiagonalPossible(Node node, int dx, int dy)
    {
        var side1 = _grid[node.X + dx, node.Y];
        var side2 = _grid[node.X, node.Y + dy];

        return side1.Walkable && side2.Walkable;
    }

    // checks whether a tile next to the current node is a valid neighbor for A* pathfinding
    private bool IsValidNeighbour(Node node, int dx, int dy, out int nx, out int ny)
    {
        nx = node.X + dx;
        ny = node.Y + dy;

        // skip center
        if (dx == 0 && dy == 0)
            return false;

        if (!IsInsideGrid(nx, ny))
            return false;

        var neighbour = _grid[nx, ny];
        if (!neighbour.Walkable)
            return false;

        if (IsDiagonal(dx, dy) && !IsDiagonalPossible(node, dx, dy))
            return false;

        return true;
    }

    public int GetNeighbours(Node node)
    {
        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (IsValidNeighbour(node, dx, dy, out int nx, out int ny))
                {
                    _neighbourBuffer[count++] = _grid[nx, ny];
                }
            }
        }

        return count;
    }
    private static void CalculateCosts(Node currentNode, Node neighbour, Node targetNode)
    {
        int movementCost;

        // check if movement is diagonal
        if (neighbour.X != currentNode.X && neighbour.Y != currentNode.Y)
            movementCost = 14;
        else
            movementCost = 10;

        int newG_cost = currentNode.G_cost + movementCost;

        if (newG_cost < neighbour.G_cost)
        {
            neighbour.G_cost = newG_cost;

            int dx = Math.Abs(neighbour.X - targetNode.X);
            int dy = Math.Abs(neighbour.Y - targetNode.Y);
            int min = Math.Min(dx, dy);
            int max = Math.Max(dx, dy);

            neighbour.H_cost = 14 * min + 10 * (max - min);
            neighbour.Parent_node = currentNode;
        }
    }

    private static List<Node> ReconstructPath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();

        Node current = endNode;

        while (current != null)
        {
            path.Add(current);

            if (current == startNode)
            {
                break;
            }

            current = current.Parent_node;
        }
        path.Reverse();
        return path;
    }

    //Before using a node in a new pathfinding reset it
    // if it is already reset start the A*
    private void PrepareNodeForSearch(Node node)
    {
        if (node.SearchId == _searchId)
            return;

        node.SearchId = _searchId;
        node.G_cost = int.MaxValue;
        node.H_cost = 0;
        node.Parent_node = null;

        node.ClosedId = -1;
    }


    // Initializes the starting node with G = 0 and heuristic (H-cost)
    private static void InitializeStartNode(Node startNode, Node targetNode)
    {
        startNode.G_cost = 0;

        int dx = Math.Abs(startNode.X - targetNode.X);
        int dy = Math.Abs(startNode.Y - targetNode.Y);
        int min = Math.Min(dx, dy);
        int max = Math.Max(dx, dy);

        startNode.H_cost = 14 * min + 10 * (max - min);
    }

    // Main A* pathfinding method
    public List<Node> FindPath(int startX, int startY, int endX, int endY)
    {
        // Validate that start and end positions are inside the map
        if (!IsInsideGrid(startX, startY) || !IsInsideGrid(endX, endY))
            return new List<Node>();

        Node startNode = _grid[startX, startY];
        Node targetNode = _grid[endX, endY];

        if (!startNode.Walkable || !targetNode.Walkable)
            return new List<Node>();

        _searchId++;
        PrepareNodeForSearch(startNode);
        PrepareNodeForSearch(targetNode);

        var openQueue = new PriorityQueue<Node, int>();

        InitializeStartNode(startNode, targetNode);
        int startPriority = startNode.F_cost * 100000 + startNode.H_cost;
        openQueue.Enqueue(startNode, startPriority);

        // Safety cap to prevent pathological cases from locking the game
        int maxIterations = 5000;
        int iterations = 0;

        while (openQueue.Count > 0)
        {
            // iteration safety
            iterations++;
            if (iterations > maxIterations)
                return new List<Node>();

            Node currentNode = openQueue.Dequeue();

            if (currentNode.ClosedId == _searchId)
                continue;

            // Path found → reconstruct and return it
            if (currentNode == targetNode)
                return ReconstructPath(startNode, targetNode);

            currentNode.ClosedId = _searchId;

            int neighbourCount = GetNeighbours(currentNode);
            for (int i = 0; i < neighbourCount; i++)
            {
                Node neighbour = _neighbourBuffer[i];
                if (neighbour.ClosedId == _searchId)
                    continue;

                PrepareNodeForSearch(neighbour);

                int oldG = neighbour.G_cost;
                CalculateCosts(currentNode, neighbour, targetNode);

                // If improved, push it with its new priority
                if (neighbour.G_cost < oldG)
                {
                    int priority = neighbour.F_cost * 100000 + neighbour.H_cost;
                    openQueue.Enqueue(neighbour, priority);
                }
            }
        }

        // No path found
        return new List<Node>();
    }
}