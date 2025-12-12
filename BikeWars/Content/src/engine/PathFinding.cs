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

    public PathFinding(Node[,] grid)
    {
        _grid = grid;
        _width = grid.GetLength(0);
        _height = grid.GetLength(1);
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
    private bool IsDiagonalPassable(Node node, int dx, int dy)
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

        if (IsDiagonal(dx, dy) && !IsDiagonalPassable(node, dx, dy))
            return false;

        return true;
    }

    // gets the neighbours of a node
    public List<Node> GetNeighbours(Node node)
    {
        var neighbours = new List<Node>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (IsValidNeighbour(node, dx, dy, out int nx, out int ny))
                {
                    neighbours.Add(_grid[nx, ny]);
                }
            }
        }
        return neighbours;
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

            neighbour.H_cost = (dx + dy) * 10;
            neighbour.Parent_node = currentNode;
        }
    }

    private static Node GetLowestFCostNode(List<Node> openList)
    {
        Node best = openList[0];

        for (int i = 1; i < openList.Count; i++)
        {
            Node current = openList[i];

            if (current.F_cost < best.F_cost || current.F_cost == best.F_cost && current.H_cost < best.H_cost)
            {
                best = current;
            }
        }
        return best;
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

    // Resets G_cost, H_cost, and Parent_node for every node in the grid
    private void ResetAllNodes()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Node node = _grid[x, y];
                node.G_cost = int.MaxValue;
                node.H_cost = 0;
                node.Parent_node = null;
            }
        }
    }

    // Initializes the starting node with G = 0 and heuristic (H-cost)
    private static void InitializeStartNode(Node startNode, Node targetNode)
    {
        startNode.G_cost = 0;

        int dx = Math.Abs(startNode.X - targetNode.X);
        int dy = Math.Abs(startNode.Y - targetNode.Y);

        startNode.H_cost = (dx + dy) * 10;
    }

    // Processes all valid neighbours of the current node (updates costs and open list)
    private void ProcessNeighbours(
        Node currentNode,
        Node targetNode,
        List<Node> openList,
        HashSet<Node> closedSet)
    {
        foreach (Node neighbour in GetNeighbours(currentNode))
        {
            if (closedSet.Contains(neighbour))
                continue;

            int oldGCost = neighbour.G_cost;

            CalculateCosts(currentNode, neighbour, targetNode);

            if (neighbour.G_cost < oldGCost && !openList.Contains(neighbour))
            {
                openList.Add(neighbour);
            }
        }
    }

    // Main A* pathfinding method
    public List<Node> FindPath(int startX, int startY, int endX, int endY)
    {
        // Validate that start and end positions are inside the map
        if (!IsInsideGrid(startX, startY) || !IsInsideGrid(endX, endY))
            return new List<Node>();

        Node startNode = _grid[startX, startY];
        Node targetNode = _grid[endX, endY];

        // If either start or end is blocked, no path exists
        if (!startNode.Walkable || !targetNode.Walkable)
            return new List<Node>();

        ResetAllNodes();

        var openList = new List<Node>();
        var closedSet = new HashSet<Node>();

        InitializeStartNode(startNode, targetNode);
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openList);

            // Path found → reconstruct and return it
            if (currentNode == targetNode)
                return ReconstructPath(startNode, targetNode);

            openList.Remove(currentNode);
            closedSet.Add(currentNode);

            ProcessNeighbours(currentNode, targetNode, openList, closedSet);
        }

        // No path found
        return new List<Node>();
    }
}