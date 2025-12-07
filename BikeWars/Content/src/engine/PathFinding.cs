using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using BikeWars.Content.engine;
using BikeWars.Content.engine.interfaces;

namespace BikeWars.Content.engine.PathFinding
{
    public class Node
    {
        public int X;
        public int Y;
        
        public bool Walkable;
        
        public int G_cost;
        public int H_cost;
        public int F_cost => G_cost + H_cost;
        public Node Parent_node;
        
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
        private Node[,] _grid;
        private int width;
        private int height;

        public PathFinding(Node[,] grid)
        {
            this._grid = grid;
            width = grid.GetLength(0);
            height = grid.GetLength(1);
        }

        public List<Node> GetNeighbours(Node node)
        {
            List <Node> neighbours = new List<Node>();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;
                    
                    int nx =  node.X + dx;
                    int ny =  node.Y + dy;

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;

                    Node neighbour = _grid[nx, ny];
                    
                    if (!neighbour.Walkable)
                        continue;
                    
                    bool isDiagonal = dx != 0 && dy != 0;
                    
                    if (isDiagonal)
                    {
                        Node side1 = _grid[node.X + dx, node.Y];    
                        Node side2 = _grid[node.X, node.Y + dy];    

                        // if either side is blocked, don't allow the diagonal
                        if (!side1.Walkable || !side2.Walkable)
                            continue;
                    }
                    
                    neighbours.Add(_grid[nx, ny]);  
                }
            }
            return neighbours;
        }

        private void CalculateCosts(Node currentNode, Node neighbour, Node targetNode)
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

        private Node GetLowestFCostNode(List<Node> openList)
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

        private List<Node> ReconstructPath(Node startNode, Node endNode)
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

        public List<Node> FindPath(int startX, int startY, int endX, int endY)
        {
            
            // check that start and target are inside the grid
            if (startX < 0 || startX >= width || startY < 0 || startY >= height)
                return new List<Node>();

            if (endX < 0 || endX >= width || endY < 0 || endY >= height)
                return new List<Node>();
            
            Node startNode = _grid[startX, startY];
            Node targetNode = _grid[endX, endY];
            
            // if start node or target node are not walkable then we have no path
            if (!startNode.Walkable || !targetNode.Walkable)
                return new List<Node>();
            
            // reset all node costs and parents
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Node node = _grid[x, y];
                    node.G_cost = int.MaxValue;
                    node.H_cost = 0;
                    node.Parent_node = null;
                }
            }
            
            // create the OPEN/CLOSED lists
            List<Node> openList = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            
            // initialize the start node
            startNode.G_cost = 0;
            
            int startDistX = Math.Abs(startX - endX);
            int startDistY = Math.Abs(startY - endY);
            startNode.H_cost = (startDistX + startDistY) * 10;
            
            openList.Add(startNode);
            
            // main A* loop
            while (openList.Count > 0)
            {
                Node currentNode = GetLowestFCostNode(openList);
                
                // returns the whole path from the enemy to the player
                if (currentNode == targetNode)
                    return ReconstructPath(startNode, targetNode);
                
                openList.Remove(currentNode);
                closedSet.Add(currentNode);
                
                // check all neighbours from the current node
                foreach (Node neighbour in GetNeighbours(currentNode))
                {
                    if (closedSet.Contains(neighbour))
                        continue;
                    
                    // remember the old g cost to see if it improves
                    int oldGCost = neighbour.G_cost;
                    
                    CalculateCosts(currentNode, neighbour, targetNode);

                    if (neighbour.G_cost < oldGCost && !openList.Contains(neighbour))
                    {
                        openList.Add(neighbour);
                    }
                }
            }
            return new List<Node>();
        }
    }
} 