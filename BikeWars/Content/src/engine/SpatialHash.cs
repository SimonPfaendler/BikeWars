#nullable enable

using System;
using System.Collections.Generic;
using BikeWars.Content.engine.interfaces;
using Microsoft.Xna.Framework;

namespace BikeWars.Content.engine;
public class CellData
{
    // public HashSet<CollisionLayer> Layers = new();
    public List<ICollider>? Colliders = null;
}

public class SpatialHash
{
    private readonly int _cellSize;
    private readonly int _worldWidthInCells;
    private readonly int _xOffset;
    private readonly int _yOffset;
    private int _queryId = 0;

    public Dictionary<int, CellData> _cells = new();

    public SpatialHash(int cellSize, int worldWidthInCells, int xOffset = 0, int yOffset = 0)
    {
        _cellSize = cellSize;
        _worldWidthInCells = worldWidthInCells;
        _xOffset = xOffset;
        _yOffset = yOffset;
    }

    private (int, int) ToCellCoords(Vector2 pos)
    {
        return ((int)MathF.Floor(pos.X / _cellSize),
                (int)MathF.Floor(pos.Y / _cellSize));
    }

    private int To1DKey(int x, int y)
    {
        return (y + _yOffset) * _worldWidthInCells + (x + _xOffset);
    }

    // public void Insert(ICollider c)
    // {
    //     float left   = c.Position.X;
    //     float right  = c.Position.X + c.Width;
    //     float top    = c.Position.Y;
    //     float bottom = c.Position.Y + c.Height;

    //     int minX = (int)MathF.Floor(left / _cellSize);
    //     int maxX = (int)MathF.Floor((right  - 1) / _cellSize);
    //     int minY = (int)MathF.Floor(top / _cellSize);
    //     int maxY = (int)MathF.Floor((bottom - 1) / _cellSize);


    //     for (int x = minX; x <= maxX; x++)
    //     {
    //         for (int y = minY; y <= maxY; y++)
    //         {
    //             int key = To1DKey(x, y);

    //             if (!_cells.TryGetValue(key, out var cell))
    //             {
    //                 cell = new CellData();
    //                 _cells[key] = cell;
    //             }

    //             cell.Count++;
    //             cell.Layers.Add(c.Layer); // Save layer
    //             cell.Colliders ??= new HashSet<ICollider>();
    //             cell.Colliders.Add(c);
    //         }
    //     }
    // }

    public void Insert(ICollider c)
    {
        int minX = (int)(c.Position.X / _cellSize);
        int maxX = (int)((c.Position.X + c.Width  - 1) / _cellSize);
        int minY = (int)(c.Position.Y / _cellSize);
        int maxY = (int)((c.Position.Y + c.Height - 1) / _cellSize);

        if (c.Width <= _cellSize && c.Height <= _cellSize) {
            int key = To1DKey((int)(c.Position.X / _cellSize), (int)(c.Position.Y / _cellSize));
            if (!_cells.TryGetValue(key, out var cell))
            {
                cell = new CellData();
                cell.Colliders = new List<ICollider>(8);
                _cells[key] = cell;
            }
            cell.Colliders.Add(c);
            return;
        }

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                int key = To1DKey(x, y);

                if (!_cells.TryGetValue(key, out var cell))
                {
                    cell = new CellData();
                    cell.Colliders = new List<ICollider>(8);
                    _cells[key] = cell;
                }
                cell.Colliders.Add(c);
            }
        }
    }

    public void Remove(ICollider c)
    {
        // Compute the range of cells covered by this collider (same logic as Insert)
        float left = c.Position.X;
        float right = c.Position.X + c.Width;
        float top = c.Position.Y;
        float bottom = c.Position.Y + c.Height;

        int minX = (int)MathF.Floor(left / _cellSize);
        int maxX = (int)MathF.Floor((right - 1) / _cellSize);
        int minY = (int)MathF.Floor(top / _cellSize);
        int maxY = (int)MathF.Floor((bottom - 1) / _cellSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                int key = To1DKey(x, y);

                if (_cells.TryGetValue(key, out var cell))
                {
                    if (cell.Colliders.Remove(c) && cell.Colliders.Count == 0)
                    {
                        _cells.Remove(key);
                        continue;
                    }

                    // cell.Layers.Clear();
                    // if (cell.Colliders != null)
                    // {
                    //     foreach (var col in cell.Colliders)
                    //     {
                    //         cell.Layers.Add(col.Layer);
                    //     }
                    // }
                }
            }
        }
    }

    public void QueryNearby(Vector2 pos, int radius, List<ICollider> results)
    {
        results.Clear();
        // _queryId++;

        var (cellX, cellY) = ToCellCoords(pos);
        for (int x = cellX - radius; x <= cellX + radius; x++)
        {
            for (int y = cellY - radius; y <= cellY + radius; y++)
            {
                int key = To1DKey(x, y);

                if (!_cells.TryGetValue(key, out var cell))
                    continue;

                foreach (ICollider c in cell.Colliders!)
                {
                    // if (c.LastQueryId == _queryId)
                    //     continue;

                    // c.LastQueryId = _queryId;
                    results.Add(c);
                }
            }
        }
    }

    public void Clear()
    {
        _cells.Clear();
    }
}